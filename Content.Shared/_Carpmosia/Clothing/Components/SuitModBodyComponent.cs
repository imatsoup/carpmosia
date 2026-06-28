using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used <with cref="ModdableSuitComponent"/> <and cref="SuitModComponent"> for upgrades that add components to a hardsuit.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SuitModSystem))]
public sealed partial class SuitModBodyComponent : Component
{

    /// <summary>
    /// Components to add to the hardsuit. For visors, <use cref= "SuitModHelmetComponent">
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public ComponentRegistry ComponentsToAdd = new();
}
