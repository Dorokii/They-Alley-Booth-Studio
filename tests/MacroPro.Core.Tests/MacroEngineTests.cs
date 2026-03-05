using MacroPro.Core.Engine;
using MacroPro.Core.Input;
using MacroPro.Core.Targeting;
using Xunit;

namespace MacroPro.Core.Tests;

public sealed class MacroEngineTests
{
    [Fact]
    public async Task Engine_StartAndStop_RunsEnabledModule()
    {
        var module = new FakeModule { IsEnabled = true };
        var engine = new MacroEngine();
        engine.ConfigureModules(new[] { module });

        var target = new TargetWindow(1234, "ragexe", (nint)123, @"C:\PW\ragexe.exe");
        engine.Start(target, new FakeInput());
        await Task.Delay(60);
        await engine.StopAsync();

        Assert.True(module.IterationCount > 0);
        Assert.False(engine.IsRunning);
    }

    [Fact]
    public void ProcessRules_AllowOnlyConfiguredRoot()
    {
        var rules = new ProcessTargetRules(new[] { @"C:\PW" });

        var allowed = new TargetWindow(1, "pw", (nint)11, @"C:\PW\game\ragexe.exe");
        var denied = new TargetWindow(2, "other", (nint)12, @"C:\Games\other.exe");

        Assert.True(rules.IsAllowed(allowed, out _));
        Assert.False(rules.IsAllowed(denied, out _));
    }

    private sealed class FakeModule : IActionModule
    {
        public string Name => "fake";
        public bool IsEnabled { get; set; }
        public int IterationCount { get; private set; }

        public async Task RunAsync(ModuleRuntimeContext context, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                IterationCount++;
                await Task.Delay(10, cancellationToken);
            }
        }
    }

    private sealed class FakeInput : IInputDispatcher
    {
        public void SendKeyTap(TargetWindow target, VirtualKey key)
        {
        }

        public void SendLeftClick(TargetWindow target)
        {
        }
    }
}
