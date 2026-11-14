using TypeO.Web.DocGen;

if (args.Length == 0)
    return Usage();

var options = ParseOptions(args.Skip(1));

try
{
    switch (args[0])
    {
        case "extract":
        {
            var repo = Require(options, "repo");
            var output = Require(options, "output");
            var documentation = RepoExtractor.Extract(
                Path.GetFullPath(repo),
                options.GetValueOrDefault("version"),
                options.GetValueOrDefault("config", "Release"));
            RepoExtractor.Write(documentation, Path.GetFullPath(output));
            return 0;
        }
        case "update":
        {
            var pipeline = new UpdatePipeline
            {
                ModulesJsonPath = Path.GetFullPath(Require(options, "modules")),
                OutputDir = Path.GetFullPath(Require(options, "output")),
                WorkDir = Path.GetFullPath(options.GetValueOrDefault("work", Path.Combine(Path.GetTempPath(), "typeo-docgen"))),
                ModuleFilter = options.GetValueOrDefault("module"),
                PythonExecutable = options.GetValueOrDefault("python", "python"),
                KeepWork = options.ContainsKey("keep-work"),
            };
            return pipeline.Run();
        }
        default:
            return Usage();
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    return 1;
}

static int Usage()
{
    Console.WriteLine("""
        TypeO.Web.DocGen - generates API documentation JSON for TypeO modules.

        Usage:
          docgen extract --repo <path> --output <docs-data-dir> [--version <version>] [--config Release]
              Extracts documentation from an already generated and built module repository.

          docgen update --modules <modules.json> --output <docs-data-dir>
                        [--work <dir>] [--module <name>] [--python <exe>] [--keep-work]
              Lists release tags (x.y.z) of every module in modules.json and, for each tag that has
              no documentation yet: clones the tag, runs 'typer generate', builds with
              GenerateDocumentationFile and writes <docs-data-dir>/<Module>/<version>.json.
        """);
    return 2;
}

static Dictionary<string, string> ParseOptions(IEnumerable<string> args)
{
    var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    string? key = null;
    foreach (var arg in args)
    {
        if (arg.StartsWith("--", StringComparison.Ordinal))
        {
            if (key != null)
                options[key] = "";
            key = arg[2..];
        }
        else if (key != null)
        {
            options[key] = arg;
            key = null;
        }
        else
        {
            throw new ArgumentException($"Unexpected argument '{arg}'");
        }
    }
    if (key != null)
        options[key] = "";
    return options;
}

static string Require(Dictionary<string, string> options, string name) =>
    options.TryGetValue(name, out var value) && value != ""
        ? value
        : throw new ArgumentException($"Missing required option --{name}");
