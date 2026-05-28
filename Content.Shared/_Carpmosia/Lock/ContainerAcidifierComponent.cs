
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Lock;

/// <summary>
/// Determines whether a container's contents should acidify on being broken open.
/// </summary>
/// <remarks>
/// Requires <c></c>.
/// </remarks>
[RegisterComponent]
[Access(typeof(LockSystem))]
public sealed partial class ContainerAcidifierComponent : Component;
