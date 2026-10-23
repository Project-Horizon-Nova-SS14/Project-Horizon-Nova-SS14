using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Horizon.Research;

/// <summary>
/// A disk of the RnD array. It goes only into the slot of its discipline and keeps the technologies of that discipline
/// that were researched while it was there. It wears out while it works in a powered array and breaks when worn out,
/// losing everything on it. Handled by RndArraySystem.
/// </summary>
[RegisterComponent]
public sealed partial class RndDiskComponent : Component
{
    [DataField(required: true)]
    public ProtoId<TechDisciplinePrototype> Discipline;

    [DataField]
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

    [DataField]
    public bool Broken;
}
