using System.Runtime.CompilerServices;
using Content.Server.Cargo.Systems;
using Content.Shared.Inventory;
using Content.Shared.Magic.Components;
using Content.Shared.Magic.Events;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.Stunnable;

namespace Content.Shared.Magic;

/// <summary>
///     Handles Carpmosia specific spell events
/// </summary>
public abstract class SharedCarpMagicSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CarpMagicComponent, BeforeCastSpellEvent>(OnBeforeCastSpell);

        SubscribeLocalEvent<GraspSpellEvent>(OnGraspSpellEvent);
    }

    private void OnBeforeCastSpell(Entity<CarpMagicComponent> ent, ref BeforeCastSpellEvent args)
    {
        var comp = ent.Comp;
        var hasReqs = true;

        if (comp.RequiresClothes)
        {
            var enumerator = _inventory.GetSlotEnumerator(args.Performer, SlotFlags.OUTERCLOTHING | SlotFlags.HEAD);
            while (enumerator.MoveNext(out var containerSlot))
            {
                if (containerSlot.ContainedEntity is { } item)
                    hasReqs = HasComp<WizardClothesComponent>(item);
                else
                    hasReqs = false;

                if (!hasReqs)
                    break;
            }
        }

        if (comp.RequiresSpeech && HasComp<MutedComponent>(args.Performer))
            hasReqs = false;

        if (hasReqs)
            return;

        args.Cancelled = true;
        _popup.PopupClient(Loc.GetString("spell-requirements-failed"), args.Performer, args.Performer);

        // TODO: Pre-cast do after, either here or in SharedActionsSystem
    }
    private bool PassesSpellPrerequisites(EntityUid spell, EntityUid performer)
    {
        var ev = new BeforeCastSpellEvent(performer);
        RaiseLocalEvent(spell, ref ev);
        return !ev.Cancelled;
    }

    /*
        TODO: Stun for Duration of spell, then apply other effects
    */

    private void OnGraspSpellEvent(GraspSpellEvent ev)
    {
        if (ev.Handled)
            return;

        ev.Handled = true;
        _stun.TryUpdateParalyzeDuration(ev.Target, ev.ParalyzeDuration);

        switch (ev.Path)
        {
            case "Depths" :

                break;
            default :
                break;
        }

    }
}
