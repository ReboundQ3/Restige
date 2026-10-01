// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared._Goobstation.CCVar;

/// <summary>
/// CVars ported from Goob-Station (Content.Goobstation.Common/CCVar/CCVars.Goob.cs).
/// </summary>
[CVarDefs]
public sealed partial class GoobCVars
{
    #region Blob

    public static readonly CVarDef<bool> BlobCanGrowInSpace =
        CVarDef.Create("blob.grow_space", true, CVar.SERVER);

    #endregion
}
