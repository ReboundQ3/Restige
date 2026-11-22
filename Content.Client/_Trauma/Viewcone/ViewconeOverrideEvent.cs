namespace Content.Client._Trauma.Viewcone;

/// <summary>
/// Used for dynamic situations where you should intuitively always show the occludable, like if you're pulling it.
/// </summary>
[ByRefEvent]
public record struct ViewconeOverrideEvent(bool Override = false);
