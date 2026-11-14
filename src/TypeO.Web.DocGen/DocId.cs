using System.Reflection;
using System.Text;

namespace TypeO.Web.DocGen;

/// <summary>
/// Computes XML documentation ids (without the "T:"/"M:" prefix) matching the compiler's
/// GenerateDocumentationFile output, so reflection data can be joined with doc comments.
/// </summary>
public static class DocId
{
    public static string ForType(Type type) => type.FullName!.Replace('+', '.');

    public static string ForMethod(Type type, MethodBase method)
    {
        var sb = new StringBuilder(ForType(type));
        sb.Append('.');
        sb.Append(method.IsConstructor
            ? method.IsStatic ? "#cctor" : "#ctor"
            : method.Name.Replace('.', '#')); // explicit interface implementations

        if (method.IsGenericMethod)
            sb.Append("``").Append(method.GetGenericArguments().Length);

        AppendParameters(sb, method.GetParameters());

        // Conversion operators encode the return type.
        if (method is MethodInfo { IsSpecialName: true } mi && mi.Name is "op_Implicit" or "op_Explicit")
            sb.Append('~').Append(Encode(mi.ReturnType));

        return sb.ToString();
    }

    public static string ForProperty(Type type, PropertyInfo property)
    {
        var sb = new StringBuilder(ForType(type));
        sb.Append('.').Append(property.Name.Replace('.', '#'));
        AppendParameters(sb, property.GetIndexParameters());
        return sb.ToString();
    }

    private static void AppendParameters(StringBuilder sb, ParameterInfo[] parameters)
    {
        if (parameters.Length == 0)
            return;
        sb.Append('(');
        sb.Append(string.Join(",", parameters.Select(p => Encode(p.ParameterType))));
        sb.Append(')');
    }

    private static string Encode(Type type)
    {
        if (type.IsByRef)
            return Encode(type.GetElementType()!) + "@";
        if (type.IsPointer)
            return Encode(type.GetElementType()!) + "*";
        if (type.IsArray)
        {
            var rank = type.GetArrayRank();
            var suffix = rank == 1 ? "[]" : "[" + string.Join(",", Enumerable.Repeat("0:", rank)) + "]";
            return Encode(type.GetElementType()!) + suffix;
        }
        if (type.IsGenericParameter)
            return (type.DeclaringMethod != null ? "``" : "`") + type.GenericParameterPosition;
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            var definitionName = type.GetGenericTypeDefinition().FullName!.Replace('+', '.');
            var backtick = definitionName.IndexOf('`');
            if (backtick >= 0)
                definitionName = definitionName[..backtick];
            var args = type.GetGenericArguments().Select(Encode);
            return $"{definitionName}{{{string.Join(",", args)}}}";
        }
        return type.FullName!.Replace('+', '.');
    }
}

/// <summary>Formats reflection types as friendly C# type names.</summary>
public static class TypeName
{
    private static readonly Dictionary<string, string> Keywords = new()
    {
        ["System.Void"] = "void",
        ["System.Object"] = "object",
        ["System.String"] = "string",
        ["System.Boolean"] = "bool",
        ["System.Char"] = "char",
        ["System.Byte"] = "byte",
        ["System.SByte"] = "sbyte",
        ["System.Int16"] = "short",
        ["System.UInt16"] = "ushort",
        ["System.Int32"] = "int",
        ["System.UInt32"] = "uint",
        ["System.Int64"] = "long",
        ["System.UInt64"] = "ulong",
        ["System.Single"] = "float",
        ["System.Double"] = "double",
        ["System.Decimal"] = "decimal",
        ["System.IntPtr"] = "nint",
        ["System.UIntPtr"] = "nuint",
    };

    /// <summary>Friendly name without namespace, e.g. "List&lt;string&gt;" or "Grid&lt;T&gt;".</summary>
    public static string Friendly(Type type)
    {
        if (type.IsByRef)
            return Friendly(type.GetElementType()!);
        if (type.IsPointer)
            return Friendly(type.GetElementType()!) + "*";
        if (type.IsArray)
        {
            var rank = type.GetArrayRank();
            return Friendly(type.GetElementType()!) + (rank == 1 ? "[]" : "[" + new string(',', rank - 1) + "]");
        }
        if (type.IsGenericParameter)
            return type.Name;

        if (type.IsGenericType)
        {
            // Nullable<T> -> T?
            if (type.GetGenericTypeDefinition().FullName == "System.Nullable`1")
                return Friendly(type.GetGenericArguments()[0]) + "?";

            var name = type.Name;
            var backtick = name.IndexOf('`');
            if (backtick >= 0)
                name = name[..backtick];
            var args = type.GetGenericArguments().Select(Friendly);
            return $"{name}<{string.Join(", ", args)}>";
        }

        if (type.FullName != null && Keywords.TryGetValue(type.FullName, out var keyword))
            return keyword;

        return type.Name;
    }

    /// <summary>Short declaration name of a (possibly generic) type definition, e.g. "Grid&lt;T&gt;".</summary>
    public static string Short(Type type) => Friendly(type);
}
