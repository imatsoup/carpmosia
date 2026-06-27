using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used with <see cref="ModdableSuitComponent"/> and <see cref="SuitModComponent"> for upgrades that have custom behavior or add new components.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SuitModSystem))]
public sealed partial class SuitModBodyComponent : Component
{

    [DataField]
    [AlwaysPushInheritance]
    public ComponentRegistry ComponentsToAdd = new();
}
