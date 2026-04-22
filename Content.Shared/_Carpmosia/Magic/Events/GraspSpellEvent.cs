using System;
using Content.Shared.Actions;
using Content.Shared.FixedPoint;

namespace Content.Shared.Magic.Events;

public sealed partial class GraspSpellEvent : EntityTargetActionEvent
{
    [DataField("paralyzeDuration")]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);

    [DataField("path")]
    public String Path { get; set; } = "";
}
