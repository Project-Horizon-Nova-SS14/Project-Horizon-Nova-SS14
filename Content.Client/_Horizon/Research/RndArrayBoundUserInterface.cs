using Content.Shared._Horizon.Research;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Horizon.Research;

[UsedImplicitly]
public sealed class RndArrayBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private RndArrayWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<RndArrayWindow>();
        _window.ToggleOpenRequested += () => SendMessage(new RndArrayToggleOpenMessage());
        _window.EjectRequested += slot => SendMessage(new RndArrayEjectMessage(slot));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not RndArrayBoundUserInterfaceState cast)
            return;

        _window.SetState(cast, _prototype);
    }
}
