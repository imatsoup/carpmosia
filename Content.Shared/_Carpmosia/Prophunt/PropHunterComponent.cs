using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Carpmosia.Prophunt;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedPropHunterSystem))]
public sealed partial class PropHunterComponent : Component
{

    /// <summary>
    /// Entity that marked this entity for a damage surplus.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("user"), AutoNetworkedField]
    public EntityUid User;


    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("damage", required: true)]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Blunt", 2 },
            { "Slash", 2 },
            { "Piercing", 2 }
        }
    };

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("healing", required: true)]
    public DamageSpecifier Healing = new()
    {
        DamageDict = new()
        {
            { "Blunt", -2 },
            { "Slash", -2 },
            { "Piercing", -2 }
        }
    };
}
