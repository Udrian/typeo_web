using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using TypeO.Web.Docs;

namespace TypeO.Web.DocGen;

/// <summary>
/// Extracts the public API of a built assembly via <see cref="MetadataLoadContext"/> and merges it
/// with the compiler-generated XML documentation file into <see cref="ProjectDoc"/> data.
/// </summary>
public static class ApiExtractor
{
    public static ProjectDoc Extract(string assemblyPath, string? xmlDocPath, string kind, IEnumerable<string> extraProbeDirs)
    {
        var xmlDoc = XmlDocFile.Load(xmlDocPath);

        var resolverPaths = BuildResolverPaths(assemblyPath, extraProbeDirs);
        using var context = new MetadataLoadContext(new PathAssemblyResolver(resolverPaths));
        var assembly = context.LoadFromAssemblyPath(assemblyPath);

        var project = new ProjectDoc
        {
            Name = Path.GetFileNameWithoutExtension(assemblyPath),
            Kind = kind,
        };

        var namespaces = new SortedDictionary<string, NamespaceDoc>(StringComparer.Ordinal);

        foreach (var type in assembly.GetTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            try
            {
                if (!IsVisibleType(type) || IsCompilerGenerated(type))
                    continue;

                var typeDoc = ExtractType(type, xmlDoc);
                var ns = type.Namespace ?? "(global)";
                if (!namespaces.TryGetValue(ns, out var nsDoc))
                    namespaces[ns] = nsDoc = new NamespaceDoc { Name = ns };
                nsDoc.Types.Add(typeDoc);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"warning: skipping type {type.FullName}: {ex.Message}");
            }
        }

