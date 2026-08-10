using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Materials.OreSilo;

/// <summary>
/// Provides additional materials to linked clients across long distances.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedOreSiloSystem))]
public sealed partial class OreSiloComponent : Component
{
    /// <summary>
    /// The <see cref="OreSiloClientComponent"/> that are connected to this silo.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Clients = new();

    /// <summary>
    /// The maximum distance you can be to the silo and still receive transmission.
    /// </summary>
    /// <remarks>
    /// Default value should be big enough to span a single large department.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public float Range = 20f;

    // Carpmosia-start - Silo Adjustments
    /// <summary>
    /// Whether the silo is considered the primary, and thus ignores the above range value.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsSecondary = true;
    // Carpmosia-end - Silo Adjustments
}

[Serializable, NetSerializable]
public sealed class OreSiloBuiState : BoundUserInterfaceState
{
    public readonly HashSet<(NetEntity, string, string)> Clients;

    public OreSiloBuiState(HashSet<(NetEntity, string, string)> clients)
    {
        Clients = clients;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleOreSiloClientMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Client;

    public ToggleOreSiloClientMessage(NetEntity client)
    {
        Client = client;
    }
}

[Serializable, NetSerializable]
public enum OreSiloUiKey : byte
{
    Key
}
