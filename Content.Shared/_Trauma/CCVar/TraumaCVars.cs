using Robust.Shared.Configuration;
using Robust.Shared;

namespace Content.Shared._Trauma.CCVar;

/// <summary>
/// CVars for content ported from Trauma Station.
/// </summary>
[CVarDefs]
public sealed class TraumaCVars : CVars
{
    /// <summary>
    /// Disable vision effect spawning like footsteps, used for integration tests.
    /// </summary>
    public static readonly CVarDef<bool> DisableVisionEffects =
        CVarDef.Create("trauma.disable_vision_effects", false, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Whether to disable vision cone overlays.
    /// </summary>
    public static readonly CVarDef<bool> DisableVisionCones =
        CVarDef.Create("trauma.disable_vision_cones", false, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Scale for how strong out-of-vision graininess is, 0 is just pure greyscale.
    /// </summary>
    public static readonly CVarDef<float> VisionGrainScale =
        CVarDef.Create("trauma.vision_grain_scale", 0.75f, CVar.CLIENTONLY | CVar.ARCHIVE);
}
