# MACRO-PRO

Safety-first Ragnarok macro project targeting **Poring World** under `C:\PW`.

## Current Status
- Project scaffold is created (`Core`, `Input.Windows`, `Profiles`, `App`, `Tests`).
- MVP shell is wired:
  - Process targeting limited by allowed root path (`C:\PW` by default).
  - Global hotkeys: `F11` toggle, `F12` panic stop.
  - Modules implemented: `SkillSpam`, `TimedRefresh`, `MacroChain`, `HunterTurret`.
  - Dedicated preset: **Hunter Turret (gl_prison)** with monster list:
    - Skeleton Prisoner
    - Zombie Prisoner
    - Whisper
    - Hunter Fly
    - Brilight
  - Target identifier system:
    - Pixel-based target presence detection (no cursor needed).
    - Optional OCR-based name matching for whitelist filtering.
  - JSON profile load/save.

## Structure
- `MacroPro.sln`
- `src/MacroPro.Core` - engine, module contracts, module implementations, target rules
- `src/MacroPro.Input.Windows` - Win32 input dispatch and process discovery
- `src/MacroPro.Profiles` - JSON profile persistence
- `src/MacroPro.App` - WinForms host app
- `tests/MacroPro.Core.Tests` - initial unit tests

## Requirements
- Windows 10/11
- .NET 8 SDK
- Optional for strict name filtering: Tesseract OCR CLI (`tesseract`) in `PATH`
  - Or set `TESSERACT_PATH` environment variable to full executable path.

## Setup
If `dotnet` is missing:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\setup-dotnet.ps1
```

Then:

```powershell
dotnet restore .\MacroPro.sln
dotnet build .\MacroPro.sln -c Release
dotnet run --project .\src\MacroPro.App\MacroPro.App.csproj
```

## Safety Defaults
- App starts OFF.
- Target process must be within configured allowed root.
- Panic stop hotkey immediately stops all running modules.
- No telemetry, no ads, no self-updater.
- Hunter preset keeps movement logic disabled (stationary/turret behavior).

## Next Milestones
1. Improve UI for macro-chain editor and multiple profiles.
2. Add process revalidation loop while running.
3. Add config schema validation and more engine tests.
4. Add release packaging script.
