using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Armor;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.Events;
using Content.Shared.Database;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.EntitySystems;

public sealed partial class SuitModSystem : EntitySystem
{

    [Dependency] private ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private ISharedAdminLogManager _adminLog = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ModdableSuitComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ModdableSuitComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<ModdableSuitComponent, SuitRefreshModifiersEvent>(RelayEvent);

        SubscribeLocalEvent<SuitModCustomComponent, SuitRefreshModifiersEvent>(OnAddComponentsMod);
        SubscribeLocalEvent<SuitModSpeedMalusComponent, SuitRefreshModifiersEvent>(OnSpeedMalusMod);
        SubscribeLocalEvent<SuitModSpeedMalusComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);

        SubscribeLocalEvent<ModdableSuitComponent, GetVerbsEvent<InteractionVerb>>(AddInsertVerb);
        SubscribeLocalEvent<ModdableSuitComponent, GetVerbsEvent<AlternativeVerb>>(AddEjectVerb);

    }

    private void RelayEvent<T>(Entity<ModdableSuitComponent> ent, ref T args) where T : notnull
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            RaiseLocalEvent(upgrade, ref args);
        }
    }

    private void OnExamine(Entity<ModdableSuitComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(ModdableSuitComponent)))
        {
            foreach (var upgrade in GetCurrentUpgrades(ent))
            {
                args.PushMarkup(Loc.GetString(upgrade.Comp.ExamineText));
            }
        }
    }

    private void OnInit(Entity<ModdableSuitComponent> ent, ref ComponentInit args)
    {
        _container.EnsureContainer<Container>(ent, ent.Comp.UpgradesContainerId);
    }

    private void AddInsertVerb(Entity<ModdableSuitComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || args.Using == null || !TryComp<SuitModComponent>(args.Using, out var mod) || GetCurrentUpgrades(ent).Count >= ent.Comp.MaxUpgradeCount)
            return;

        var container = _container.GetContainer(ent, ent.Comp.UpgradesContainerId);

        if (!_actionBlocker.CanDrop(args.User))
            return;

        if (container== null)
            return;

        var verbData = args;

        if (_container.CanInsert(args.Using.Value, container))
        {
            InteractionVerb insertVerb = new()
            {
                Text = Name(args.Using.Value),
                Category = VerbCategory.Insert,
                Act = () =>
                {
                    _adminLog.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(verbData.User):player} inserted {ToPrettyString(verbData.Using.Value)} into {ToPrettyString(ent)}");
                    _container.Insert(verbData.Using.Value, container);
                }
            };
            args.Verbs.Add(insertVerb);
        }
    }

    private void AddEjectVerb(Entity<ModdableSuitComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        var container = _container.GetContainer(ent, ent.Comp.UpgradesContainerId);

        if (!_actionBlocker.CanDrop(args.User))
            return;

        if (container== null)
            return;

        var verbData = args;

        // Add the eject-item verbs
        foreach (var item in container.ContainedEntities)
        {
            if (!_container.CanRemove(item, container))
                continue;

                // if (!_actionBlockerSystem.CanPickup(args.User, slot.Item!.Value))
                //     continue;

            var verbSubject = "Eject Upgrade";

            AlternativeVerb verb = new()
            {
                IconEntity = GetNetEntity(item),
                Act = () => _container.Remove(item, container)
            };

            verb.Text = verbSubject;
            verb.Category = VerbCategory.Eject;

            args.Verbs.Add(verb);
        }
    }

    public void OnAddComponentsMod(Entity<SuitModCustomComponent> ent, ref SuitRefreshModifiersEvent args)
    {
        EntityManager.AddComponents(args.Suit, ent.Comp.ComponentsToAdd);
    }

    public void OnSpeedMalusMod(Entity<SuitModSpeedMalusComponent> ent, ref SuitRefreshModifiersEvent args)
    {
        var ev = new RefreshMovementSpeedModifiersEvent();

        RaiseLocalEvent(ent, ev);
    }

    private void OnRefreshMovespeed(Entity<SuitModSpeedMalusComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.Modifier, ent.Comp.Modifier);
    }

    /// <summary>
    /// Gets the entities inside the gun's upgrade container.
    /// </summary>
    public HashSet<Entity<SuitModComponent>> GetCurrentUpgrades(Entity<ModdableSuitComponent> ent)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.UpgradesContainerId, out var container))
            return new HashSet<Entity<SuitModComponent>>();

        var upgrades = new HashSet<Entity<SuitModComponent>>();
        foreach (var contained in container.ContainedEntities)
        {
            if (TryComp<SuitModComponent>(contained, out var upgradeComp))
                upgrades.Add((contained, upgradeComp));
        }

        return upgrades;
    }

    /// <summary>
    /// Gets the tags of the upgrades currently applied.
    /// </summary>
    public IEnumerable<ProtoId<TagPrototype>> GetCurrentUpgradeTags(Entity<ModdableSuitComponent> ent)
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            foreach (var tag in upgrade.Comp.Tags)
            {
                yield return tag;
            }
        }
    }
}
