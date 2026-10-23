using Content.Shared._Horizon.Research;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Horizon.Research;

[UsedImplicitly]
public sealed class RndAccessPointBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private RndAccessPointWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<RndAccessPointWindow>();
        _window.ConnectRequested += relay => SendMessage(new RndAccessPointConnectMessage(relay));
        _window.DisconnectRequested += () => SendMessage(new RndAccessPointDisconnectMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not RndAccessPointBoundUserInterfaceState cast)
            return;

        _window.SetState(cast);
    }
}
