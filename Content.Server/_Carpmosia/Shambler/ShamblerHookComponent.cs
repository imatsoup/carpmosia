using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Server.Shambler;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShamblerHookComponent : Component
{
    public float TargetStopDistance = 1.3f;

    public float MinimumHookDistance = 0.5f;

    public EntProtoId HookProto = "ShamblerHook";
}
