namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Game rule for saboteurs. Handles conversion.
/// </summary>
[RegisterComponent, Access(typeof(SyndicateSaboteurRuleSystem))]
public sealed partial class SyndicateSaboteurRuleComponent : Component
{

    /// <summary>
    /// Text for round-end shuttle call.
    /// </summary>
    [DataField]
    public string RoundEndTextShuttleCall = "nuke-ops-no-more-threat-announcement-shuttle-call";

    /// <summary>
    /// Text for round-end announcement. Used if shuttle is already called
    /// </summary>
    [DataField]
    public string RoundEndTextAnnouncement = "nuke-ops-no-more-threat-announcement";

    /// <summary>
    /// Generated round start for the character summary.
    /// </summary>
    [DataField]
    public string TeamName = "Fall Guys";

    /// <summary>
    /// Generated round-start for the character summary.
    /// </summary>
    [DataField]
    public string Company = "DonkCo";

    /// <summary>
    /// Time to emergency shuttle to arrive if RoundEndBehavior is ShuttleCall.
    /// </summary>
    [DataField]
    public TimeSpan EvacShuttleTime = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Whether or not the Syndicate Warship Beacon has been activated (endgame)
    /// </summary>
    [DataField]
    public bool IsBeaconActive = false;

    /// <summary>
    ///     Time crew can't call emergency shuttle after beacon has been placed.
    /// </summary>
    [DataField]
    public TimeSpan TimeEvacShuttleDisabled = TimeSpan.FromMinutes(25);

    [DataField]
    public WinType WinType = WinType.Neutral;
    
    [DataField]
    public List<WinCondition> WinConditions = new ();

}
public enum WinType : byte
{
    /// <summary>
    ///     Saboteur win. This means all objectives were completed and the warship was called in.
    /// </summary>
    SaboVictory,
    /// <summary>
    ///     Neutral win. The saboteurs failed to complete their objectives, crew had to evac for some other reason.
    /// </summary>
    Neutral,
    /// <summary>
    ///     Crew minor victory. The saboteurs were able to complete their objectives, but were unable to summon the warship.
    /// </summary>
    CrewMinor,
    /// <summary>
    ///     Crew major win. This means they prevented the saboteurs from completing all their objectives,
    ///     or the saboteurs all died in some other way.
    /// </summary>
    CrewMajor
}

public enum WinCondition : byte
{
    WarshipCalled,
    ObjectivesCompleted,
    ObjectivesIncomplete,
}
