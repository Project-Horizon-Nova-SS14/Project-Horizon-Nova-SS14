using Content.Shared._Horizon.Research;
using Content.Shared.Research.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Prototypes;

namespace Content.Client._Horizon.Research;

/// <summary>
/// The window of the RnD array: the lock and the rack, and for every slot its discipline, the lamp and the rounded
/// condition of the disk in it.
/// </summary>
public sealed class RndArrayWindow : DefaultWindow
{
    private readonly Label _statusLabel = new();
    private readonly Label _infoLabel = new();
    private readonly Button _openButton = new();
    private readonly BoxContainer _slots = new()
    {
        Orientation = BoxContainer.LayoutOrientation.Vertical,
        SeparationOverride = 4,
    };

    public event Action? ToggleOpenRequested;
    public event Action<string>? EjectRequested;

    public RndArrayWindow()
    {
        Title = Loc.GetString("rnd-array-ui-title");
        MinSize = new(500, 360);

        var box = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new(5),
            SeparationOverride = 8,
        };

        box.AddChild(_statusLabel);
        box.AddChild(_infoLabel);
        box.AddChild(_openButton);
        box.AddChild(new Label { Text = Loc.GetString("rnd-array-ui-slots") });
        box.AddChild(_slots);
        Contents.AddChild(box);

        _openButton.OnPressed += _ => ToggleOpenRequested?.Invoke();
    }

    public void SetState(RndArrayBoundUserInterfaceState state, IPrototypeManager prototype)
    {
        _statusLabel.Text = Loc.GetString("rnd-array-ui-status",
            ("lock", Loc.GetString(state.Locked ? "rnd-array-ui-locked" : "rnd-array-ui-unlocked")),
            ("rack", Loc.GetString(state.Open ? "rnd-array-ui-opened" : "rnd-array-ui-closed")));
        _infoLabel.Text = Loc.GetString("rnd-array-ui-info", ("points", state.Points), ("clients", state.Clients));
        _openButton.Text = Loc.GetString(state.Open ? "rnd-array-ui-close" : "rnd-array-ui-open");
        _openButton.Disabled = state.Locked;

        _slots.RemoveAllChildren();
        foreach (var slot in state.Slots)
        {
            _slots.AddChild(CreateSlotRow(slot, state.Open, prototype));
        }
    }

    private Control CreateSlotRow(RndArraySlotState slot, bool open, IPrototypeManager prototype)
    {
        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
        };

        row.AddChild(new PanelContainer
        {
            MinSize = new(16, 16),
            VerticalAlignment = Control.VAlignment.Center,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = GetLampColor(slot.Lamp),
                BorderColor = Color.Black,
                BorderThickness = new(1),
            },
        });

        var name = slot.Discipline.Id;
        var color = Color.White;
        if (prototype.TryIndex(slot.Discipline, out var discipline))
        {
            name = Loc.GetString(discipline.Name);
            color = discipline.Color;
        }

        row.AddChild(new Label { Text = name, FontColorOverride = color, MinWidth = 170 });

        string condition;
        if (!slot.HasDisk)
            condition = Loc.GetString("rnd-array-ui-no-disk");
        else if (slot.Broken)
            condition = Loc.GetString("rnd-array-ui-broken");
        else
            condition = Loc.GetString("rnd-array-ui-condition", ("condition", slot.Condition));

        row.AddChild(new Label { Text = condition, MinWidth = 90 });
        row.AddChild(new Label
        {
            Text = slot.HasDisk ? Loc.GetString("rnd-array-ui-technologies", ("count", slot.Technologies)) : string.Empty,
            HorizontalExpand = true,
        });

        var eject = new Button
        {
            Text = Loc.GetString("rnd-array-ui-eject"),
            Disabled = !open || !slot.HasDisk,
        };
        eject.OnPressed += _ => EjectRequested?.Invoke(slot.Id);
        row.AddChild(eject);

        return row;
    }

    private static Color GetLampColor(RndDiskLamp lamp)
    {
        return lamp switch
        {
            RndDiskLamp.Green => Color.FromHex("#3BD13B"),
            RndDiskLamp.Yellow => Color.FromHex("#E8D43A"),
            RndDiskLamp.Orange => Color.FromHex("#E8892E"),
            RndDiskLamp.Red => Color.FromHex("#D93636"),
            _ => Color.FromHex("#262626"),
        };
    }
}
