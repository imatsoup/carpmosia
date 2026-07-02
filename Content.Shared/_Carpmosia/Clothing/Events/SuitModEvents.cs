using Content.Shared.Actions;
using Content.Shared.Clothing.Components;

namespace Content.Shared.Clothing.Events;
/// <summary>

/// </summary>
[ByRefEvent]
public record struct SuitRefreshModifiersEvent(
    Entity<ModdableSuitComponent> Suit,
    EntityUid User,
    bool IsInserting
);

public sealed partial class SuitModEquipmentActionEvent : InstantActionEvent
{

}
