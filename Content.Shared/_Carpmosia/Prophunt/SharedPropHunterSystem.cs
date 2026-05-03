
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Carpmosia.Prophunt;

public abstract class SharedPropHunterSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PropHunterComponent, MeleeHitEvent>(OnAttackAttempt);

    }

    private void OnAttackAttempt(EntityUid ent, PropHunterComponent comp, MeleeHitEvent args)
    {
        if (args.Handled)
            return;

        foreach (var uid in args.HitEntities)
        {
            if (args.User == uid)
                continue;

            if (TryComp<MobStateComponent>(uid, out var mobState))
            {
                if (_mobState.IsAlive(uid, mobState))
                {
                    _damageable.TryChangeDamage(args.User, comp.Healing, true);
                    args.Handled = true;
                    return;
                }
            }
        }

        _damageable.TryChangeDamage(args.User, comp.Damage, true);
        args.Handled = true;
    }

}
