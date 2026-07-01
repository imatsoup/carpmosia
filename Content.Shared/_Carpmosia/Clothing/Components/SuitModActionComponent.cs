using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used <with cref="ModdableSuitComponent"/> <and cref="SuitModComponent"> for upgrades that provide new actions to players.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SuitModSystem))]
public sealed partial class SuitModActionComponent : Component
{

    [DataField(required: true), AutoNetworkedField, AlwaysPushInheritance]
    public List<EntProtoId> Actions = new();

    [DataField]
    public List<EntityUid>? ActionEntities;
}
