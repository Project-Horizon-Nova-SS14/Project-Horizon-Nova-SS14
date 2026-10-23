using Content.Shared._Horizon.Research;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._Horizon.Research;

/// <summary>
/// The window of an RnD access point: whether it is linked, and the relays to link to with their free channels.
/// </summary>
public sealed class RndAccessPointWindow : DefaultWindow
{
    private readonly Label _statusLabel = new();
    private readonly Label _relayLabel = new();
    private readonly Button _disconnectButton = new() { Text = Loc.GetString("rnd-network-ui-disconnect") };
    private readonly BoxContainer _relays = new()
    {
        Orientation = BoxContainer.LayoutOrientation.Vertical,
        SeparationOverride = 4,
    };

    public event Action<NetEntity>? ConnectRequested;
    public event Action? DisconnectRequested;

    public RndAccessPointWindow()
    {
        Title = Loc.GetString("rnd-access-point-ui-title");
        MinSize = new(460, 300);

        var box = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new(5),
            SeparationOverride = 8,
        };

        box.AddChild(_statusLabel);
        box.AddChild(_relayLabel);
        box.AddChild(_disconnectButton);
        box.AddChild(new Label { Text = Loc.GetString("rnd-access-point-ui-relays") });
        box.AddChild(new ScrollContainer { VerticalExpand = true, HScrollEnabled = false, Children = { _relays } });
        Contents.AddChild(box);

        _disconnectButton.OnPressed += _ => DisconnectRequested?.Invoke();
    }

    public void SetState(RndAccessPointBoundUserInterfaceState state)
    {
        _statusLabel.Text = Loc.GetString(state.Linked ? "rnd-access-point-ui-linked" : "rnd-access-point-ui-unlinked");

        var current = state.Relays.Find(r => r.Entity == state.Relay);
        _relayLabel.Text = Loc.GetString("rnd-access-point-ui-relay",
            ("relay", current?.Name ?? Loc.GetString("rnd-network-ui-none")));
        _disconnectButton.Disabled = state.Relay == null;

        _relays.RemoveAllChildren();
        if (state.Relays.Count == 0)
        {
            _relays.AddChild(new Label { Text = Loc.GetString("rnd-network-ui-none") });
            return;
        }

        foreach (var relay in state.Relays)
        {
            var row = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                SeparationOverride = 8,
            };

            var free = Math.Max(0, relay.Capacity - relay.Used);
            row.AddChild(new Label { Text = relay.Name, HorizontalExpand = true, ClipText = true });
            row.AddChild(new Label
            {
                Text = Loc.GetString("rnd-access-point-ui-free", ("free", free), ("capacity", relay.Capacity)),
            });
            row.AddChild(new Label
            {
                Text = Loc.GetString(relay.Online ? "rnd-network-ui-online" : "rnd-network-ui-offline"),
            });

            var connect = new Button
            {
                Text = Loc.GetString("rnd-access-point-ui-connect"),
                Disabled = relay.Entity == state.Relay || !relay.Online || free == 0,
            };
            connect.OnPressed += _ => ConnectRequested?.Invoke(relay.Entity);
            row.AddChild(connect);

            _relays.AddChild(row);
        }
    }
}
