namespace MacroPro.Core.Engine;

public interface IActionModule
{
    string Name { get; }
    bool IsEnabled { get; set; }
    Task RunAsync(ModuleRuntimeContext context, CancellationToken cancellationToken);
}
