using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used to denote compatibility with <see cref="UpgradeableSuitComponent"/>. Does not contain explicit behavior.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SuitModSystem))]
public sealed partial class SuitModComponent : Component
{
    /// <summary>
    /// Tags used to ensure mutually exclusive upgrades and duplicates are not stacked.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> Tags = new();

    [DataField]
    [AlwaysPushInheritance]
    public ComponentRegistry? ToAdd = new();

    public bool HelmUpgrade = false;

    /// <summary>
    /// Markup added to the suit on examine to display the upgrades.
    /// </summary>
    [DataField]
    public LocId ExamineText;

    [DataField]
    public bool TargetDeployable;
}
