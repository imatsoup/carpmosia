using Content.Shared.Chemistry.Components;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Shambler
{
    [RegisterComponent]
    public sealed partial class ShamblerComponent : Component
    {

        [ViewVariables(VVAccess.ReadWrite), DataField("soundDeath")]
        public SoundSpecifier? SoundDeath = new SoundPathSpecifier("/Audio/Animals/space_dragon_roar.ogg");

        [ViewVariables(VVAccess.ReadWrite), DataField("soundRoar")]
        public SoundSpecifier? SoundRoar =
            new SoundPathSpecifier("/Audio/Animals/space_dragon_roar.ogg")
            {
                Params = AudioParams.Default.WithVolume(3f),
            };

        /// <summary>
        /// Spawns a rift which can summon more mobs.
        /// </summary>
        [DataField("shamblerJauntActionEntity")]
        public EntityUid? ShamblerJauntActionEntity;

        /// <summary>
        /// Maximum time the dragon can go without spawning a rift before they die.
        /// </summary>

        [ViewVariables(VVAccess.ReadWrite), DataField("shamblerJauntAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ShamblerJauntAction = "ActionShamblerJaunt";

        /// <summary>
        /// NPC faction to re-add after being zombified.
        /// Prevents zombie dragon from being attacked by its own carp.
        /// </summary>
        [DataField]
        public ProtoId<NpcFactionPrototype> Faction = "Dragon";

        /// <summary>
        /// The smoke to spawn upon rift timeout death.
        /// </summary>
        [DataField]
        public EntProtoId SmokePrototype = "BloodSmoke";

        /// <summary>
        /// The solution to place into the smoke (mostly just needed for color)
        /// </summary>
        [DataField]
        public Solution SmokeSolution = new ([new("Blood", 1)]);
    }
}
