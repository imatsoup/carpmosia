using Content.Shared.Roles.Components;

namespace Content.Shared.Shambler;

public sealed class ShamblerPullSystem : EntitySystem
{


    public override void Initialize()
    {
        base.Initialize;

        SubscribeLocalEvent<ShamblerPullActionComponent, ShamblerPullActionEvent>(OnShamblerPullAction);
    }

    private void OnShamblerPullAction(Entity<ShamblerPullActionComponent> ent, ShamblerPullActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
    }
}
