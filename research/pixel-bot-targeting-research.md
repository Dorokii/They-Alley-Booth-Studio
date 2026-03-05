# Pixel Bot Targeting Research (GitHub)

## Repositories Reviewed
- `jannoelc/ro-pixel-bot`
- `jsbots/AutoFish`
- `OpenKore/openkore`

## Key Findings
1. **Color-based target detection in scan region**
   - Pattern: search for a known target color in a bounded screen/window region.
   - Source examples:
     - `_research_ro_pixel_bot/autohotkey.ahk` (`PixelSearch` command loop)
     - `_research_ro_pixel_bot/bot_scripts/sniper.js` (`searchPixel`, repeated scan + attack/click)
   - Applicability: high for Ragnarok when UI style is stable.

2. **Target-key cycling + target state check**
   - Pattern: send target key repeatedly (e.g., nearest target), then validate target state by pixel cues (target HP area) and optional text/OCR.
   - Source examples:
     - `_research_autofish/README.md` sections:
       - "Check For Players Around"
       - "Attacking/Running away"
     - Describes target key + HP pixel + optional text/recognition.
   - Applicability: high for no-cursor turret behavior.

3. **Whitelist/prioritized target selection**
   - Pattern: filter candidate targets by rules and choose best by priority/distance.
   - Source examples:
     - `_research_openkore/control/mon_control.txt` (monster-specific behavior flags)
     - `_research_openkore/src/Misc.pm` (`getBestTarget` priority + distance selection)
   - Applicability: strong long-term model for a full bot.

## What We Applied to MACRO-PRO
1. `HunterTurret` module with no-cursor target cycling (`Tab`) + attack key spam (`F1`).
2. `TargetIdentifier` subsystem:
   - Pixel target presence detection in configurable region.
   - Optional OCR name read + whitelist matching.
3. Dedicated preset for `gl_prison` with allowed monsters:
   - Skeleton Prisoner
   - Zombie Prisoner
   - Whisper
   - Hunter Fly
   - Brilight

## Current Limits
1. Pixel detection requires calibration if UI theme/resolution differs.
2. OCR matching requires Tesseract CLI installed and tuned.
3. Current target selection is "cycle + verify", not full on-screen multi-target scoring yet.

## Long-Term Evolution Path
1. Add multi-candidate scan and scoring (priority + distance + risk).
2. Add per-map target profiles and pixel calibration wizard.
3. Add confidence tracking and fallback behavior when OCR confidence is low.
4. Add anti-stuck logic for invalid/blocked targets and reacquisition states.
