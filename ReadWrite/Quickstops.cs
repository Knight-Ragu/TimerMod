using System.IO;
using System.Linq;

namespace TimerMod;

public partial class ReadWrite
{
    internal static void CreateNewQuickstopsFile(string QuickstopsFile)
        => File.WriteAllLines(QuickstopsFile, [
            "// File for setting your quickstop preferences, valid settings are 'Enable', 'Disable', and 'Ignore'.",
            "// The third and fifth ones are the first and second quickstops on quarry/storm drain",
            "Ignore",
            "Ignore",
            "Enable",
            "Ignore",
            "Enable",
            "Ignore",
            "Ignore",
            "Ignore"
        ]);

    internal static Quickstop[] ReadQuickstopsFile()
    {
        string[] lines = File.ReadAllLines(Timer.QuickstopsFile);
        Quickstop[] quickstops = Enumerable.Repeat(Quickstop.Ignore, lines.Length).ToArray();

        int i = 0;
        foreach (var line in lines)
        {
            switch (line)
            {
                case "Enable":
                    quickstops[i] = Quickstop.Enable;
                    i++;
                break;

                case "Disable":
                    quickstops[i] = Quickstop.Disable;
                    i++;
                break;

                case "Ignore":
                    quickstops[i] = Quickstop.Ignore;
                    i++;
                break;
            }

            Timer.Log.Msg($"{i}: '{line}'");
        }

        return quickstops;
    }
}
