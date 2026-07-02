
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used to specify equipment provided by <see cref="SuitModEquipmentToggleComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SuitModSystem))]
public sealed partial class SuitModProvidedEquipmentComponent : Component
{
    [DataField]
    public bool DelteOnDrop = true;
}
