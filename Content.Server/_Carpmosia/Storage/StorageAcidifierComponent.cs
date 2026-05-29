
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Server.Storage.EntitySystems;

/// <summary>
/// Pops up with a message when a container is destroyed. Intended for use with secure containers.
/// </summary>
/// <remarks>
/// Requires <c>EntityStorageComponent</c>.
/// </remarks>
[RegisterComponent]
[Access(typeof(StorageAcidifierSystem))]
public sealed partial class StorageAcidifierComponent : Component
{
    [DataField]
    public LocId Msg = "crate-acidifier-popup";
}
