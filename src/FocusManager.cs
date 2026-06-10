using System.Diagnostics;

namespace Pomu;

static class FocusManager
{
    const string ShortcutsBinary = "/usr/bin/shortcuts";
    const string ShortcutPrefix = "Pomu ";
    const string OnSuffix = " On";
    const string OffSuffix = " Off";
    const int ListTimeoutMs = 3000;

    public static List<string> ListFocusNames()
    {
        var names = new List<string>();
        string output = RunShortcuts("list", waitForOutput: true);
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var all = new HashSet<string>(lines);

        foreach (var line in lines)
        {
            if (!line.StartsWith(ShortcutPrefix) || !line.EndsWith(OnSuffix)) continue;
            string baseName = line[..^OnSuffix.Length];
            if (!all.Contains(baseName + OffSuffix)) continue;
            names.Add(baseName[ShortcutPrefix.Length..]);
        }

        names.Sort();
        return names;
    }

    public static void EnableFocus(string focusName) =>
        RunShortcutNamed(ShortcutPrefix + focusName + OnSuffix);

    public static void DisableFocus(string focusName) =>
        RunShortcutNamed(ShortcutPrefix + focusName + OffSuffix);

    static void RunShortcutNamed(string shortcutName)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ShortcutsBinary,
                ArgumentList = { "run", shortcutName },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
        }
        catch
        {
        }
    }

    static string RunShortcuts(string command, bool waitForOutput)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = ShortcutsBinary,
                ArgumentList = { command },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (process == null) return string.Empty;
            string output = process.StandardOutput.ReadToEnd();
            if (!process.WaitForExit(ListTimeoutMs))
            {
                process.Kill();
                return string.Empty;
            }
            return output;
        }
        catch
        {
            return string.Empty;
        }
    }
}
