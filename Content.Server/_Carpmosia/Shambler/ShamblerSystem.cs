using Content.Shared.Actions;
using Content.Shared.Maps;
// using Content.Shared.Shambler;

namespace Content.Server.Shambler;

public sealed partial class ShamblerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShamblerComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(EntityUid uid, ShamblerComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.ShamblerJauntActionEntity, component.ShamblerJauntAction);
        // _actions.AddAction(uid, ref component.ShamblerPullActionEntity, component.ShamblerPullAction);
        // _actions.AddAction(uid, ref component.ShamblerTelepathyActionEntity, component.ShamblerTelepathyAction);
        // _actions.AddAction(uid, ref component.ShamblerSlamActionEntity, component.ShamblerSlamAction);
    }
}
