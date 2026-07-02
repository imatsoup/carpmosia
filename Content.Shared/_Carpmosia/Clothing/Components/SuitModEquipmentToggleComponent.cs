using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used <with cref="ModdableSuitComponent"/> <and cref="SuitModComponent"> for upgrades that provide new actions to players.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SuitModSystem))]
public sealed partial class SuitModEquipmentToggleComponent : Component
{

    /// <summary>
    /// The action prototype for deploying equipment.
    /// </summary>
    [DataField]
    public EntProtoId Action = "ActionDeployEquipment";


    [DataField]
    public EntProtoId? SpawnedPrototype;

    /// <summary>
    /// Entity to hold the action prototype.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    ///  Entity after prototy has spawned, used to QueueDel
    /// </summary>
    [DataField]
    public EntityUid? Equipment;

    [DataField]
    public float RequiredHands = 1f;


}
