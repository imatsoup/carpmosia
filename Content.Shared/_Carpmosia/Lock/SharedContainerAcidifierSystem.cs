using Content.Shared.Destructible;
using Content.Shared.Storage;

namespace Content.Shared.Lock;

/// <summary>
/// Handles behavior for when secure containers should acidify their contents on break.
/// </summary>
public sealed partial class SharedContainerAcidifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContainerAcidifierComponent, BreakageEventArgs>(OnBreakage);
    }

    public void OnBreakage(Entity<ContainerAcidifierComponent> ent, ref BreakageEventArgs args)
    {
        if(!TryComp<EntityStorageComponent>(ent, out var storage) || storage.ContainedEntities <= 0)
            return;

        foreach (var (item, _location) in storage.StoredItems)
        {
            Del(item);
        }
    }
}
