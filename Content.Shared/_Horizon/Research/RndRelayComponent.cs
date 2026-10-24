using Robust.Shared.Serialization;

namespace Content.Shared._Horizon.Research;

/// <summary>
/// An RnD relay: wirelessly links access points to the RnD array, wherever they are. Every linked access point takes
/// one channel, and when all are taken no more access points can link. Relays of higher levels have more channels.
/// Handled by RndNetworkSystem.
/// </summary>
[RegisterComponent]
public sealed partial class RndRelayComponent : Component
{
    [DataField]
    public int Level = 1;

    [DataField]
    public int Capacity = 5;

    [ViewVariables]
    public List<EntityUid> AccessPoints = new();
}

[Serializable, NetSerializable]
public enum RndRelayUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RndNetworkDeviceEntry(NetEntity entity, string name, bool online)
{
    public readonly NetEntity Entity = entity;
    public readonly string Name = name;
    public readonly bool Online = online;
}

[Serializable, NetSerializable]
public sealed class RndRelayBoundUserInterfaceState(
    bool online,
    int level,
    int capacity,
    List<RndNetworkDeviceEntry> accessPoints) : BoundUserInterfaceState
{
    /// <summary>
    /// Whether the relay works and the RnD array exists.
    /// </summary>
    public readonly bool Online = online;

    public readonly int Level = level;

    public readonly int Capacity = capacity;
    public readonly List<RndNetworkDeviceEntry> AccessPoints = accessPoints;
}

/// <summary>
/// Unlinks an access point from the relay.
/// </summary>
[Serializable, NetSerializable]
public sealed class RndRelayDisconnectMessage(NetEntity accessPoint) : BoundUserInterfaceMessage
{
    public readonly NetEntity AccessPoint = accessPoint;
}
