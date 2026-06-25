using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used with <see cref="ModdableSuitComponent"/> and <see cref="SuitModComponent"> for upgrades that affect movement speed.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SuitModSystem))]
public sealed partial class SuitModSpeedComponent : Component
{

    [DataField]
    public float Modifier = 0.2f;
}
