using System.Runtime.InteropServices;
using System.Drawing;
using MacroPro.Core.Configuration;
using MacroPro.Core.Engine;
using MacroPro.Core.Input;
using MacroPro.Core.Modules;
using MacroPro.Core.Targeting;
using MacroPro.Input.Windows;
using MacroPro.Profiles;

namespace MacroPro.App;

public sealed class MainForm : Form
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_TOGGLE_ID = 1001;
    private const int HOTKEY_PANIC_ID = 1002;
    private const uint MOD_NOREPEAT = 0x4000;

    private readonly MacroEngine _engine = new();
    private readonly IInputDispatcher _inputDispatcher = new WindowMessageInputDispatcher();
    private readonly ProfileStore _profileStore;
    private readonly string _profileName = "poring-world";

    private MacroProfile _profile = new();
    private ProcessTargetRules _targetRules = new(new[] { @"C:\PW" });
    private bool _isTransitioning;

    private readonly TextBox _allowedRootText = new();
    private readonly ComboBox _processCombo = new();
    private readonly Button _refreshButton = new();
    private readonly Button _startStopButton = new();
    private readonly Button _saveButton = new();
    private readonly Label _statusLabel = new();
    private readonly TextBox _logText = new();

    private readonly CheckBox _skillEnabled = new();
    private readonly ComboBox _skillKey = new();
    private readonly NumericUpDown _skillInterval = new();
    private readonly NumericUpDown _skillJitter = new();

    private readonly CheckBox _refreshEnabled = new();
    private readonly ComboBox _refreshKey = new();
    private readonly NumericUpDown _refreshIntervalSeconds = new();

    private readonly CheckBox _chainEnabled = new();
    private readonly NumericUpDown _chainCycleDelay = new();
    private readonly TextBox _chainStepsText = new();

    private readonly CheckBox _hunterEnabled = new();
    private readonly ComboBox _hunterSkillKey = new();
    private readonly ComboBox _hunterTargetKey = new();
    private readonly NumericUpDown _hunterAttackInterval = new();
    private readonly NumericUpDown _hunterAcquireInterval = new();
    private readonly CheckBox _hunterUseOcr = new();
    private readonly CheckBox _hunterRequireNameMatch = new();
    private readonly TextBox _hunterWhitelistText = new();
    private readonly Button _applyHunterPresetButton = new();

    public MainForm()
    {
        Text = "MacroPro - Poring World";
        Width = 980;
        Height = 900;
        AutoScroll = true;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        _profileStore = new ProfileStore(Path.Combine(AppContext.BaseDirectory, "profiles"));

        BuildUi();
        WireEvents();
        SetEngineStatus(false);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _profile = await _profileStore.LoadOrCreateAsync(_profileName);
        EnsureProfileDefaults();
        ApplyProfileToUi();
        RefreshTargetList();
        RegisterHotkeys();

        Log($"Loaded profile '{_profileName}'.");
        Log($"Allowed root: {_profile.AllowedProcessRoot}");
        Log($"Hotkeys: Toggle={_profile.ToggleHotkey}, Panic={_profile.PanicHotkey}");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        UnregisterHotkeys();

        if (_engine.IsRunning)
        {
            _engine.StopAsync().GetAwaiter().GetResult();
        }

        base.OnFormClosing(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            var id = m.WParam.ToInt32();
            if (id == HOTKEY_PANIC_ID)
            {
                _ = PanicStopAsync();
                return;
            }

            if (id == HOTKEY_TOGGLE_ID)
            {
                _ = ToggleEngineAsync();
                return;
            }
        }

        base.WndProc(ref m);
    }

    private void BuildUi()
    {
        var row = 16;

        var allowedRootLabel = new Label
        {
            Text = "Allowed Root Path",
            Left = 16,
            Top = row + 6,
            Width = 140
        };
        Controls.Add(allowedRootLabel);

        _allowedRootText.Left = 160;
        _allowedRootText.Top = row;
        _allowedRootText.Width = 320;
        Controls.Add(_allowedRootText);

        _refreshButton.Text = "Refresh Processes";
        _refreshButton.Left = 500;
        _refreshButton.Top = row - 1;
        _refreshButton.Width = 140;
        Controls.Add(_refreshButton);

        row += 42;

        var processLabel = new Label
        {
            Text = "Target Process",
            Left = 16,
            Top = row + 6,
            Width = 140
        };
        Controls.Add(processLabel);

        _processCombo.Left = 160;
        _processCombo.Top = row;
        _processCombo.Width = 480;
        _processCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(_processCombo);

        _startStopButton.Text = "START";
        _startStopButton.Left = 660;
        _startStopButton.Top = row - 1;
        _startStopButton.Width = 130;
        _startStopButton.Height = 30;
        Controls.Add(_startStopButton);

        row += 44;

        _statusLabel.Text = "Status: OFF";
        _statusLabel.Left = 16;
        _statusLabel.Top = row + 6;
        _statusLabel.Width = 480;
        Controls.Add(_statusLabel);

        _saveButton.Text = "Save Profile";
        _saveButton.Left = 660;
        _saveButton.Top = row - 1;
        _saveButton.Width = 130;
        _saveButton.Height = 30;
        Controls.Add(_saveButton);

        row += 50;

        var groupSkill = new GroupBox
        {
            Text = "Skill Spam",
            Left = 16,
            Top = row,
            Width = 460,
            Height = 140
        };
        Controls.Add(groupSkill);

        _skillEnabled.Text = "Enabled";
        _skillEnabled.Left = 14;
        _skillEnabled.Top = 28;
        _skillEnabled.Width = 100;
        groupSkill.Controls.Add(_skillEnabled);

        var skillKeyLabel = new Label { Text = "Skill Key", Left = 14, Top = 60, Width = 70 };
        groupSkill.Controls.Add(skillKeyLabel);
        _skillKey.Left = 90;
        _skillKey.Top = 56;
        _skillKey.Width = 100;
        _skillKey.DropDownStyle = ComboBoxStyle.DropDownList;
        groupSkill.Controls.Add(_skillKey);

        var skillIntervalLabel = new Label { Text = "Interval ms", Left = 210, Top = 60, Width = 80 };
        groupSkill.Controls.Add(skillIntervalLabel);
        _skillInterval.Left = 295;
        _skillInterval.Top = 56;
        _skillInterval.Width = 80;
        _skillInterval.Minimum = 40;
        _skillInterval.Maximum = 5000;
        groupSkill.Controls.Add(_skillInterval);

        var skillJitterLabel = new Label { Text = "Jitter ms", Left = 14, Top = 92, Width = 70 };
        groupSkill.Controls.Add(skillJitterLabel);
        _skillJitter.Left = 90;
        _skillJitter.Top = 88;
        _skillJitter.Width = 80;
        _skillJitter.Minimum = 0;
        _skillJitter.Maximum = 2000;
        groupSkill.Controls.Add(_skillJitter);

        var groupRefresh = new GroupBox
        {
            Text = "Timed Refresh",
            Left = 500,
            Top = row,
            Width = 430,
            Height = 140
        };
        Controls.Add(groupRefresh);

        _refreshEnabled.Text = "Enabled";
        _refreshEnabled.Left = 14;
        _refreshEnabled.Top = 28;
        _refreshEnabled.Width = 100;
        groupRefresh.Controls.Add(_refreshEnabled);

        var refreshKeyLabel = new Label { Text = "Refresh Key", Left = 14, Top = 60, Width = 80 };
        groupRefresh.Controls.Add(refreshKeyLabel);
        _refreshKey.Left = 98;
        _refreshKey.Top = 56;
        _refreshKey.Width = 100;
        _refreshKey.DropDownStyle = ComboBoxStyle.DropDownList;
        groupRefresh.Controls.Add(_refreshKey);

        var refreshIntervalLabel = new Label { Text = "Interval sec", Left = 214, Top = 60, Width = 80 };
        groupRefresh.Controls.Add(refreshIntervalLabel);
        _refreshIntervalSeconds.Left = 300;
        _refreshIntervalSeconds.Top = 56;
        _refreshIntervalSeconds.Width = 80;
        _refreshIntervalSeconds.Minimum = 1;
        _refreshIntervalSeconds.Maximum = 3600;
        groupRefresh.Controls.Add(_refreshIntervalSeconds);

        row += 160;

        var groupChain = new GroupBox
        {
            Text = "Macro Chain",
            Left = 16,
            Top = row,
            Width = 914,
            Height = 180
        };
        Controls.Add(groupChain);

        _chainEnabled.Text = "Enabled";
        _chainEnabled.Left = 14;
        _chainEnabled.Top = 28;
        _chainEnabled.Width = 100;
        groupChain.Controls.Add(_chainEnabled);

        var chainCycleLabel = new Label { Text = "Cycle Delay ms", Left = 136, Top = 30, Width = 90 };
        groupChain.Controls.Add(chainCycleLabel);
        _chainCycleDelay.Left = 232;
        _chainCycleDelay.Top = 26;
        _chainCycleDelay.Width = 80;
        _chainCycleDelay.Minimum = 80;
        _chainCycleDelay.Maximum = 5000;
        groupChain.Controls.Add(_chainCycleDelay);

        var chainStepsLabel = new Label
        {
            Text = "Steps (one per line): KEY:F1:120 | CLICK:80 | WAIT:250",
            Left = 14,
            Top = 64,
            Width = 520
        };
        groupChain.Controls.Add(chainStepsLabel);

        _chainStepsText.Left = 14;
        _chainStepsText.Top = 86;
        _chainStepsText.Width = 886;
        _chainStepsText.Height = 80;
        _chainStepsText.Multiline = true;
        _chainStepsText.ScrollBars = ScrollBars.Vertical;
        groupChain.Controls.Add(_chainStepsText);

        row += 196;

        var groupHunter = new GroupBox
        {
            Text = "Hunter Turret (gl_prison)",
            Left = 16,
            Top = row,
            Width = 914,
            Height = 190
        };
        Controls.Add(groupHunter);

        _hunterEnabled.Text = "Enabled";
        _hunterEnabled.Left = 14;
        _hunterEnabled.Top = 28;
        _hunterEnabled.Width = 90;
        groupHunter.Controls.Add(_hunterEnabled);

        _applyHunterPresetButton.Text = "Apply gl_prison Preset";
        _applyHunterPresetButton.Left = 120;
        _applyHunterPresetButton.Top = 24;
        _applyHunterPresetButton.Width = 170;
        _applyHunterPresetButton.Height = 28;
        groupHunter.Controls.Add(_applyHunterPresetButton);

        var hunterSkillLabel = new Label { Text = "DS Key", Left = 14, Top = 66, Width = 70 };
        groupHunter.Controls.Add(hunterSkillLabel);
        _hunterSkillKey.Left = 90;
        _hunterSkillKey.Top = 62;
        _hunterSkillKey.Width = 90;
        _hunterSkillKey.DropDownStyle = ComboBoxStyle.DropDownList;
        groupHunter.Controls.Add(_hunterSkillKey);

        var hunterTargetKeyLabel = new Label { Text = "Target Key", Left = 196, Top = 66, Width = 75 };
        groupHunter.Controls.Add(hunterTargetKeyLabel);
        _hunterTargetKey.Left = 276;
        _hunterTargetKey.Top = 62;
        _hunterTargetKey.Width = 90;
        _hunterTargetKey.DropDownStyle = ComboBoxStyle.DropDownList;
        groupHunter.Controls.Add(_hunterTargetKey);

        var hunterAttackIntervalLabel = new Label { Text = "Attack ms", Left = 382, Top = 66, Width = 70 };
        groupHunter.Controls.Add(hunterAttackIntervalLabel);
        _hunterAttackInterval.Left = 456;
        _hunterAttackInterval.Top = 62;
        _hunterAttackInterval.Width = 90;
        _hunterAttackInterval.Minimum = 40;
        _hunterAttackInterval.Maximum = 2000;
        groupHunter.Controls.Add(_hunterAttackInterval);

        var hunterAcquireLabel = new Label { Text = "Acquire ms", Left = 562, Top = 66, Width = 70 };
        groupHunter.Controls.Add(hunterAcquireLabel);
        _hunterAcquireInterval.Left = 636;
        _hunterAcquireInterval.Top = 62;
        _hunterAcquireInterval.Width = 90;
        _hunterAcquireInterval.Minimum = 60;
        _hunterAcquireInterval.Maximum = 2000;
        groupHunter.Controls.Add(_hunterAcquireInterval);

        _hunterUseOcr.Text = "Use OCR Name Read";
        _hunterUseOcr.Left = 14;
        _hunterUseOcr.Top = 100;
        _hunterUseOcr.Width = 150;
        groupHunter.Controls.Add(_hunterUseOcr);

        _hunterRequireNameMatch.Text = "Require Name Match";
        _hunterRequireNameMatch.Left = 176;
        _hunterRequireNameMatch.Top = 100;
        _hunterRequireNameMatch.Width = 150;
        groupHunter.Controls.Add(_hunterRequireNameMatch);

        var hunterWhitelistLabel = new Label
        {
            Text = "Allowed Monsters (one per line or comma-separated)",
            Left = 14,
            Top = 130,
            Width = 320
        };
        groupHunter.Controls.Add(hunterWhitelistLabel);

        _hunterWhitelistText.Left = 340;
        _hunterWhitelistText.Top = 128;
        _hunterWhitelistText.Width = 560;
        _hunterWhitelistText.Height = 46;
        _hunterWhitelistText.Multiline = true;
        _hunterWhitelistText.ScrollBars = ScrollBars.Vertical;
        groupHunter.Controls.Add(_hunterWhitelistText);

        row += 206;

        var hotkeyInfo = new Label
        {
            Left = 16,
            Top = row,
            Width = 920,
            Height = 24,
            Text = "Global Hotkeys: Toggle F11, Panic Stop F12 (loaded from profile)"
        };
        Controls.Add(hotkeyInfo);

        row += 30;

        _logText.Left = 16;
        _logText.Top = row;
        _logText.Width = 914;
        _logText.Height = 300;
        _logText.Multiline = true;
        _logText.ReadOnly = true;
        _logText.ScrollBars = ScrollBars.Vertical;
        Controls.Add(_logText);

        PopulateVirtualKeyCombo(_skillKey, VirtualKey.F1);
        PopulateVirtualKeyCombo(_refreshKey, VirtualKey.F5);
        PopulateVirtualKeyCombo(_hunterSkillKey, VirtualKey.F1);
        PopulateVirtualKeyCombo(_hunterTargetKey, VirtualKey.Tab);
    }

    private void WireEvents()
    {
        _refreshButton.Click += (_, _) => RefreshTargetList();
        _startStopButton.Click += async (_, _) => await ToggleEngineAsync();
        _saveButton.Click += async (_, _) => await SaveProfileAsync();
        _allowedRootText.Leave += (_, _) => RefreshTargetList();
        _applyHunterPresetButton.Click += (_, _) => ApplyHunterPreset();
    }

    private async Task ToggleEngineAsync()
    {
        if (_isTransitioning)
        {
            return;
        }

        _isTransitioning = true;
        try
        {
            if (_engine.IsRunning)
            {
                await StopEngineAsync();
            }
            else
            {
                await StartEngineAsync();
            }
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private async Task StartEngineAsync()
    {
        ApplyUiToProfile();

        if (_processCombo.SelectedItem is not ProcessItem selected)
        {
            Log("Select a target process before starting.");
            return;
        }

        if (!_targetRules.IsAllowed(selected.Target, out var reason))
        {
            Log($"Start blocked: {reason}");
            return;
        }

        ITargetNameReader nameReader = _profile.HunterTurret.Identifier.UseNameOcr
            ? new TesseractCliTargetNameReader(_profile.HunterTurret.Identifier.TargetNameRegion)
            : new NoopTargetNameReader();

        if (_profile.HunterTurret.Identifier.RequireNameMatch && !_profile.HunterTurret.Identifier.UseNameOcr)
        {
            Log("Warning: Require Name Match is ON but OCR is OFF. Target matching may reject all names.");
        }

        var targetIdentifier = new PixelTargetIdentifier(_profile.HunterTurret.Identifier, nameReader);

        var modules = new IActionModule[]
        {
            new SkillSpamModule(_profile.SkillSpam),
            new TimedRefreshModule(_profile.TimedRefresh),
            new MacroChainModule(_profile.MacroChain),
            new HunterTurretModule(_profile.HunterTurret, targetIdentifier)
        };

        _engine.ConfigureModules(modules);
        _engine.Start(selected.Target, _inputDispatcher, Log);

        SetEngineStatus(true);
        Log($"Started on {selected.Target.ProcessName} (PID {selected.Target.ProcessId}).");
        await Task.CompletedTask;
    }

    private async Task StopEngineAsync()
    {
        await _engine.StopAsync();
        SetEngineStatus(false);
        Log("Stopped.");
    }

    private async Task PanicStopAsync()
    {
        await StopEngineAsync();
        Log("PANIC STOP triggered.");
    }

    private async Task SaveProfileAsync()
    {
        ApplyUiToProfile();
        await _profileStore.SaveAsync(_profileName, _profile);
        Log("Profile saved.");
    }

    private void RefreshTargetList()
    {
        var allowedRoot = _allowedRootText.Text.Trim();
        _targetRules = new ProcessTargetRules(new[] { allowedRoot });

        var targets = ProcessTargetDiscovery.GetRunningTargets();
        var allowed = targets
            .Where(target => _targetRules.IsAllowed(target, out _))
            .Select(target => new ProcessItem(target))
            .ToList();

        _processCombo.BeginUpdate();
        _processCombo.Items.Clear();
        foreach (var item in allowed)
        {
            _processCombo.Items.Add(item);
        }
        _processCombo.EndUpdate();

        if (_processCombo.Items.Count > 0)
        {
            _processCombo.SelectedIndex = 0;
        }

        Log($"Process refresh complete. Allowed targets: {_processCombo.Items.Count}");
    }

    private void ApplyProfileToUi()
    {
        EnsureProfileDefaults();
        _allowedRootText.Text = _profile.AllowedProcessRoot;

        _skillEnabled.Checked = _profile.SkillSpam.Enabled;
        SelectComboValue(_skillKey, _profile.SkillSpam.Key.ToString(), VirtualKey.F1.ToString());
        _skillInterval.Value = Math.Clamp(_profile.SkillSpam.IntervalMs, (int)_skillInterval.Minimum, (int)_skillInterval.Maximum);
        _skillJitter.Value = Math.Clamp(_profile.SkillSpam.JitterMs, (int)_skillJitter.Minimum, (int)_skillJitter.Maximum);

        _refreshEnabled.Checked = _profile.TimedRefresh.Enabled;
        SelectComboValue(_refreshKey, _profile.TimedRefresh.Key.ToString(), VirtualKey.F5.ToString());
        _refreshIntervalSeconds.Value = Math.Clamp(_profile.TimedRefresh.IntervalSeconds, (int)_refreshIntervalSeconds.Minimum, (int)_refreshIntervalSeconds.Maximum);

        _chainEnabled.Checked = _profile.MacroChain.Enabled;
        _chainCycleDelay.Value = Math.Clamp(_profile.MacroChain.CycleDelayMs, (int)_chainCycleDelay.Minimum, (int)_chainCycleDelay.Maximum);
        _chainStepsText.Text = FormatMacroSteps(_profile.MacroChain.Steps);

        _hunterEnabled.Checked = _profile.HunterTurret.Enabled;
        SelectComboValue(_hunterSkillKey, _profile.HunterTurret.AttackSkillKey.ToString(), VirtualKey.F1.ToString());
        SelectComboValue(_hunterTargetKey, _profile.HunterTurret.TargetCycleKey.ToString(), VirtualKey.Tab.ToString());
        _hunterAttackInterval.Value = Math.Clamp(_profile.HunterTurret.AttackIntervalMs, (int)_hunterAttackInterval.Minimum, (int)_hunterAttackInterval.Maximum);
        _hunterAcquireInterval.Value = Math.Clamp(_profile.HunterTurret.AcquireIntervalMs, (int)_hunterAcquireInterval.Minimum, (int)_hunterAcquireInterval.Maximum);
        _hunterUseOcr.Checked = _profile.HunterTurret.Identifier.UseNameOcr;
        _hunterRequireNameMatch.Checked = _profile.HunterTurret.Identifier.RequireNameMatch;
        _hunterWhitelistText.Text = string.Join(Environment.NewLine, _profile.HunterTurret.Identifier.AllowedMonsterNames);
    }

    private void ApplyUiToProfile()
    {
        EnsureProfileDefaults();
        _profile.AllowedProcessRoot = string.IsNullOrWhiteSpace(_allowedRootText.Text)
            ? @"C:\PW"
            : _allowedRootText.Text.Trim();

        _profile.SkillSpam.Enabled = _skillEnabled.Checked;
        _profile.SkillSpam.Key = ParseVirtualKey(_skillKey.SelectedItem?.ToString(), VirtualKey.F1);
        _profile.SkillSpam.IntervalMs = (int)_skillInterval.Value;
        _profile.SkillSpam.JitterMs = (int)_skillJitter.Value;

        _profile.TimedRefresh.Enabled = _refreshEnabled.Checked;
        _profile.TimedRefresh.Key = ParseVirtualKey(_refreshKey.SelectedItem?.ToString(), VirtualKey.F5);
        _profile.TimedRefresh.IntervalSeconds = (int)_refreshIntervalSeconds.Value;

        _profile.MacroChain.Enabled = _chainEnabled.Checked;
        _profile.MacroChain.CycleDelayMs = (int)_chainCycleDelay.Value;
        _profile.MacroChain.Steps = ParseMacroSteps(_chainStepsText.Text);

        _profile.HunterTurret.Enabled = _hunterEnabled.Checked;
        _profile.HunterTurret.AttackSkillKey = ParseVirtualKey(_hunterSkillKey.SelectedItem?.ToString(), VirtualKey.F1);
        _profile.HunterTurret.TargetCycleKey = ParseVirtualKey(_hunterTargetKey.SelectedItem?.ToString(), VirtualKey.Tab);
        _profile.HunterTurret.AttackIntervalMs = (int)_hunterAttackInterval.Value;
        _profile.HunterTurret.AcquireIntervalMs = (int)_hunterAcquireInterval.Value;
        _profile.HunterTurret.Identifier.UseNameOcr = _hunterUseOcr.Checked;
        _profile.HunterTurret.Identifier.RequireNameMatch = _hunterRequireNameMatch.Checked;
        _profile.HunterTurret.Identifier.AllowedMonsterNames = ParseMonsterList(_hunterWhitelistText.Text);
    }

    private void EnsureProfileDefaults()
    {
        _profile.SkillSpam ??= new SkillSpamOptions();
        _profile.TimedRefresh ??= new TimedRefreshOptions();
        _profile.MacroChain ??= new MacroChainOptions();
        _profile.MacroChain.Steps ??= new List<MacroStep>();
        _profile.HunterTurret ??= new HunterTurretOptions();
        _profile.HunterTurret.Identifier ??= new TargetIdentifierOptions();
        _profile.HunterTurret.Identifier.AllowedMonsterNames ??= new List<string>();
        _profile.AllowedProcessRoot = string.IsNullOrWhiteSpace(_profile.AllowedProcessRoot) ? @"C:\PW" : _profile.AllowedProcessRoot;
    }

    private void SetEngineStatus(bool on)
    {
        _statusLabel.Text = on ? "Status: ON" : "Status: OFF";
        _statusLabel.ForeColor = on ? Color.DarkGreen : Color.DarkRed;
        _startStopButton.Text = on ? "STOP" : "START";
        _startStopButton.BackColor = on ? Color.IndianRed : Color.LightGreen;
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<string>(Log), message);
            return;
        }

        _logText.AppendText($"{message}{Environment.NewLine}");
    }

    private void RegisterHotkeys()
    {
        UnregisterHotkeys();

        var toggle = ParseVirtualKey(_profile.ToggleHotkey, VirtualKey.F11);
        var panic = ParseVirtualKey(_profile.PanicHotkey, VirtualKey.F12);

        if (!RegisterHotKey(Handle, HOTKEY_TOGGLE_ID, MOD_NOREPEAT, (uint)toggle))
        {
            Log("Warning: could not register toggle hotkey.");
        }

        if (!RegisterHotKey(Handle, HOTKEY_PANIC_ID, MOD_NOREPEAT, (uint)panic))
        {
            Log("Warning: could not register panic hotkey.");
        }
    }

    private void UnregisterHotkeys()
    {
        if (IsHandleCreated)
        {
            UnregisterHotKey(Handle, HOTKEY_TOGGLE_ID);
            UnregisterHotKey(Handle, HOTKEY_PANIC_ID);
        }
    }

    private static void PopulateVirtualKeyCombo(ComboBox combo, VirtualKey defaultKey)
    {
        combo.Items.Clear();
        foreach (var key in Enum.GetValues<VirtualKey>())
        {
            if (key == VirtualKey.None)
            {
                continue;
            }

            combo.Items.Add(key.ToString());
        }

        combo.SelectedItem = defaultKey.ToString();
    }

    private static VirtualKey ParseVirtualKey(string? raw, VirtualKey fallback)
    {
        return Enum.TryParse<VirtualKey>(raw, ignoreCase: true, out var parsed)
            ? parsed
            : fallback;
    }

    private static void SelectComboValue(ComboBox combo, string requested, string fallback)
    {
        if (combo.Items.Contains(requested))
        {
            combo.SelectedItem = requested;
            return;
        }

        combo.SelectedItem = fallback;
    }

    private void ApplyHunterPreset()
    {
        EnsureProfileDefaults();
        _profile.HunterTurret = HunterPresets.CreateGlPrisonTurret();
        _profile.SkillSpam.Enabled = false;
        _profile.TimedRefresh.Enabled = false;
        _profile.MacroChain.Enabled = false;
        _profile.AllowedProcessRoot = @"C:\PW";

        ApplyProfileToUi();
        RefreshTargetList();
        Log("Applied Hunter Turret preset for gl_prison.");
    }

    private static List<string> ParseMonsterList(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new List<string>();
        }

        var separators = new[] { ',', '\r', '\n', '\t' };
        return raw.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(static token => token.Trim())
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private List<MacroStep> ParseMacroSteps(string raw)
    {
        var steps = new List<MacroStep>();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return steps;
        }

        var lines = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var originalLine in lines)
        {
            var line = originalLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries)
                .Select(static part => part.Trim())
                .ToArray();
            if (parts.Length == 0)
            {
                continue;
            }

            var token = parts[0].ToUpperInvariant();
            if (token is "KEY" or "KEYTAP")
            {
                var key = parts.Length > 1 ? ParseVirtualKey(parts[1], VirtualKey.None) : VirtualKey.None;
                if (key == VirtualKey.None)
                {
                    Log($"MacroChain: skipped invalid key step '{line}'.");
                    continue;
                }

                var delay = ParsePositiveIntOrDefault(parts.Length > 2 ? parts[2] : null, 80, 30);
                steps.Add(new MacroStep
                {
                    Action = MacroStepAction.KeyTap,
                    Key = key,
                    DelayAfterMs = delay
                });
                continue;
            }

            if (token is "CLICK" or "LEFTCLICK")
            {
                var delay = ParsePositiveIntOrDefault(parts.Length > 1 ? parts[1] : null, 80, 30);
                steps.Add(new MacroStep
                {
                    Action = MacroStepAction.LeftClick,
                    Key = VirtualKey.None,
                    DelayAfterMs = delay
                });
                continue;
            }

            if (token == "WAIT")
            {
                var delay = ParsePositiveIntOrDefault(parts.Length > 1 ? parts[1] : null, 80, 30);
                steps.Add(new MacroStep
                {
                    Action = MacroStepAction.Wait,
                    Key = VirtualKey.None,
                    DelayAfterMs = delay
                });
                continue;
            }

            Log($"MacroChain: skipped unknown step '{line}'.");
        }

        return steps;
    }

    private static string FormatMacroSteps(IReadOnlyList<MacroStep> steps)
    {
        if (steps.Count == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>(steps.Count);
        foreach (var step in steps)
        {
            lines.Add(step.Action switch
            {
                MacroStepAction.KeyTap when step.Key != VirtualKey.None => $"KEY:{step.Key}:{Math.Max(30, step.DelayAfterMs)}",
                MacroStepAction.LeftClick => $"CLICK:{Math.Max(30, step.DelayAfterMs)}",
                MacroStepAction.Wait => $"WAIT:{Math.Max(30, step.DelayAfterMs)}",
                _ => $"WAIT:{Math.Max(30, step.DelayAfterMs)}"
            });
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static int ParsePositiveIntOrDefault(string? raw, int fallback, int minimum)
    {
        if (!int.TryParse(raw, out var value))
        {
            value = fallback;
        }

        return Math.Max(minimum, value);
    }

    private sealed class ProcessItem
    {
        public ProcessItem(TargetWindow target)
        {
            Target = target;
        }

        public TargetWindow Target { get; }

        public override string ToString()
        {
            return $"{Target.ProcessName} (PID {Target.ProcessId}) - {Path.GetFileName(Target.ExecutablePath)}";
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);
}
