using Content.Shared.Roles.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared.Shambler;

public sealed class ShamblerPullSystem : EntitySystem
{


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShamblerPullActionComponent, ShamblerPullActionEvent>(OnShamblerPullAction);
    }

    private void OnShamblerPullAction(Entity<ShamblerPullActionComponent> ent, ref ShamblerPullActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
    }
}
