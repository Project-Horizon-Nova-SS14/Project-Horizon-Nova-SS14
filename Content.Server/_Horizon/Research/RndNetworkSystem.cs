using Content.Server.Power.EntitySystems;
using Content.Server.Research.Systems;
using Content.Shared._Horizon.Research;
using Content.Shared.Popups;
using Content.Shared.Research.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._Horizon.Research;

/// <summary>
/// The RnD network: access points link to relays, and the research machines of a grid with a linked access point
/// are connected to the RnD array, wherever it is. Machines on the grid of the array are connected directly.
/// </summary>
public sealed class RndNetworkSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    /// <summary>
    /// How often the connections of the research machines are checked.
    /// </summary>
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(2);

    private TimeSpan _nextCheck;

    private readonly HashSet<EntityUid> _reachableGrids = new();
    private readonly List<EntityUid> _changed = new();
    private readonly List<Entity<ResearchClientComponent>> _waiting = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RndRelayComponent, ComponentShutdown>(OnRelayShutdown);
        SubscribeLocalEvent<RndRelayComponent, BoundUIOpenedEvent>(OnRelayUiOpened);
        SubscribeLocalEvent<RndRelayComponent, RndRelayDisconnectMessage>(OnRelayDisconnect);

        SubscribeLocalEvent<RndAccessPointComponent, ComponentShutdown>(OnPointShutdown);
        SubscribeLocalEvent<RndAccessPointComponent, BoundUIOpenedEvent>(OnPointUiOpened);
        SubscribeLocalEvent<RndAccessPointComponent, RndAccessPointConnectMessage>(OnPointConnect);
        SubscribeLocalEvent<RndAccessPointComponent, RndAccessPointDisconnectMessage>(OnPointDisconnect);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextCheck)
            return;
        _nextCheck = _timing.CurTime + CheckInterval;

        RefreshClients();

        var relays = EntityQueryEnumerator<RndRelayComponent>();
        while (relays.MoveNext(out var uid, out var relay))
        {
            if (_ui.IsUiOpen(uid, RndRelayUiKey.Key))
                UpdateRelayUi((uid, relay));
        }

        var points = EntityQueryEnumerator<RndAccessPointComponent>();
        while (points.MoveNext(out var uid, out var point))
        {
            if (_ui.IsUiOpen(uid, RndAccessPointUiKey.Key))
                UpdatePointUi((uid, point));
        }
    }

    /// <summary>
    /// Whether a research machine can be connected to the array: it is on the grid of the array,
    /// or on a grid with a linked access point.
    /// </summary>
    public bool CanReachArray(EntityUid client, EntityUid array)
    {
        if (Transform(client).GridUid is not { } grid)
            return false;

        if (grid == Transform(array).GridUid)
            return true;

        var query = EntityQueryEnumerator<RndAccessPointComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var point, out var xform))
        {
            if (xform.GridUid == grid && IsLinked((uid, point)))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the relay works: it is anchored and powered, and there is an array to link to.
    /// </summary>
    public bool IsRelayOnline(EntityUid relay)
    {
        return Transform(relay).Anchored && this.IsPowered(relay, EntityManager) && ArrayExists();
    }

    /// <summary>
    /// Whether the access point works and is linked to a working relay.
    /// </summary>
    public bool IsLinked(Entity<RndAccessPointComponent> point)
    {
        return point.Comp.Relay is { } relay
            && !TerminatingOrDeleted(relay)
            && Transform(point).Anchored
            && this.IsPowered(point, EntityManager)
            && IsRelayOnline(relay);
    }

    private bool ArrayExists()
    {
        var query = EntityQueryEnumerator<RndArrayComponent>();
        return query.MoveNext(out _, out _);
    }

    /// <summary>
    /// Fills the set with the grids whose research machines can reach the array:
    /// the grid of the array and the grids with a linked access point.
    /// </summary>
    private void CollectReachableGrids(EntityUid array, HashSet<EntityUid> grids)
    {
        grids.Clear();
        if (Transform(array).GridUid is { } arrayGrid)
            grids.Add(arrayGrid);

        var query = EntityQueryEnumerator<RndAccessPointComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var point, out var xform))
        {
            if (xform.GridUid is { } grid && !grids.Contains(grid) && IsLinked((uid, point)))
                grids.Add(grid);
        }
    }

    /// <summary>
    /// Disconnects the research machines that lost the array and connects the ones that can reach it.
    /// The reachable grids are found once, since there are many machines and few access points.
    /// </summary>
    private void RefreshClients()
    {
        var arrays = EntityQueryEnumerator<RndArrayComponent, ResearchServerComponent>();
        if (!arrays.MoveNext(out var arrayUid, out _, out var server))
            return;

        CollectReachableGrids(arrayUid, _reachableGrids);

        _changed.Clear();
        foreach (var client in server.Clients)
        {
            if (Transform(client).GridUid is not { } grid || !_reachableGrids.Contains(grid))
                _changed.Add(client);
        }

        foreach (var client in _changed)
        {
            _research.UnregisterClient(client, arrayUid, serverComponent: server);
        }

        _waiting.Clear();
        var clients = EntityQueryEnumerator<ResearchClientComponent, TransformComponent>();
        while (clients.MoveNext(out var uid, out var client, out var xform))
        {
            if (client.Server == null
                && xform.Anchored
                && xform.GridUid is { } grid
                && _reachableGrids.Contains(grid))
                _waiting.Add((uid, client));
        }

        foreach (var client in _waiting)
        {
            _research.RegisterClient(client, arrayUid, client.Comp, server);
        }
    }

    private void Link(Entity<RndAccessPointComponent> point, Entity<RndRelayComponent> relay)
    {
        Unlink(point);

        relay.Comp.AccessPoints.Add(point);
        point.Comp.Relay = relay.Owner;

        UpdateRelayUi(relay);
        UpdatePointUi(point);
        RefreshClients();
    }

    private void Unlink(Entity<RndAccessPointComponent> point)
    {
        if (point.Comp.Relay is not { } relayUid)
            return;

        point.Comp.Relay = null;
        if (TryComp<RndRelayComponent>(relayUid, out var relay))
        {
            relay.AccessPoints.Remove(point);
            UpdateRelayUi((relayUid, relay));
        }

        UpdatePointUi(point);
    }

    private void OnRelayShutdown(Entity<RndRelayComponent> ent, ref ComponentShutdown args)
    {
        foreach (var uid in ent.Comp.AccessPoints)
        {
            if (!TryComp<RndAccessPointComponent>(uid, out var point))
                continue;

            point.Relay = null;
            UpdatePointUi((uid, point));
        }

        ent.Comp.AccessPoints.Clear();
    }

    private void OnPointShutdown(Entity<RndAccessPointComponent> ent, ref ComponentShutdown args)
    {
        Unlink(ent);
    }

    private void OnRelayUiOpened(Entity<RndRelayComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateRelayUi(ent);
    }

    private void OnPointUiOpened(Entity<RndAccessPointComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdatePointUi(ent);
    }

    private void OnRelayDisconnect(Entity<RndRelayComponent> ent, ref RndRelayDisconnectMessage args)
    {
        if (!TryGetEntity(args.AccessPoint, out var uid)
            || !TryComp<RndAccessPointComponent>(uid, out var point)
            || point.Relay != ent.Owner)
            return;

        Unlink((uid.Value, point));
        RefreshClients();
    }

    private void OnPointConnect(Entity<RndAccessPointComponent> ent, ref RndAccessPointConnectMessage args)
    {
        if (!TryGetEntity(args.Relay, out var uid)
            || !TryComp<RndRelayComponent>(uid, out var relay)
            || ent.Comp.Relay == uid)
            return;

        if (!IsRelayOnline(uid.Value))
        {
            _popup.PopupEntity(Loc.GetString("rnd-access-point-relay-offline"), ent, args.Actor);
            return;
        }

        if (relay.AccessPoints.Count >= relay.Capacity)
        {
            _popup.PopupEntity(Loc.GetString("rnd-access-point-relay-full"), ent, args.Actor);
            return;
        }

        Link(ent, (uid.Value, relay));
    }

    private void OnPointDisconnect(Entity<RndAccessPointComponent> ent, ref RndAccessPointDisconnectMessage args)
    {
        Unlink(ent);
        RefreshClients();
    }

    /// <summary>
    /// The name of a network device with the grid it is on.
    /// </summary>
    private string DeviceName(EntityUid uid)
    {
        if (Transform(uid).GridUid is not { } grid)
            return Name(uid);

        return Loc.GetString("rnd-network-device-name", ("device", Name(uid)), ("grid", Name(grid)));
    }

    private void UpdateRelayUi(Entity<RndRelayComponent> ent)
    {
        var points = new List<RndNetworkDeviceEntry>();
        foreach (var uid in ent.Comp.AccessPoints)
        {
            if (!TryComp<RndAccessPointComponent>(uid, out var point))
                continue;

            points.Add(new RndNetworkDeviceEntry(GetNetEntity(uid), DeviceName(uid), IsLinked((uid, point))));
        }

        _ui.SetUiState(ent.Owner,
            RndRelayUiKey.Key,
            new RndRelayBoundUserInterfaceState(IsRelayOnline(ent), ent.Comp.Level, ent.Comp.Capacity, points));
    }

    private void UpdatePointUi(Entity<RndAccessPointComponent> ent)
    {
        var relays = new List<RndRelayEntry>();
        var query = EntityQueryEnumerator<RndRelayComponent>();
        while (query.MoveNext(out var uid, out var relay))
        {
            relays.Add(new RndRelayEntry(GetNetEntity(uid),
                DeviceName(uid),
                IsRelayOnline(uid),
                relay.AccessPoints.Count,
                relay.Capacity));
        }

        _ui.SetUiState(ent.Owner,
            RndAccessPointUiKey.Key,
            new RndAccessPointBoundUserInterfaceState(IsLinked(ent), GetNetEntity(ent.Comp.Relay), relays));
    }
}
