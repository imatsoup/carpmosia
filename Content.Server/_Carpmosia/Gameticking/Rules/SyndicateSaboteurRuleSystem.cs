using Content.Server.GameTicking.Rules.Components;
using Content.Server.Roles;


namespace Content.Server.GameTicking.Rules;

/* TODO
Swap endRound with callShuttle (we like it more)
Events
Method for determining what 'stage' we are at and disbursing appropriate objectives while hiding prior objectives
Method for generating team name and company name at round start
*/

public sealed partial class SyndicateSaboteurRuleSystem : GameRuleSystem<SyndicateSaboteurRuleComponent>
{
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private RoleSystem _roleSystem = default!;

    #region Event Handlers
    protected override void AppendRoundEndText(Entity<SyndicateSaboteurRuleComponent> ent,
    GameRuleComponent gameRule,
    ref RoundEndTextAppendEvent args)
    {
        var winText = Loc.GetString($"nukeops-{ent.Comp.WinType.ToString().ToLower()}");
        args.AddLine(winText);

        args.AddLine(Loc.GetString("nukeops-list-start"));

        var antags = _antag.GetAntagIdentifiers(ent);
        foreach (var (_, sessionData, name) in antags)
        {
            args.AddLine(Loc.GetString("nukeops-list-name-user", ("name", name), ("user", sessionData.UserName)));
        }
        args.AddLine("");
    }

    private void OnWarshipSummoned(WarshipSummonedEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var saboteurs, out _))
        {
            if (ev.OwningStation != null)
            {
                if (!TryComp<SyndicateSaboteurRuleComponent>(uid, out var SyndicateSaboteurRule))
                    continue;

                if (GameTicker.IsGameRuleActive(uid, SyndicateSaboteurRule))
                {
                    // Need to be callshuttle
                    _roundEndSystem.EndRound();
                }
            }
        }
    }
    private void OnRoundEnd(Entity<SyndicateSaboteurRuleComponent> ent)
    {
        if (ent.Comp.WinType == WinType.Neutral)
            return;

        if (ent.Comp.WinConditions.Contains(WinCondition.ObjectivesCompleted))
            SetWinType(ent, WinType.CrewMinor);

    }
    #endregion

    private void SetWinType(Entity<SyndicateSaboteurRuleComponent> ent, WinType type, bool endRound = true)
    {
        ent.Comp.WinType = type;

        if (endRound && (type == WinType.CrewMajor || type == WinType.SaboVictory))
            _roundEndSystem.EndRound();
    }

}
