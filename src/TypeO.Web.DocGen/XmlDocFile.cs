using System.Text;
using System.Xml;

namespace TypeO.Web.DocGen;

public class XmlMemberDoc
{
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public string? Returns { get; set; }
    public Dictionary<string, string> Parameters { get; } = new();
}

/// <summary>
/// Parses a compiler-generated XML documentation file (GenerateDocumentationFile)
/// into a lookup of documentation id (e.g. "M:TypeD.Models.ModuleModel.Add(System.String)") to doc text.
/// </summary>
public class XmlDocFile
{
    private readonly Dictionary<string, XmlMemberDoc> members = new();

    public static XmlDocFile Load(string? path)
    {
        var result = new XmlDocFile();
        if (path == null || !File.Exists(path))
            return result;

        var doc = new XmlDocument();
        doc.Load(path);

        foreach (XmlElement member in doc.SelectNodes("/doc/members/member")!)
        {
            var id = member.GetAttribute("name");
            if (string.IsNullOrEmpty(id))
                continue;

            var entry = new XmlMemberDoc
            {
                Summary = FlattenElement(member.SelectSingleNode("summary")),
                Remarks = FlattenElement(member.SelectSingleNode("remarks")),
                Returns = FlattenElement(member.SelectSingleNode("returns")),
            };

            foreach (XmlElement param in member.SelectNodes("param")!)
            {
                var name = param.GetAttribute("name");
                var text = FlattenElement(param);
                if (!string.IsNullOrEmpty(name) && text != null)
                    entry.Parameters[name] = text;
            }

            result.members[id] = entry;
        }

        return result;
    }

    public XmlMemberDoc? Find(string id) => members.TryGetValue(id, out var doc) ? doc : null;

    /// <summary>Flattens doc XML to plain text, resolving see/paramref/c style tags.</summary>
    private static string? FlattenElement(XmlNode? node)
    {
        if (node == null)
            return null;

        var sb = new StringBuilder();
        Flatten(node, sb);
        var text = NormalizeWhitespace(sb.ToString());
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static void Flatten(XmlNode node, StringBuilder sb)
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child)
            {
                case XmlComment:
                    break;
                case XmlCharacterData text:
                    sb.Append(text.Value);
                    break;
                case XmlElement element when element.Name is "see" or "seealso":
                    sb.Append(CrefDisplayName(element.GetAttribute("cref"), element.GetAttribute("langword"), element.InnerText));
                    break;
                case XmlElement element when element.Name is "paramref" or "typeparamref":
                    sb.Append(element.GetAttribute("name"));
                    break;
                case XmlElement element when element.Name is "para":
                    Flatten(element, sb);
                    sb.Append('\n');
                    break;
                case XmlElement element:
                    Flatten(element, sb);
                    break;
            }
        }
    }

    private static string CrefDisplayName(string cref, string langword, string innerText)
    {
        if (!string.IsNullOrEmpty(innerText))
            return innerText;
        if (!string.IsNullOrEmpty(langword))
            return langword;
        if (string.IsNullOrEmpty(cref))
            return "";

        // "M:TypeD.Models.ModuleModel.Add(System.String)" -> "ModuleModel.Add"
        var name = cref;
        var colon = name.IndexOf(':');
        if (colon >= 0)
            name = name[(colon + 1)..];
        var paren = name.IndexOf('(');
        if (paren >= 0)
            name = name[..paren];

        var parts = name.Split('.');
        return parts.Length >= 2 ? $"{parts[^2]}.{parts[^1]}" : name;
    }

    private static string NormalizeWhitespace(string text)
    {
        var lines = text.Split('\n').Select(l => l.Trim());
        return string.Join("\n", lines).Trim();
    }
}
