using Content.Shared.Clothing.Components;

namespace Content.Shared.Clothing.Events;
/// <summary>

/// </summary>
[ByRefEvent]
public record struct SuitRefreshModifiersEvent(
    Entity<ModdableSuitComponent> suit
);
