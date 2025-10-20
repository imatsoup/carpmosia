using Content.Shared.Interaction;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;

namespace Content.Shared.Materials;
public abstract class SharedMaterialInserterSystem : EntitySystem
{
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _matStorage = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public bool OnInteractUsing(EntityUid uid, ref InteractUsingEvent args)
    {
        // Console.WriteLine("Method Called");
        if (!TryComp<StorageComponent>(uid, out var storage))
            return false;

        if (!TryComp<MaterialStorageComponent>(args.Target, out var matStorage))
            return false;

        foreach (var (item, _location) in storage.StoredItems)
        {
            if (!_matStorage.TryInsertMaterialEntity(args.User, item, args.Target, matStorage))
                return false;
        }

        return true;
    }
}
