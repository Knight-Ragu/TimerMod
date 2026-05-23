using System.IO;

namespace TimerMod;

public partial class Timer
{
    public override void OnInitializeMelon()
    {
        if (!Directory.Exists(Timer.DataFolder))
            Directory.CreateDirectory(Timer.DataFolder);
        
        if (!Directory.Exists(Timer.TimesFolder))
            Directory.CreateDirectory(Timer.TimesFolder);
        
        if (!File.Exists(Timer.SeedFile))
            ReadWrite.CreateNewSeedFile(Timer.SeedFile);
    }
}
