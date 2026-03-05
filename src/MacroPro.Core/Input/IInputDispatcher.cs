using MacroPro.Core.Targeting;

namespace MacroPro.Core.Input;

public interface IInputDispatcher
{
    void SendKeyTap(TargetWindow target, VirtualKey key);
    void SendLeftClick(TargetWindow target);
}
