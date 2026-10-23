using Robust.Shared.Serialization;

namespace Content.Shared._Horizon.Research;

/// <summary>
/// An RnD access point: put on a shuttle and linked to an RnD relay, it gives the research machines of its grid the
/// technologies of the RnD array. Handled by RndNetworkSystem.
/// </summary>
[RegisterComponent]
public sealed partial class RndAccessPointComponent : Component
{
    [ViewVariables]
    public EntityUid? Relay;
}

[Serializable, NetSerializable]
public enum RndAccessPointUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RndRelayEntry(NetEntity entity, string name, bool online, int used, int capacity)
{
    public readonly NetEntity Entity = entity;
    public readonly string Name = name;
    public readonly bool Online = online;
    public readonly int Used = used;
    public readonly int Capacity = capacity;
}

[Serializable, NetSerializable]
public sealed class RndAccessPointBoundUserInterfaceState(bool linked, NetEntity? relay, List<RndRelayEntry> relays)
    : BoundUserInterfaceState
{
    /// <summary>
    /// Whether the machines of the grid get the technologies right now.
    /// </summary>
    public readonly bool Linked = linked;

    public readonly NetEntity? Relay = relay;
    public readonly List<RndRelayEntry> Relays = relays;
}

[Serializable, NetSerializable]
public sealed class RndAccessPointConnectMessage(NetEntity relay) : BoundUserInterfaceMessage
{
    public readonly NetEntity Relay = relay;
}

[Serializable, NetSerializable]
public sealed class RndAccessPointDisconnectMessage : BoundUserInterfaceMessage;
