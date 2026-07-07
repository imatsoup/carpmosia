using System;
using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
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
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.EntitySystems;

/// <summary>
/// Handles the entirity of logic for moddable suits and suit modkits (upgrades)
/// </summary>
public sealed partial class SuitModSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private ClothingSystem _clothing = default!;
    [Dependency] private EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private ISharedAdminLogManager _adminLog = default!;
    [Dependency] private ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
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

        SubscribeLocalEvent<SuitModEquipmentToggleComponent, ComponentInit>(OnDeployableGearModInit);
        SubscribeLocalEvent<SuitModEquipmentToggleComponent, ComponentShutdown>(OnDeployableGearModShutdown);
        SubscribeLocalEvent<SuitModEquipmentToggleComponent, SuitModEquipmentActionEvent>(OnAddEquipmentModToggleAction);

        SubscribeLocalEvent<ModdableSuitComponent, GotUnequippedEvent>(OnSuitUnequipped);

        SubscribeLocalEvent<ModdableSuitComponent, GetVerbsEvent<InteractionVerb>>(AddInsertVerb);
        SubscribeLocalEvent<ModdableSuitComponent, GetVerbsEvent<AlternativeVerb>>(AddEjectVerb);

    }

    /// <summary>
    /// Event relay for modkit system.
    /// Call the appropriate function based on the Modkit component.
    /// </summary>
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

    /// <summary>
    /// Handles what to do when the deployable equipment modkit component initializes
    /// </summary>
    private void OnDeployableGearModInit(Entity<SuitModEquipmentToggleComponent> ent, ref ComponentInit args)
    {
        var container = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);

        if (ent.Comp.SpawnedPrototype == null)
            return;

        // Spawn the equipment, then insert into into its own special storage.
        var item = Spawn(ent.Comp.SpawnedPrototype.Value);
        _container.Insert(item, container);
        ent.Comp.Equipment = item;

    }

    /// <summary>
    /// Handles the deployable modkit component shutdown.
    /// Needed to avoid mysterious client-sided clones.
    /// </summary>
    private void OnDeployableGearModShutdown(Entity<SuitModEquipmentToggleComponent> ent, ref ComponentShutdown args)
    {
        var container = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);

        // If the equipment exists, delete it.
        if (ent.Comp.Equipment == null)
            return;

        QueueDel(ent.Comp.Equipment);
    }

    /// <summary>
    /// Adds interaction verbs for inserting a modkit into a hardsuit.
    /// </summary>
    private void AddInsertVerb(Entity<ModdableSuitComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        // Can the user properly interact with it? Does it even exist?
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || args.Using == null || !_actionBlocker.CanDrop(args.User))
            return;

        // Check we don't already have one installed and we're not at our limit.
        if (!TryComp<SuitModComponent>(args.Using, out var mod) || GetCurrentUpgrades(ent).Count >= ent.Comp.MaxUpgradeCount
        || GetCurrentUpgradeTags(ent).ToHashSet().IsSupersetOf(mod.Tags))
            return;

        // Check to see if the modkit is whitelisted to "fit" in the suit
        if (_entityWhitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Using.Value))
            return;

        // Check if our suit is currently equipped, if so, block the verb.
        if (TryComp<ClothingComponent>(ent, out var clothing) && clothing.InSlot == "outerClothing")
            return;

        var container = _container.GetContainer(ent, ent.Comp.UpgradesContainerId);

        // If the upgrade container doesn't exist, don't show the insert verb.
        if (container == null)
            return;

        var verbData = args;

        var user = args.User;

        // Add the insert item verb
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

    /// <summary>
    /// Adds alternative verb for ejecting modkits from a modded suit.
    /// </summary>
    private void AddEjectVerb(Entity<ModdableSuitComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        // Can we interact with it properly?
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || !_actionBlocker.CanDrop(args.User))
            return;

        var container = _container.GetContainer(ent, ent.Comp.UpgradesContainerId);

        // Check if its currently equipped, if so, block the verb
        if (TryComp<ClothingComponent>(ent, out var clothing) && clothing.InSlot == "outerClothing")
            return;

        // Check to see if our container exists.
        if (container == null)
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

    /// <summary>
    /// For use with mods that add bespoke components to a hardsuit.
    /// If you need to add a visor or other mod that should only be active
    /// with the helmet active, use OnHelmetMod.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="args"></param>
    public void OnAddComponentsMod(Entity<SuitModBodyComponent> ent, ref SuitRefreshModifiersEvent args)
    {
        if (args.IsInserting)
            EntityManager.AddComponents(args.Suit, ent.Comp.ComponentsToAdd);
        else
            EntityManager.RemoveComponents(args.Suit, ent.Comp.ComponentsToAdd);
    }

    /// <summary>
    /// For mods that should only be active while the helmet is deployed.
    /// </summary>
    public void OnHelmetMod(Entity<SuitModHelmetComponent> ent, ref SuitRefreshModifiersEvent args)
    {
        if (!TryComp<ToggleableClothingComponent>(args.Suit, out var toggle) || toggle.ClothingUid == null)
            return;
        if (args.IsInserting)
            EntityManager.AddComponents(toggle.ClothingUid.Value, ent.Comp.ComponentsToAdd);
        else
            EntityManager.RemoveComponents(toggle.ClothingUid.Value, ent.Comp.ComponentsToAdd);
    }

    /// <summary>
    /// For mods that add inventory slots to modded suits.
    /// </summary>
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
    /// Action event for deployable equipment. (Equipment that is part of the hardsuit mod deploys to your hands)
    /// </summary>
    public void OnAddEquipmentModToggleAction(Entity<SuitModEquipmentToggleComponent> ent, ref SuitModEquipmentActionEvent args)
    {
        // Check if we've already handled the event.
        if (args.Handled)
            return;

        // Try to get the deployable equipment container
        if (!_container.TryGetContainer(ent, ent.Comp.ContainerId, out var container))
            return;

        // Check that the deployable exists
        if (ent.Comp.Equipment == null)
            return;

        // If the equipment isn't deployed, deploy it. If it is, put it back in its container
        if (!ent.Comp.Deployed)
        {
            if ( _hands.GetEmptyHandCount(args.Performer) < ent.Comp.RequiredHands)
            {
                _popupSystem.PopupPredicted(Loc.GetString("wieldable-component-not-enough-free-hands",
                ("number", ent.Comp.RequiredHands), ("item", ent.Comp.Equipment.Value)), ent.Comp.Equipment.Value, args.Performer);
                return;
            }
            _hands.TryPickupAnyHand(args.Performer, ent.Comp.Equipment.Value);
            ent.Comp.Deployed = true;
            EnsureComp<UnremoveableComponent>(ent.Comp.Equipment.Value);
        }
        else
        {
            RemComp<UnremoveableComponent>(ent.Comp.Equipment.Value);
            _container.Insert(ent.Comp.Equipment.Value, container);
            ent.Comp.Deployed = false;
            DirtyEntity(ent.Comp.Equipment.Value);
        }

        args.Handled = true;
    }

    /// <summary>
    /// Cleans up after SuitModEquipmentToggleComponent.
    /// </summary>
    public void OnSuitUnequipped(Entity<ModdableSuitComponent> ent, ref GotUnequippedEvent args)
    {
        if (TryComp<SuitModEquipmentToggleComponent>(ent.Owner, out var comp)
            && comp.Equipment != null && comp.Deployed
            && _container.TryGetContainer(ent, comp.ContainerId, out var container))
        {
            RemComp<UnremoveableComponent>(comp.Equipment.Value);
            _container.Insert(comp.Equipment.Value, container);
            comp.Deployed = false;
            DirtyEntity(comp.Equipment.Value);
        }
    }

    /// <summary>
    /// Method that activates all the mods via event relay.
    /// </summary>
    /// <param name="user">The individual interacting with the modkit</param>
    /// <param name="isInserting">Are we inserting or removing the modkit?</param>
    private void RefreshArmorMods(Entity<ModdableSuitComponent> ent, EntityUid user, bool isInserting)
    {
        var ev = new SuitRefreshModifiersEvent(
            ent,
            user,
            isInserting
        );
        RaiseLocalEvent(ent, ref ev);
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
