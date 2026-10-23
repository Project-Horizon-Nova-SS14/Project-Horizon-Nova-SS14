using Content.Shared.Containers.ItemSlots;
using Content.Shared.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Horizon.Research;

/// <summary>
/// The RnD array: the one research server of the game. Each slot holds the disk of one discipline, and the
/// technologies of a discipline exist only while a working disk is in its slot. The rack can be opened to change
/// disks only while it is unlocked. The slots are set up by RndArraySlotSystem, the rest is handled by RndArraySystem.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class RndArrayComponent : Component
{
    [DataField(required: true)]
    public List<RndArraySlot> Slots = new();

    /// <summary>
    /// Whether the rack is open, so disks can be put in and taken out.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Open;

    /// <summary>
    /// How often the disks wear out while the array is powered.
    /// </summary>
    [DataField]
    public TimeSpan WearInterval = TimeSpan.FromSeconds(1);

    [DataField, AutoPausedField]
    public TimeSpan NextWear;

    /// <summary>
    /// How often the disk condition shown in the interface is refreshed.
    /// </summary>
    [DataField]
    public TimeSpan DisplayInterval = TimeSpan.FromSeconds(30);

    [DataField, AutoPausedField]
    public TimeSpan NextDisplay;

    /// <summary>
    /// The shown disk condition is rounded to this many percent.
    /// </summary>
    [DataField]
    public int DisplayStep = 5;

    /// <summary>
    /// The disk condition as it was at the last refresh, by slot id. Slots without a disk are missing.
    /// </summary>
    [ViewVariables]
    public Dictionary<string, int> DisplayedCondition = new();
}

/// <summary>
/// A disk slot of the RnD array, specialized on one discipline.
/// </summary>
[DataDefinition]
public sealed partial class RndArraySlot
{
    [DataField(required: true)]
    public string Id = string.Empty;

    [DataField(required: true)]
    public ProtoId<TechDisciplinePrototype> Discipline;

    [DataField(required: true)]
    public ItemSlot Slot = new();

    /// <summary>
    /// The sprite layer shown while a disk is in the slot.
    /// </summary>
    [DataField]
    public string? DiskLayer;

    /// <summary>
    /// The sprite layer of the disk light, shown while the disk works and the array is powered.
    /// </summary>
    [DataField]
    public string? LedLayer;
}

/// <summary>
/// Bit masks over the slots of the RnD array, the first slot being the lowest bit.
/// </summary>
[Serializable, NetSerializable]
public enum RndArrayVisuals : byte
{
    /// <summary>
    /// The slots with a disk in them.
    /// </summary>
    Disks,

    /// <summary>
    /// The slots whose disk light is on.
    /// </summary>
    Leds,
}

/// <summary>
/// The lamp of a disk slot, by the disk condition.
/// </summary>
[Serializable, NetSerializable]
public enum RndDiskLamp : byte
{
    Off,
    Red,
    Orange,
    Yellow,
    Green,
}

[Serializable, NetSerializable]
public enum RndArrayUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RndArraySlotState(
    string id,
    ProtoId<TechDisciplinePrototype> discipline,
    bool hasDisk,
    bool broken,
    int condition,
    RndDiskLamp lamp,
    int technologies)
{
    public readonly string Id = id;
    public readonly ProtoId<TechDisciplinePrototype> Discipline = discipline;
    public readonly bool HasDisk = hasDisk;
    public readonly bool Broken = broken;

    /// <summary>
    /// The rounded condition in percent, as shown with "~".
    /// </summary>
    public readonly int Condition = condition;

    public readonly RndDiskLamp Lamp = lamp;
    public readonly int Technologies = technologies;
}

[Serializable, NetSerializable]
public sealed class RndArrayBoundUserInterfaceState(
    bool locked,
    bool open,
    int points,
    int clients,
    List<RndArraySlotState> slots) : BoundUserInterfaceState
{
    public readonly bool Locked = locked;
    public readonly bool Open = open;
    public readonly int Points = points;
    public readonly int Clients = clients;
    public readonly List<RndArraySlotState> Slots = slots;
}

[Serializable, NetSerializable]
public sealed class RndArrayToggleOpenMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class RndArrayEjectMessage(string slot) : BoundUserInterfaceMessage
{
    public readonly string Slot = slot;
}
