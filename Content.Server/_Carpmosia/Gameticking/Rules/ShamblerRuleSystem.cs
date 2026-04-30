using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Station.Systems;
using Content.Shared.Localizations;
using Content.Shared.Roles.Components;
using Robust.Server.GameObjects;

namespace Content.Server.GameTicking.Rules;

public sealed class ShambleRuleSystem : GameRuleSystem<ShamblerRuleComponent>
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShamblerRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagEntitySelected);
        SubscribeLocalEvent<ShamblerRoleComponent, GetBriefingEvent>(UpdateBriefing);
    }

    private void UpdateBriefing(Entity<ShamblerRoleComponent> entity, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;

        if(ent is null)
            return;

        args.Append(MakeBriefing(ent.Value));
    }

    private void AfterAntagEntitySelected(Entity<ShamblerRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (!_mind.TryGetMind(args.EntityUid, out var mindId, out var mind))
            return;

        _roleSystem.MindHasRole<ShamblerRoleComponent>(mindId, out var shamblerRole);

        if(shamblerRole is null)
            return;

        _antag.SendBriefing(args.EntityUid, MakeBriefing(args.EntityUid), null, null);
    }

    private string MakeBriefing(EntityUid shambler)
    {
        var direction = string.Empty;

        var shamblerXform = Transform(shambler);

        EntityUid? stationGrid = null;
        if (_station.GetStationInMap(shamblerXform.MapID) is { } station)
            stationGrid = _station.GetLargestGrid(station);

        if (stationGrid is not null)
        {
            var stationPosition = _transform.GetWorldPosition((EntityUid)stationGrid);
            var shamblerPosition = _transform.GetWorldPosition(shambler);

            var vectorToStation = stationPosition - shamblerPosition;
            direction = ContentLocalizationManager.FormatDirection(vectorToStation.GetDir());
        }

        var briefing = Loc.GetString("dragon-role-briefing", ("direction", direction));

        return briefing;
    }
}
