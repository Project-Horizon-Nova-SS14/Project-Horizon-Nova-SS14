using Content.Shared.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Horizon.Research;

/// <summary>
/// A storage drive of the RnD array. A blank drive fits any slot and takes the discipline of the slot it is put in;
/// once technologies are written to it, it fits only the slot of its discipline. It keeps the technologies of that
/// discipline that were researched while it was there, wears out while it works in a powered array and breaks when
/// worn out, losing everything on it. Handled by RndArraySystem.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RndDiskComponent : Component
{
    /// <summary>
    /// The discipline given by the slot the drive was put in, null while it is blank.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<TechDisciplinePrototype>? Discipline;

    [DataField, AutoNetworkedField]
    public List<ProtoId<TechnologyPrototype>> Technologies = new();

    /// <summary>
    /// How long the disk works before it breaks.
    /// </summary>
    [DataField]
    public TimeSpan Lifetime = TimeSpan.FromHours(24);

    /// <summary>
    /// How long the disk has worked already.
    /// </summary>
    [DataField]
    public TimeSpan Wear;

    [DataField, AutoNetworkedField]
    public bool Broken;
}
