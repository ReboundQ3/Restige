using Content.Shared.Roles.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._SV.Roles.Components;

/// <summary>
/// Added to mind role entities to tag that they're a malfuntioning AI.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfunctioningAiRoleComponent : BaseMindRoleComponent;
