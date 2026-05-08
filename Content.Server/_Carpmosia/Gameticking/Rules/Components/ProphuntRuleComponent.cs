using Content.Shared.FixedPoint;
using Content.Shared.Roles;
using Content.Shared.Storage;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Gamerule that ends when a player gets a certain number of kills.
/// </summary>
[RegisterComponent, Access(typeof(ProphuntRuleSystem))]
public sealed partial class ProphuntRuleComponent : Component
{

    /// <summary>
    /// How long until the round restarts
    /// </summary>
    [DataField("restartDelay"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan RestartDelay = TimeSpan.FromSeconds(10f);

    /// <summary>
    /// Winning team.
    /// </summary>
    [DataField("victor")]
    public string? Victor;


    public struct HunterTeam
    {
        public List<ICommonSession> teamMembers = new();
        public FixedPoint2 score;
    }

    public List<ICommonSession> propTeam = new();

    /// <summary>
    /// The gear team hunter players spawn with.
    /// </summary>
    [DataField("propHunterStartingGear", customTypeSerializer: typeof(PrototypeIdSerializer<StartingGearPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string HunterGear = "PropHunterStartingGear";

    /// <summary>
    /// The gear team prop players spawn with.
    /// </summary>
    [DataField("propStartingGear", customTypeSerializer: typeof(PrototypeIdSerializer<StartingGearPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string PropGear = "PropStartingGear";
}
