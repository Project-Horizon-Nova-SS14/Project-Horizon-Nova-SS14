using Content.Shared._Horizon.Research;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._Horizon.Research;

/// <summary>
/// The window of an RnD relay: whether it works, how many channels are left and which access points are linked.
/// </summary>
public sealed class RndRelayWindow : DefaultWindow
{
    private readonly Label _statusLabel = new();
    private readonly Label _levelLabel = new();
    private readonly Label _capacityLabel = new();
    private readonly BoxContainer _points = new()
    {
        Orientation = BoxContainer.LayoutOrientation.Vertical,
        SeparationOverride = 4,
    };

    public event Action<NetEntity>? DisconnectRequested;

    public RndRelayWindow()
    {
        Title = Loc.GetString("rnd-relay-ui-title");
        MinSize = new(420, 280);

        var box = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new(5),
            SeparationOverride = 8,
        };

        box.AddChild(_statusLabel);
        box.AddChild(_levelLabel);
        box.AddChild(_capacityLabel);
        box.AddChild(new Label { Text = Loc.GetString("rnd-relay-ui-access-points") });
        box.AddChild(new ScrollContainer { VerticalExpand = true, HScrollEnabled = false, Children = { _points } });
        Contents.AddChild(box);
    }

    public void SetState(RndRelayBoundUserInterfaceState state)
    {
        _statusLabel.Text = Loc.GetString(state.Online ? "rnd-network-ui-online" : "rnd-network-ui-offline");
        _levelLabel.Text = Loc.GetString("rnd-relay-ui-level", ("level", state.Level));
        _capacityLabel.Text = Loc.GetString("rnd-relay-ui-capacity",
            ("used", state.AccessPoints.Count),
            ("capacity", state.Capacity),
            ("free", Math.Max(0, state.Capacity - state.AccessPoints.Count)));

        _points.RemoveAllChildren();
        if (state.AccessPoints.Count == 0)
        {
            _points.AddChild(new Label { Text = Loc.GetString("rnd-network-ui-none") });
            return;
        }

        foreach (var point in state.AccessPoints)
        {
            var row = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                SeparationOverride = 8,
            };

            row.AddChild(new Label { Text = point.Name, HorizontalExpand = true, ClipText = true });
            row.AddChild(new Label
            {
                Text = Loc.GetString(point.Online ? "rnd-network-ui-online" : "rnd-network-ui-offline"),
            });

            var disconnect = new Button { Text = Loc.GetString("rnd-network-ui-disconnect") };
            disconnect.OnPressed += _ => DisconnectRequested?.Invoke(point.Entity);
            row.AddChild(disconnect);

            _points.AddChild(row);
        }
    }
}
