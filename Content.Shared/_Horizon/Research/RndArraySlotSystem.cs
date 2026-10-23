using Content.Shared.Containers.ItemSlots;
using Content.Shared.Lock;
using Content.Shared.Popups;

namespace Content.Shared._Horizon.Research;

/// <summary>
/// Sets up the disk slots of the RnD array, lets only the disk of the right discipline into a slot,
/// and keeps the rack from being locked while it is open.
/// </summary>
public sealed class RndArraySlotSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RndArrayComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<RndArrayComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<RndArrayComponent, ItemSlotInsertAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<RndArrayComponent, LockToggleAttemptEvent>(OnLockToggleAttempt);
    }

    private void OnInit(Entity<RndArrayComponent> ent, ref ComponentInit args)
    {
        // The slots start locked in the prototype, since the rack starts closed.
        foreach (var slot in ent.Comp.Slots)
        {
            _itemSlots.AddItemSlot(ent, slot.Id, slot.Slot);
        }
    }

    private void OnRemove(Entity<RndArrayComponent> ent, ref ComponentRemove args)
    {
        foreach (var slot in ent.Comp.Slots)
        {
            _itemSlots.RemoveItemSlot(ent, slot.Slot);
        }
    }

    private void OnInsertAttempt(Entity<RndArrayComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var itemSlot = args.Slot;
        var slot = ent.Comp.Slots.Find(s => s.Slot == itemSlot);
        if (slot == null)
            return;

        if (!TryComp<RndDiskComponent>(args.Item, out var disk) || disk.Discipline != slot.Discipline)
            args.Cancelled = true;
    }

    private void OnLockToggleAttempt(Entity<RndArrayComponent> ent, ref LockToggleAttemptEvent args)
    {
        if (args.Cancelled || !ent.Comp.Open)
            return;

        args.Cancelled = true;
        if (!args.Silent)
            _popup.PopupClient(Loc.GetString("rnd-array-close-first"), ent, args.User);
    }

    /// <summary>
    /// Opens or closes the rack, locking the disk slots while it is closed.
    /// </summary>
    public void SetOpen(Entity<RndArrayComponent> ent, bool open)
    {
        if (ent.Comp.Open == open)
            return;

        ent.Comp.Open = open;
        Dirty(ent);

        foreach (var slot in ent.Comp.Slots)
        {
            _itemSlots.SetLock(ent, slot.Slot, !open);
        }
    }
}
