using MacroPro.Core.Input;
using MacroPro.Core.Modules;

namespace MacroPro.Core.Configuration;

public static class HunterPresets
{
    public static HunterTurretOptions CreateGlPrisonTurret()
    {
        return new HunterTurretOptions
        {
            Enabled = true,
            MapName = "gl_prison",
            AttackSkillKey = VirtualKey.F1,
            TargetCycleKey = VirtualKey.Tab,
            AttackIntervalMs = 145,
            AcquireIntervalMs = 110,
            LostTargetCooldownMs = 180,
            Identifier = new()
            {
                Enabled = true,
                UseNameOcr = false,
                RequireNameMatch = false,
                TargetBarColorHex = "#5BC236",
                ColorTolerance = 35,
                TargetBarSearchRegion = new()
                {
                    X = 0.34,
                    Y = 0.02,
                    Width = 0.32,
                    Height = 0.08
                },
                TargetNameRegion = new()
                {
                    X = 0.38,
                    Y = 0.01,
                    Width = 0.24,
                    Height = 0.04
                },
                AllowedMonsterNames =
                {
                    "skeleton prisoner",
                    "zombie prisoner",
                    "whisper",
                    "hunter fly",
                    "brilight"
                }
            }
        };
    }
}
