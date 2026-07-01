using System;
using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Armor;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.Events;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Database;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Inventory;
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
    [Dependency] private ClothingSystem _clothing = default!;
    [Dependency] private EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private ISharedAdminLogManager _adminLog = default!;
    [Dependency] private ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ModdableSuitComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ModdableSuitComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<ModdableSuitComponent, SuitRefreshModifiersEvent>(RelayEvent);

        SubscribeLocalEvent<SuitModBodyComponent, SuitRefreshModifiersEvent>(OnAddComponentsMod);
        SubscribeLocalEvent<SuitModHelmetComponent, SuitRefreshModifiersEvent>(OnHelmetMod);
        SubscribeLocalEvent<SuitModSlotComponent, SuitRefreshModifiersEvent>(OnSlotMod);


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
            args.PushMarkup(Loc.GetString("moddable-suit-description", ("count", ent.Comp.MaxUpgradeCount)));
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
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || args.Using == null
        || !TryComp<SuitModComponent>(args.Using, out var mod) || GetCurrentUpgrades(ent).Count >= ent.Comp.MaxUpgradeCount
        || GetCurrentUpgradeTags(ent).ToHashSet().IsSupersetOf(mod.Tags)
        || _entityWhitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Using.Value)
        || !_actionBlocker.CanDrop(args.User)
        )
            return;

        // Check if its currently equipped
        if (TryComp<ClothingComponent>(ent, out var clothing) && clothing.InSlot == "outerClothing")
            return;

        var container = _container.GetContainer(ent, ent.Comp.UpgradesContainerId);

        if (!_actionBlocker.CanDrop(args.User))
            return;

        if (container == null)
            return;

        var verbData = args;

        var user = args.User;

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
                    RefreshArmorMods(ent, user, true);
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

        // Check if its currently equipped
        if (TryComp<ClothingComponent>(ent, out var clothing) && clothing.InSlot == "outerClothing")
            return;

        if (container== null)
            return;

        var verbData = args;
        var user = args.User;

        // Add the eject-item verbs
        foreach (var item in container.ContainedEntities)
        {
            if (!_container.CanRemove(item, container))
                continue;

            var verbSubject = "Eject Upgrade";

            AlternativeVerb verb = new()
            {
                IconEntity = GetNetEntity(item),
                Act = () =>
                {
                    RefreshArmorMods(ent, user, false);
                    _container.Remove(item, container);
                    _hands.TryPickupAnyHand(user, item);
                }
            };

            verb.Text = verbSubject;
            verb.Category = VerbCategory.Eject;

            args.Verbs.Add(verb);
        }
    }

    private void RefreshArmorMods(Entity<ModdableSuitComponent> ent, EntityUid user, bool isInserting)
    {

        var ev = new SuitRefreshModifiersEvent(
            ent,
            user,
            isInserting
        );
        RaiseLocalEvent(ent, ref ev);
    }

    public void OnAddComponentsMod(Entity<SuitModBodyComponent> ent, ref SuitRefreshModifiersEvent args)
    {
        if (args.IsInserting)
            EntityManager.AddComponents(args.Suit, ent.Comp.ComponentsToAdd);
        else
            EntityManager.RemoveComponents(args.Suit, ent.Comp.ComponentsToAdd);
    }

    public void OnHelmetMod(Entity<SuitModHelmetComponent> ent, ref SuitRefreshModifiersEvent args)
    {
        if (!TryComp<ToggleableClothingComponent>(args.Suit, out var toggle) || toggle.ClothingUid == null)
            return;
        if (args.IsInserting)
            EntityManager.AddComponents(toggle.ClothingUid.Value, ent.Comp.ComponentsToAdd);
        else
            EntityManager.RemoveComponents(toggle.ClothingUid.Value, ent.Comp.ComponentsToAdd);
    }

    public void OnSlotMod(Entity<SuitModSlotComponent> ent, ref SuitRefreshModifiersEvent args)
    {
        var i = 0;
        foreach (var key in ent.Comp.Keys)
        {
            if (args.IsInserting)
            {

                ItemSlot slot = new();

                if (ent.Comp.Whitelists != null)
                {
                    // Safety measure to prevent going over the list size
                    if (i <= ent.Comp.Whitelists.Count)
                    {
                        slot.Whitelist = ent.Comp.Whitelists[i];
                        i += 1;
                    }
                    _itemSlotsSystem.AddItemSlot(args.Suit, key, slot);
                }
            }
            else
            {
                if (!_itemSlotsSystem.TryGetSlot(args.Suit, key, out var slot) || slot.ContainerSlot == null)
                    return;

                if (slot.ContainerSlot.ContainedEntity != null)
                    _transform.PlaceNextTo(slot.ContainerSlot.ContainedEntity.Value, slot.ContainerSlot.ContainedEntity.Value);

                _itemSlotsSystem.RemoveItemSlot(args.Suit, slot);
            }
        }
    }

    /// <summary>
    /// Gets the entities inside the upgrade container.
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
