using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Store;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server.Store.Conditions;

/// <summary>
/// Filters out an entry based on whether the entity has difficult enough objectives.
/// </summary>
public sealed partial class BuyerDifficultyWhitelistCondition : ListingCondition
{

    /// <summary>
    /// Difficulty rating of objectives required to see the listing.
    /// </summary>
    [DataField(required: true)]
    public float Difficulty;

    public override bool Condition(ListingConditionArgs args)
    {
        var ent = args.EntityManager;
        var whitelistSystem = ent.System<EntityWhitelistSystem>();

        if (!args.EntityManager.TryGetComponent<MindComponent>(args.Buyer, out var mindComp))
            return true; // inanimate objects don't have minds

        var whitelisted = false;

        foreach (var objective in mindComp.Objectives)
        {
            if (args.EntityManager.TryGetComponent<ObjectiveComponent>(objective, out var obj) && obj.Difficulty >= Difficulty)
                whitelisted = true;
        }

        return whitelisted;
    }
}
