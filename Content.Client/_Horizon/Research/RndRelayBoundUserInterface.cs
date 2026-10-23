using Content.Shared._Horizon.Research;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Horizon.Research;

[UsedImplicitly]
public sealed class RndRelayBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private RndRelayWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<RndRelayWindow>();
        _window.DisconnectRequested += point => SendMessage(new RndRelayDisconnectMessage(point));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not RndRelayBoundUserInterfaceState cast)
            return;

        _window.SetState(cast);
    }
}
