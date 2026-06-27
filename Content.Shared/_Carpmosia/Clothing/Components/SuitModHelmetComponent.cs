using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used with <see cref="ModdableSuitComponent"/> and <see cref="SuitModComponent"> for upgrades that add new components to hardsuit helmets.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SuitModSystem))]
public sealed partial class SuitModHelmetComponent : Component
{

    [DataField]
    [AlwaysPushInheritance]
    public ComponentRegistry ComponentsToAdd = new();
}
