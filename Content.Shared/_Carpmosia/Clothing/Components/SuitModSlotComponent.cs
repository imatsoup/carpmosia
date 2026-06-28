using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Whitelist;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used <with cref="ModdableSuitComponent"/> <and cref="SuitModComponent"> for upgrades that add item slots to a hardsuit.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SuitModSystem))]
public sealed partial class SuitModSlotComponent : Component
{

    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// The key that accompanies the itemslot. Used to get it for ejecting the item on removal of the mod.
    /// </summary>
    [DataField]
    public string[] Keys = [ "default" ];
}
