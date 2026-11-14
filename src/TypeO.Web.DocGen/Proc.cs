using System.Diagnostics;
using System.Text;

namespace TypeO.Web.DocGen;

public static class Proc
{
    public static string Run(string fileName, string arguments, string? workingDirectory = null, bool echo = true)
    {
        if (echo)
            Console.WriteLine($"> {fileName} {arguments}");

        var psi = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {fileName}");

        var output = new StringBuilder();
        process.OutputDataReceived += (_, e) => { if (e.Data != null) { output.AppendLine(e.Data); if (echo) Console.WriteLine(e.Data); } };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) { output.AppendLine(e.Data); if (echo) Console.Error.WriteLine(e.Data); } };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"'{fileName} {arguments}' failed with exit code {process.ExitCode}");

        return output.ToString();
    }
}
