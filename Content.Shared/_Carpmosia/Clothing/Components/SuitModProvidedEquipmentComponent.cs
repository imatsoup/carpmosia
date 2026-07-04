
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Used to specify equipment provided by <see cref="SuitModEquipmentToggleComponent"/>.
/// Relevant equipment should also come with the Unremovable component to prevent goofy situations.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SuitModSystem))]
public sealed partial class SuitModProvidedEquipmentComponent : Component;
