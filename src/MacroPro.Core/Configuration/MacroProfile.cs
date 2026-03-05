using MacroPro.Core.Modules;

namespace MacroPro.Core.Configuration;

public sealed class MacroProfile
{
    public string ProfileName { get; set; } = "poring-world-default";
    public string AllowedProcessRoot { get; set; } = @"C:\PW";
    public string ToggleHotkey { get; set; } = "F11";
    public string PanicHotkey { get; set; } = "F12";

    public SkillSpamOptions SkillSpam { get; set; } = new();
    public TimedRefreshOptions TimedRefresh { get; set; } = new();
    public MacroChainOptions MacroChain { get; set; } = new();
    public HunterTurretOptions HunterTurret { get; set; } = new()
    {
        Enabled = false
    };
}