        project.Namespaces = namespaces.Values.ToList();
        return project;
    }

    private static List<string> BuildResolverPaths(string assemblyPath, IEnumerable<string> extraProbeDirs)
    {
        // First hit wins per simple name: the module's own bin folders first
        // (they contain all NuGet/module dependencies), then the runtime for framework assemblies.
        var dirs = new List<string> { Path.GetDirectoryName(Path.GetFullPath(assemblyPath))! };
        dirs.AddRange(extraProbeDirs);
        dirs.Add(RuntimeEnvironment.GetRuntimeDirectory());

        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dir in dirs.Where(Directory.Exists))
        {
            foreach (var dll in Directory.EnumerateFiles(dir, "*.dll"))
            {
                var name = Path.GetFileNameWithoutExtension(dll);
                if (!seen.ContainsKey(name))
                    seen[name] = dll;
            }
        }
        return seen.Values.ToList();
    }

    private static bool IsVisibleType(Type type) =>
        type.IsPublic || type.IsNestedPublic || type.IsNestedFamily || type.IsNestedFamORAssem;

    private static bool IsCompilerGenerated(MemberInfo member) =>
        member.Name.Contains('<') ||
        member.GetCustomAttributesData().Any(a =>
            a.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");

    private static TypeDoc ExtractType(Type type, XmlDocFile xmlDoc)
    {
        var id = DocId.ForType(type);
        var doc = xmlDoc.Find("T:" + id);

        var typeDoc = new TypeDoc
        {
            Id = id,
            Name = TypeName.Short(type),
            Namespace = type.Namespace ?? "(global)",
            Kind = TypeKind(type),
            Signature = TypeSignature(type),
            Summary = doc?.Summary,
            Remarks = doc?.Remarks,
        };

        if (type.IsEnum)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.IsLiteral)
                    continue;
                var fieldDoc = xmlDoc.Find($"F:{id}.{field.Name}");
                typeDoc.Members.Add(new MemberDoc
                {
                    Id = $"{id}.{field.Name}",
                    Name = field.Name,
                    Kind = "EnumValue",
                    Signature = $"{field.Name} = {field.GetRawConstantValue()}",
                    Summary = fieldDoc?.Summary,
                });
            }
            return typeDoc;
        }

        if (IsDelegate(type))
            return typeDoc;

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var ctor in type.GetConstructors(flags).Where(IsVisibleMethod))
            AddMember(typeDoc, ExtractMethod(type, ctor, xmlDoc));

        foreach (var property in type.GetProperties(flags)
                     .Where(p => (p.GetMethod != null && IsVisibleMethod(p.GetMethod)) ||
                                 (p.SetMethod != null && IsVisibleMethod(p.SetMethod))))
            AddMember(typeDoc, ExtractProperty(type, property, xmlDoc));

        foreach (var method in type.GetMethods(flags)
                     .Where(m => IsVisibleMethod(m) && !m.IsSpecialName && !IsCompilerGenerated(m)))
            AddMember(typeDoc, ExtractMethod(type, method, xmlDoc));

        foreach (var method in type.GetMethods(flags)
                     .Where(m => IsVisibleMethod(m) && m.IsSpecialName && m.Name.StartsWith("op_", StringComparison.Ordinal)))
            AddMember(typeDoc, ExtractMethod(type, method, xmlDoc));

        foreach (var evt in type.GetEvents(flags)
                     .Where(e => e.AddMethod != null && IsVisibleMethod(e.AddMethod)))
            AddMember(typeDoc, ExtractEvent(type, evt, xmlDoc));

        foreach (var field in type.GetFields(flags)
                     .Where(f => (f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly) && !IsCompilerGenerated(f)))
            AddMember(typeDoc, ExtractField(type, field, xmlDoc));

        return typeDoc;
    }

    private static void AddMember(TypeDoc typeDoc, MemberDoc? member)
    {
        if (member != null)
            typeDoc.Members.Add(member);
    }

    private static bool IsVisibleMethod(MethodBase method) =>
        method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;

    private static MemberDoc? ExtractMethod(Type type, MethodBase method, XmlDocFile xmlDoc)
    {
        try
        {
            var id = DocId.ForMethod(type, method);
            var doc = xmlDoc.Find("M:" + id);
            var member = new MemberDoc
            {
                Id = id,
                Name = method.IsConstructor ? TypeName.Short(type) : MethodDisplayName(method),
                Kind = method.IsConstructor ? "Constructor" :
                       method.Name.StartsWith("op_", StringComparison.Ordinal) ? "Operator" : "Method",
                Signature = MethodSignature(method),
                Summary = doc?.Summary,
                Remarks = doc?.Remarks,
                Returns = doc?.Returns,
            };
            foreach (var parameter in method.GetParameters())
            {
                member.Parameters.Add(new ParameterDoc
                {
                    Name = parameter.Name ?? "",
                    Type = TypeName.Friendly(parameter.ParameterType),
                    Description = parameter.Name != null && doc != null && doc.Parameters.TryGetValue(parameter.Name, out var d) ? d : null,
                });
            }
            return member;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"warning: skipping member {type.FullName}.{method.Name}: {ex.Message}");
            return null;
        }
    }

    private static MemberDoc? ExtractProperty(Type type, PropertyInfo property, XmlDocFile xmlDoc)
    {
        try
        {
            var id = DocId.ForProperty(type, property);
            var doc = xmlDoc.Find("P:" + id);
            return new MemberDoc
            {
                Id = id,
                Name = property.GetIndexParameters().Length > 0 ? "this[]" : property.Name,
                Kind = "Property",
                Signature = PropertySignature(property),
                Summary = doc?.Summary,
                Remarks = doc?.Remarks,
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"warning: skipping member {type.FullName}.{property.Name}: {ex.Message}");
            return null;
        }
    }

    private static MemberDoc? ExtractEvent(Type type, EventInfo evt, XmlDocFile xmlDoc)
    {
        try
        {
            var id = $"{DocId.ForType(type)}.{evt.Name}";
            var doc = xmlDoc.Find("E:" + id);
            return new MemberDoc
            {
                Id = id,
                Name = evt.Name,
                Kind = "Event",
                Signature = $"{Accessibility(evt.AddMethod!)} event {TypeName.Friendly(evt.EventHandlerType!)} {evt.Name}",
                Summary = doc?.Summary,
                Remarks = doc?.Remarks,
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"warning: skipping member {type.FullName}.{evt.Name}: {ex.Message}");
            return null;
        }
    }

    private static MemberDoc? ExtractField(Type type, FieldInfo field, XmlDocFile xmlDoc)
    {
        try
        {
            var id = $"{DocId.ForType(type)}.{field.Name}";
            var doc = xmlDoc.Find("F:" + id);

            var sb = new StringBuilder();
            sb.Append(field.IsPublic ? "public" : "protected");
            if (field.IsLiteral)
                sb.Append(" const");
            else if (field.IsStatic)
                sb.Append(" static");
            if (field.IsInitOnly)
                sb.Append(" readonly");
            sb.Append($" {TypeName.Friendly(field.FieldType)} {field.Name}");
            if (field.IsLiteral)
                sb.Append($" = {FormatConstant(field.GetRawConstantValue())}");

            return new MemberDoc
            {
                Id = id,
                Name = field.Name,
                Kind = "Field",
                Signature = sb.ToString(),
                Summary = doc?.Summary,
                Remarks = doc?.Remarks,
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"warning: skipping member {type.FullName}.{field.Name}: {ex.Message}");
            return null;
        }
    }

    private static string FormatConstant(object? value) => value switch
    {
        null => "null",
        string s => $"\"{s}\"",
        bool b => b ? "true" : "false",
        _ => value.ToString() ?? "",
    };

    private static string TypeKind(Type type) =>
        type.IsEnum ? "Enum" :
        IsDelegate(type) ? "Delegate" :
        type.IsInterface ? "Interface" :
        type.IsValueType ? "Struct" : "Class";

    private static bool IsDelegate(Type type) =>
        type.BaseType?.FullName is "System.MulticastDelegate" or "System.Delegate";

    private static string TypeSignature(Type type)
    {
        var sb = new StringBuilder("public ");

        if (IsDelegate(type))
        {
            var invoke = type.GetMethod("Invoke")!;
            sb.Append($"delegate {TypeName.Friendly(invoke.ReturnType)} {TypeName.Short(type)}");
            sb.Append($"({string.Join(", ", invoke.GetParameters().Select(FormatParameter))})");
            return sb.ToString();
        }

        if (type.IsEnum)
            return sb.Append($"enum {TypeName.Short(type)}").ToString();

        if (type.IsInterface)
        {
            sb.Append("interface ");
        }
        else if (type.IsValueType)
        {
            sb.Append("struct ");
        }
        else
        {
            if (type.IsAbstract && type.IsSealed)
                sb.Append("static ");
            else if (type.IsAbstract)
                sb.Append("abstract ");
            else if (type.IsSealed)
                sb.Append("sealed ");
            sb.Append("class ");
        }

        sb.Append(TypeName.Short(type));

        var bases = new List<string>();
        if (type.BaseType != null && !type.IsValueType &&
            type.BaseType.FullName != "System.Object")
            bases.Add(TypeName.Friendly(type.BaseType));

        var baseInterfaces = new HashSet<Type>(type.BaseType?.GetInterfaces() ?? Type.EmptyTypes);
        foreach (var iface in type.GetInterfaces())
        {
            if (!baseInterfaces.Contains(iface) && (iface.IsPublic || iface.IsNestedPublic))
                bases.Add(TypeName.Friendly(iface));
        }

        if (bases.Count > 0)
            sb.Append(" : ").Append(string.Join(", ", bases));

        return sb.ToString();
    }

    private static string MethodDisplayName(MethodBase method)
    {
        var name = method.Name;
        if (method.IsGenericMethod)
            name += $"<{string.Join(", ", method.GetGenericArguments().Select(TypeName.Friendly))}>";
        return name;
    }

    private static string MethodSignature(MethodBase method)
    {
        var sb = new StringBuilder(Accessibility(method));
        sb.Append(' ');

        if (method.IsStatic)
            sb.Append("static ");
        if (method is MethodInfo { IsAbstract: true } && !method.DeclaringType!.IsInterface)
            sb.Append("abstract ");
        else if (method is MethodInfo { IsVirtual: true } m2 && !method.DeclaringType!.IsInterface)
            sb.Append((m2.Attributes & System.Reflection.MethodAttributes.NewSlot) == 0 ? "override " : "virtual ");

        if (method is MethodInfo methodInfo)
        {
            sb.Append(TypeName.Friendly(methodInfo.ReturnType)).Append(' ');
            sb.Append(MethodDisplayName(method));
        }
        else
        {
            sb.Append(TypeName.Short(method.DeclaringType!));
        }

        sb.Append($"({string.Join(", ", method.GetParameters().Select(FormatParameter))})");
        return sb.ToString();
    }

    private static string PropertySignature(PropertyInfo property)
    {
        var getter = property.GetMethod != null && IsVisibleMethod(property.GetMethod) ? property.GetMethod : null;
        var setter = property.SetMethod != null && IsVisibleMethod(property.SetMethod) ? property.SetMethod : null;
        var main = getter ?? setter!;

        var sb = new StringBuilder(Accessibility(main));
        sb.Append(' ');
        if (main.IsStatic)
            sb.Append("static ");
        sb.Append(TypeName.Friendly(property.PropertyType)).Append(' ');

        var indexParameters = property.GetIndexParameters();
        if (indexParameters.Length > 0)
            sb.Append($"this[{string.Join(", ", indexParameters.Select(FormatParameter))}]");
        else
            sb.Append(property.Name);

        sb.Append(" {");
        if (getter != null)
            sb.Append(" get;");
        if (setter != null)
        {
            if (getter != null && Accessibility(setter) != Accessibility(getter))
                sb.Append($" {Accessibility(setter)} set;");
            else
                sb.Append(" set;");
        }
        sb.Append(" }");
        return sb.ToString();
    }

    private static string FormatParameter(ParameterInfo parameter)
    {
        var sb = new StringBuilder();
        var type = parameter.ParameterType;
        if (type.IsByRef)
        {
            sb.Append(parameter.IsOut ? "out " : parameter.IsIn ? "in " : "ref ");
            type = type.GetElementType()!;
        }
        else if (parameter.GetCustomAttributesData().Any(a => a.AttributeType.FullName == "System.ParamArrayAttribute"))
        {
            sb.Append("params ");
        }

        sb.Append(TypeName.Friendly(type)).Append(' ').Append(parameter.Name);

        if (parameter.HasDefaultValue)
            sb.Append($" = {FormatConstant(parameter.RawDefaultValue)}");

        return sb.ToString();
    }

    private static string Accessibility(MethodBase method) =>
        method.IsPublic ? "public" : "protected";
}
