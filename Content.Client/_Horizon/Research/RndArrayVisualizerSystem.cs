using Content.Shared._Horizon.Research;
using Robust.Client.GameObjects;

namespace Content.Client._Horizon.Research;

/// <summary>
/// Shows the disks in the slots of the RnD array and lights the ones that work.
/// </summary>
public sealed class RndArrayVisualizerSystem : VisualizerSystem<RndArrayComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, RndArrayComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        AppearanceSystem.TryGetData<int>(uid, RndArrayVisuals.Disks, out var disks, args.Component);
        AppearanceSystem.TryGetData<int>(uid, RndArrayVisuals.Leds, out var leds, args.Component);

        var sprite = (uid, args.Sprite);
        for (var i = 0; i < component.Slots.Count; i++)
        {
            var slot = component.Slots[i];
            var bit = 1 << i;

            if (slot.DiskLayer != null && SpriteSystem.LayerMapTryGet(sprite, slot.DiskLayer, out var disk, false))
                SpriteSystem.LayerSetVisible(sprite, disk, (disks & bit) != 0);

            if (slot.LedLayer != null && SpriteSystem.LayerMapTryGet(sprite, slot.LedLayer, out var led, false))
                SpriteSystem.LayerSetVisible(sprite, led, (leds & bit) != 0);
        }
    }
}
