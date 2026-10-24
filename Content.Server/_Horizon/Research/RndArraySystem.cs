using Content.Server.Power.EntitySystems;
using Content.Server.Research.Systems;
using Content.Shared._Horizon.Research;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Horizon.Research;

/// <summary>
/// Keeps the database of the RnD array equal to what is on its working disks: newly researched technologies are
/// written to the disk of their discipline, and taking a disk out or breaking it takes its technologies away from
/// everybody. Wears the disks out and shows their condition.
/// </summary>
public sealed class RndArraySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly LockSystem _lock = default!;
    [Dependency] private readonly RndArraySlotSystem _slots = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RndArrayComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RndArrayComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<RndArrayComponent, EntRemovedFromContainerMessage>(OnRemoved);
        SubscribeLocalEvent<RndArrayComponent, TechnologyDatabaseModifiedEvent>(OnDatabaseModified);
        SubscribeLocalEvent<RndArrayComponent, LockToggledEvent>(OnLockToggled);
        SubscribeLocalEvent<RndArrayComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<RndArrayComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<RndArrayComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<RndArrayComponent, RndArrayToggleOpenMessage>(OnToggleOpenMessage);
        SubscribeLocalEvent<RndArrayComponent, RndArrayEjectMessage>(OnEjectMessage);

        SubscribeLocalEvent<RndDiskComponent, ExaminedEvent>(OnDiskExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<RndArrayComponent>();
        while (query.MoveNext(out var uid, out var array))
        {
            var ent = (uid, array);
            var broke = false;
            if (now >= array.NextWear)
            {
                // Never catch up on missed time, that would wear the disks out many times faster.
                array.NextWear = now + array.WearInterval;
                if (this.IsPowered(uid, EntityManager))
                    broke = WearDisks(ent, array.WearInterval);
            }

            if (broke)
                Store(ent, false);

            if (broke || now >= array.NextDisplay)
            {
                RefreshDisplay(ent);
                UpdateUi(ent);
            }
        }
    }

    private void OnMapInit(Entity<RndArrayComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextWear = _timing.CurTime + ent.Comp.WearInterval;
        Store(ent, false);
        RefreshDisplay(ent);
    }

    private void OnInserted(Entity<RndArrayComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        OnSlotChanged(ent, args.Container);
    }

    private void OnRemoved(Entity<RndArrayComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        OnSlotChanged(ent, args.Container);
    }

    private void OnSlotChanged(Entity<RndArrayComponent> ent, BaseContainer container)
    {
        if (TerminatingOrDeleted(ent) || !ent.Comp.Slots.Exists(s => s.Id == container.ID))
            return;

        Store(ent, false);
        RefreshDisplay(ent);
        UpdateUi(ent);
    }

    private void OnDatabaseModified(Entity<RndArrayComponent> ent, ref TechnologyDatabaseModifiedEvent args)
    {
        Store(ent, true);
    }

    private void OnLockToggled(Entity<RndArrayComponent> ent, ref LockToggledEvent args)
    {
        UpdateUi(ent);
    }

    private void OnUiOpened(Entity<RndArrayComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUi(ent);
    }

    private void OnGetVerbs(Entity<RndArrayComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var user = args.User;
        var locked = _lock.IsLocked(ent.Owner);
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString(ent.Comp.Open ? "rnd-array-verb-close" : "rnd-array-verb-open"),
            Disabled = locked,
            Message = locked ? Loc.GetString("rnd-array-locked") : null,
            Act = () => TryToggleOpen(ent, user),
        });
    }

    private void OnToggleOpenMessage(Entity<RndArrayComponent> ent, ref RndArrayToggleOpenMessage args)
    {
        TryToggleOpen(ent, args.Actor);
    }

    private void OnEjectMessage(Entity<RndArrayComponent> ent, ref RndArrayEjectMessage args)
    {
        if (!ent.Comp.Open)
            return;

        var id = args.Slot;
        var slot = ent.Comp.Slots.Find(s => s.Id == id);
        if (slot != null)
            _itemSlots.TryEjectToHands(ent, slot.Slot, args.Actor, true);
    }

    private void TryToggleOpen(Entity<RndArrayComponent> ent, EntityUid user)
    {
        if (_lock.IsLocked(ent.Owner))
        {
            _popup.PopupEntity(Loc.GetString("rnd-array-locked"), ent, user);
            return;
        }

        _slots.SetOpen(ent, !ent.Comp.Open);
        UpdateUi(ent);
    }

    /// <summary>
    /// The disks that work: in the slot of their discipline and not broken.
    /// </summary>
    private List<(RndArraySlot Slot, RndDiskComponent Disk)> GetWorkingDisks(RndArrayComponent array)
    {
        var disks = new List<(RndArraySlot, RndDiskComponent)>();
        foreach (var slot in array.Slots)
        {
            if (slot.Slot.Item is { } item
                && TryComp<RndDiskComponent>(item, out var disk)
                && !disk.Broken
                && disk.Discipline == slot.Discipline)
                disks.Add((slot, disk));
        }

        return disks;
    }

    /// <summary>
    /// Makes the database of the array hold exactly the technologies of its working disks,
    /// and only their disciplines researchable.
    /// </summary>
    /// <param name="absorb">
    /// Write the technologies of the database that are on no disk to the disk of their discipline first, instead of
    /// dropping them. Used when something was researched.
    /// </param>
    private void Store(Entity<RndArrayComponent> ent, bool absorb)
    {
        if (!TryComp<TechnologyDatabaseComponent>(ent, out var database))
            return;

        var disks = GetWorkingDisks(ent.Comp);
        if (absorb)
        {
            foreach (var tech in database.UnlockedTechnologies)
            {
                if (disks.Exists(d => d.Disk.Technologies.Contains(tech))
                    || !_prototype.TryIndex(tech, out var proto))
                    continue;

                foreach (var discipline in proto.GetAllDisciplines())
                {
                    var target = disks.Find(d => d.Slot.Discipline == discipline);
                    if (target.Disk == null)
                        continue;

                    target.Disk.Technologies.Add(tech);
                    break;
                }
            }
        }

        var technologies = new List<ProtoId<TechnologyPrototype>>();
        var disciplines = new List<ProtoId<TechDisciplinePrototype>>();
        foreach (var (slot, disk) in disks)
        {
            if (!disciplines.Contains(slot.Discipline))
                disciplines.Add(slot.Discipline);

            foreach (var tech in disk.Technologies)
            {
                if (!technologies.Contains(tech))
                    technologies.Add(tech);
            }
        }

        _research.SetStoredTechnologies(ent, technologies, disciplines, database);
        UpdateVisuals(ent);
    }

    private void OnPowerChanged(Entity<RndArrayComponent> ent, ref PowerChangedEvent args)
    {
        UpdateVisuals(ent);
    }

    /// <summary>
    /// Shows which slots hold a disk, and lights the working disks while the array is powered.
    /// </summary>
    private void UpdateVisuals(Entity<RndArrayComponent> ent)
    {
        var powered = this.IsPowered(ent, EntityManager);
        var disks = 0;
        var leds = 0;
        for (var i = 0; i < ent.Comp.Slots.Count; i++)
        {
            var slot = ent.Comp.Slots[i];
            if (slot.Slot.Item is not { } item || !TryComp<RndDiskComponent>(item, out var disk))
                continue;

            disks |= 1 << i;
            if (powered && !disk.Broken && disk.Discipline == slot.Discipline)
                leds |= 1 << i;
        }

        _appearance.SetData(ent, RndArrayVisuals.Disks, disks);
        _appearance.SetData(ent, RndArrayVisuals.Leds, leds);
    }

    /// <returns>Whether a disk broke.</returns>
    private bool WearDisks(Entity<RndArrayComponent> ent, TimeSpan time)
    {
        var broke = false;
        foreach (var (slot, disk) in GetWorkingDisks(ent.Comp))
        {
            disk.Wear += time;
            if (disk.Wear < disk.Lifetime)
                continue;

            disk.Broken = true;
            disk.Technologies.Clear();
            broke = true;

            _popup.PopupEntity(Loc.GetString("rnd-array-disk-broken", ("discipline", DisciplineName(slot.Discipline))),
                ent,
                PopupType.MediumCaution);
        }

        return broke;
    }

    /// <summary>
    /// Takes a new snapshot of the disk condition for the interface.
    /// </summary>
    private void RefreshDisplay(Entity<RndArrayComponent> ent)
    {
        ent.Comp.NextDisplay = _timing.CurTime + ent.Comp.DisplayInterval;
        ent.Comp.DisplayedCondition.Clear();
        foreach (var slot in ent.Comp.Slots)
        {
            if (slot.Slot.Item is { } item && TryComp<RndDiskComponent>(item, out var disk))
                ent.Comp.DisplayedCondition[slot.Id] = GetCondition(disk);
        }
    }

    private void UpdateUi(Entity<RndArrayComponent> ent)
    {
        if (!_ui.IsUiOpen(ent.Owner, RndArrayUiKey.Key))
            return;

        var slots = new List<RndArraySlotState>();
        foreach (var slot in ent.Comp.Slots)
        {
            if (slot.Slot.Item is { } item && TryComp<RndDiskComponent>(item, out var disk))
            {
                var condition = ent.Comp.DisplayedCondition.GetValueOrDefault(slot.Id);
                slots.Add(new RndArraySlotState(slot.Id,
                    slot.Discipline,
                    true,
                    disk.Broken,
                    RoundCondition(condition, ent.Comp.DisplayStep),
                    GetLamp(condition),
                    disk.Technologies.Count));
            }
            else
            {
                slots.Add(new RndArraySlotState(slot.Id, slot.Discipline, false, false, 0, RndDiskLamp.Off, 0));
            }
        }

        TryComp<ResearchServerComponent>(ent, out var server);
        _ui.SetUiState(ent.Owner,
            RndArrayUiKey.Key,
            new RndArrayBoundUserInterfaceState(_lock.IsLocked(ent.Owner),
                ent.Comp.Open,
                server?.Points ?? 0,
                server?.Clients.Count ?? 0,
                slots));
    }

    private void OnDiskExamined(Entity<RndDiskComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(RndDiskComponent)))
        {
            args.PushMarkup(Loc.GetString("rnd-disk-examine-discipline",
                ("discipline", DisciplineName(ent.Comp.Discipline))));

            if (ent.Comp.Broken)
            {
                args.PushMarkup(Loc.GetString("rnd-disk-examine-broken"));
                return;
            }

            args.PushMarkup(Loc.GetString("rnd-disk-examine-condition",
                ("condition", RoundCondition(GetCondition(ent.Comp), 5))));
            args.PushMarkup(Loc.GetString("rnd-disk-examine-technologies",
                ("count", ent.Comp.Technologies.Count)));
        }
    }

    private string DisciplineName(ProtoId<TechDisciplinePrototype> discipline)
    {
        return _prototype.TryIndex(discipline, out var proto) ? Loc.GetString(proto.Name) : discipline.Id;
    }

    /// <summary>
    /// What is left of the disk in percent, 0 only when it is broken.
    /// </summary>
    private static int GetCondition(RndDiskComponent disk)
    {
        if (disk.Broken || disk.Lifetime <= TimeSpan.Zero)
            return 0;

        var left = 1 - disk.Wear / disk.Lifetime;
        return Math.Clamp((int) Math.Ceiling(left * 100), 1, 100);
    }

    /// <summary>
    /// Rounds the condition to the step, never to 0 while the disk works.
    /// </summary>
    private static int RoundCondition(int condition, int step)
    {
        if (condition <= 0 || step <= 0)
            return condition;

        return Math.Clamp((int) Math.Round(condition / (double) step) * step, step, 100);
    }

    private static RndDiskLamp GetLamp(int condition)
    {
        return condition switch
        {
            >= 76 => RndDiskLamp.Green,
            >= 51 => RndDiskLamp.Yellow,
            >= 26 => RndDiskLamp.Orange,
            >= 1 => RndDiskLamp.Red,
            _ => RndDiskLamp.Off,
        };
    }
}
