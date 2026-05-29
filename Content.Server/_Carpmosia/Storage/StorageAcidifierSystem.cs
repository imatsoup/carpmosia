using Content.Shared.Destructible;
using Content.Shared.Examine;
using Content.Shared.Lock;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Robust.Shared.Containers;

namespace Content.Server.Storage.EntitySystems;

/// <summary>
/// Behavior for storage acidifier popup.
/// </summary>
public sealed partial class StorageAcidifierSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _containerSystem = default!;
    [Dependency] private LockSystem _lockSystem = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StorageAcidifierComponent, DestructionEventArgs>(OnDestroy);
        SubscribeLocalEvent<StorageAcidifierComponent, ExaminedEvent>(OnExamined);
    }

    public void OnDestroy(Entity<StorageAcidifierComponent> ent, ref DestructionEventArgs args)
    {
        if(!TryComp<ContainerManagerComponent>(ent.Owner, out var containerManager) || !_lockSystem.IsLocked(ent.Owner))
            return;

        foreach(var container in _containerSystem.GetAllContainers(ent.Owner, containerManager))
        {
            foreach (var item in new List<EntityUid>(container.ContainedEntities))
            {
                QueueDel(item);
            }
        }
        _popup.PopupCoordinates(Loc.GetString(ent.Comp.Msg), Transform(ent).Coordinates, PopupType.Medium);
    }


    // Warning text on crates with acidifier
    private void OnExamined(Entity<StorageAcidifierComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var msg = Loc.GetString("storage-acidifier-warning-text");

        args.PushMarkup(msg);
    }
}
