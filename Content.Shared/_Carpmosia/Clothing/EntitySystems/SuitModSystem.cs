
namespace Content.Shared.Clothing.EntitySystems;

public sealed partial class GunUpgradeSystem : EntitySystem
{

    /// <inheritdoc/>
    public override void Initialize()
    {

    }

    private void RelayEvent<T>(Entity<UpgradeableSuitComponent> ent, ref T args) where T : notnull
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            RaiseLocalEvent(upgrade, ref args);
        }
    }

    private void OnExamine(Entity<UpgradeableSuitComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(UpgradeableSuitComponent)))
        {
            foreach (var upgrade in GetCurrentUpgrades(ent))
            {
                args.PushMarkup(Loc.GetString(upgrade.Comp.ExamineText));
            }
        }
    }

    private void OnInit(Entity<UpgradeableSuitComponent> ent, ref ComponentInit args)
    {
        _container.EnsureContainer<Container>(ent, ent.Comp.UpgradesContainerId);
    }

    private void OnAfterInteractUsing(Entity<UpgradeableSuitComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (args.Handled || !args.CanReach || !TryComp<SuitUpgradeComponent>(args.Used, out var upgradeComponent))
            return;

        if (GetCurrentUpgrades(ent).Count >= ent.Comp.MaxUpgradeCount)
        {
            _popup.PopupPredicted(Loc.GetString("upgradeable-gun-popup-upgrade-limit"), ent, args.User);
            return;
        }

        if (_entityWhitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Used))
            return;

        if (GetCurrentUpgradeTags(ent).ToHashSet().IsSupersetOf(upgradeComponent.Tags))
        {
            _popup.PopupPredicted(Loc.GetString("upgradeable-gun-popup-already-present"), ent, args.User);
            return;
        }

        args.Handled = _container.Insert(args.Used, _container.GetContainer(ent, ent.Comp.UpgradesContainerId));
        _audio.PlayPredicted(ent.Comp.InsertSound, ent, args.User);
        _popup.PopupClient(Loc.GetString("gun-upgrade-popup-insert", ("upgrade", args.Used), ("gun", ent.Owner)), args.User);
        _gun.RefreshModifiers(ent.Owner);

        _adminLog.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):player} inserted gun upgrade {ToPrettyString(args.Used)} into {ToPrettyString(ent.Owner)}.");
    }

    /// <summary>
    /// Gets the entities inside the gun's upgrade container.
    /// </summary>
    public HashSet<Entity<SuitUpgradeComponent>> GetCurrentUpgrades(Entity<UpgradeableSuitComponent> ent)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.UpgradesContainerId, out var container))
            return new HashSet<Entity<SuitUpgradeComponent>>();

        var upgrades = new HashSet<Entity<SuitUpgradeComponent>>();
        foreach (var contained in container.ContainedEntities)
        {
            if (TryComp<SuitUpgradeComponent>(contained, out var upgradeComp))
                upgrades.Add((contained, upgradeComp));
        }

        return upgrades;
    }

    /// <summary>
    /// Gets the tags of the upgrades currently applied.
    /// </summary>
    public IEnumerable<ProtoId<TagPrototype>> GetCurrentUpgradeTags(Entity<UpgradeableSuitComponent> ent)
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
