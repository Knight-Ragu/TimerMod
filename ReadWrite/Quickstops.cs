using System;
using System.IO;
using System.Linq;

namespace TimerMod;

public partial class ReadWrite
{
    internal static void CreateNewQuickstopsFile(string QuickstopsFile)
        => File.WriteAllLines(QuickstopsFile, [
            "// File for setting your quickstop preferences, which will be used when restarting with 'Ctrl+Shift+R'",
            "// Valid options are 'Enable', 'Disable', and 'Any'",
            "// The third and fifth settings correspond to the first and second quickstops on Quarry/Storm Drain",
            "// To set all quickstops at once, just put a single setting",
            "Any",
            "Any",
            "Enable",
            "Any",
            "Enable",
            "Any",
            "Any",
            "Any"
        ]);

    internal static Quickstop[] ReadQuickstopsFile()
    {
        string[] lines = File.ReadAllLines(Timer.QuickstopsFile);
        Quickstop[] quickstops = [.. Enumerable.Repeat(Quickstop.Any, 8)];

 
        int quickstop = 0;
        for (int line = 0; line < Math.Max(lines.Length, quickstops.Length); line++)
        {
            if (ParseQuickstopSetting(lines[Math.Min(line, lines.Length - 1)]) is not Quickstop setting) continue;
            
            quickstops[quickstop] = setting;
            quickstop++;
        }

        return quickstops;
    }

    private static Quickstop? ParseQuickstopSetting(string line)
    {
        return line switch
        {
            "Enable" => Quickstop.Enable,
            "Disable" => Quickstop.Disable,
            "Any" => Quickstop.Any,
            _ => null,
        };
    }
}
