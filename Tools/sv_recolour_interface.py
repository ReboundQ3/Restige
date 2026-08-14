#!/usr/bin/env python3
# SPDX-FileCopyrightText: 2026 Sector-Vestige contributors
# SPDX-FileCopyrightText: 2026 ReboundQ3 <22770594+ReboundQ3@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later
"""Regenerate Resources/Textures/Interface/SV from the upstream Nano chrome textures.

Most of the Nano interface atlas is white or greyscale and gets tinted at runtime by
`.Modulate(palette)`, so it follows whatever palette the active stylesheet declares. Twenty-six
textures are not: the window background, title bar, tab container, line edit, tooltip, checkboxes
and radial menu have their colour baked into the pixels, and their sheetlets apply no modulate.
No palette edit can reach them.

This script re-projects those textures onto the Sector Vestige palette:

    keep each pixel's Oklab lightness and alpha, replace its hue and chroma with the palette's.

Because only hue and chroma move, nine-patch structure, anti-aliasing and the relative contrast
between a texture's own tones all survive untouched.

Pixels are routed to one of three target ramps by their source chroma and hue, so a texture that
mixes chrome with an accent -- a checkbox with a gold tick, a radial menu with a red close button --
is handled correctly without a per-file table. Routing blends smoothly across the chroma threshold
so anti-aliased edges do not speckle.

The palette constants below mirror Content.Client/_SV/Stylesheets/SVPalettes.cs. Change them there
first, then re-run this script to regenerate the textures.

Usage:
    python3 Tools/sv_recolour_interface.py            # regenerate
    python3 Tools/sv_recolour_interface.py --dry-run   # report what would change
"""

import argparse
import math
import shutil
import sys
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    sys.exit("Pillow is required: pip install Pillow")

REPO = Path(__file__).resolve().parent.parent
SRC = REPO / "Resources/Textures/Interface/Nano"
DST = REPO / "Resources/Textures/Interface/SV"

# --- palette, mirroring SVPalettes.cs -------------------------------------------------------

DECK_BASE = "#545459"   # SVPalettes.Deck   -- structural greys
STEEL_BASE = "#a8c6d8"  # SVPalettes.Steel  -- highlight, replaces the NanoTrasen gold
RED_BASE = "#a34649"    # SVPalettes.Red    -- alarm and destructive

# Source pixels below this chroma are chrome and are neutralised. The most chromatic chrome tone
# upstream is the gear icon at 0.0485; the least chromatic accent is the gold tick at 0.0706.
CHROMA_CHROME = 0.055
CHROMA_ACCENT = 0.085

# Source hue windows, in degrees, for pixels chromatic enough to be an accent.
HUE_RED = (-10.0, 50.0)
HUE_GOLD = (50.0, 110.0)

# In-world art that happens to live in the interface atlas. Recolouring these would tint props,
# not chrome.
SKIP = {
    "lined_paper.svg.96dpi.png",
    "inverted_triangle.svg.png",
    "triangle_right.png",
}

ATTRIBUTION = """\
# SPDX-FileCopyrightText: 2026 Sector-Vestige contributors
#
# SPDX-License-Identifier: CC-BY-SA-3.0

# Every texture in this directory is a recoloured derivative of the equivalent file in
# Resources/Textures/Interface/Nano, produced by Tools/sv_recolour_interface.py. Only hue and
# chroma were changed; lightness, alpha and geometry are unmodified. Original authorship and
# licensing carry over from Nano/attributions.yml.
- files: [{files}]
  license: "CC-BY-SA-3.0"
  copyright: "Space Wizards Federation, recoloured for Sector Vestige"
  source: "https://github.com/space-wizards/space-station-14"
"""


# --- Oklab ----------------------------------------------------------------------------------


def _srgb_to_lin(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def _lin_to_srgb(c):
    c = max(0.0, min(1.0, c))
    return c * 12.92 if c <= 0.0031308 else 1.055 * (c ** (1 / 2.4)) - 0.055


def to_oklab(rgb):
    r, g, b = (_srgb_to_lin(c / 255) for c in rgb)
    l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b
    m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b
    s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b
    l_, m_, s_ = (math.copysign(abs(v) ** (1 / 3), v) for v in (l, m, s))
    return (
        0.2104542553 * l_ + 0.7936177850 * m_ - 0.0040720468 * s_,
        1.9779984951 * l_ - 2.4285922050 * m_ + 0.4505937099 * s_,
        0.0259040371 * l_ + 0.7827717662 * m_ - 0.8086757660 * s_,
    )


def from_oklab(lab):
    L, a, b = lab
    l_ = L + 0.3963377774 * a + 0.2158037573 * b
    m_ = L - 0.1055613458 * a - 0.0638541728 * b
    s_ = L - 0.0894841775 * a - 1.2914855480 * b
    l, m, s = l_ ** 3, m_ ** 3, s_ ** 3
    r = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s
    g = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s
    bb = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s
    return tuple(round(_lin_to_srgb(v) * 255) for v in (r, g, bb))


def polar(hexstr):
    """Return the (chroma, hue) a palette base contributes to re-projected pixels."""
    rgb = tuple(int(hexstr.lstrip("#")[i:i + 2], 16) for i in (0, 2, 4))
    _, a, b = to_oklab(rgb)
    return math.hypot(a, b), math.atan2(b, a)


# --- re-projection --------------------------------------------------------------------------


TARGETS = {name: polar(base) for name, base in
           (("chrome", DECK_BASE), ("gold", STEEL_BASE), ("red", RED_BASE))}


def _smoothstep(edge0, edge1, x):
    t = max(0.0, min(1.0, (x - edge0) / (edge1 - edge0)))
    return t * t * (3 - 2 * t)


def reproject(rgb):
    """Keep lightness, adopt the palette's hue and chroma for this pixel's routed ramp."""
    L, a, b = to_oklab(rgb)
    chroma = math.hypot(a, b)
    hue = math.degrees(math.atan2(b, a))

    if HUE_RED[0] <= hue < HUE_RED[1]:
        accent = "red"
    elif HUE_GOLD[0] <= hue < HUE_GOLD[1]:
        accent = "gold"
    else:
        accent = None

    c_chrome, h_chrome = TARGETS["chrome"]
    neutral = (L, c_chrome * math.cos(h_chrome), c_chrome * math.sin(h_chrome))
    if accent is None:
        return from_oklab(neutral)

    c_acc, h_acc = TARGETS[accent]
    semantic = (L, c_acc * math.cos(h_acc), c_acc * math.sin(h_acc))

    # Blend rather than switch, so anti-aliased edges between chrome and accent stay smooth.
    w = _smoothstep(CHROMA_CHROME, CHROMA_ACCENT, chroma)
    return from_oklab(tuple(n + (s - n) * w for n, s in zip(neutral, semantic)))


def pixels(img):
    """Pillow renamed getdata() to get_flattened_data(); support both."""
    getter = getattr(img, "get_flattened_data", None) or img.getdata
    return list(getter())


def is_baked(img):
    """True if any sufficiently opaque pixel carries visible colour."""
    return any(
        px[3] > 32 and (abs(px[0] - px[1]) >= 6 or abs(px[1] - px[2]) >= 6)
        for px in pixels(img)
    )


def main():
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--dry-run", action="store_true", help="report without writing")
    args = ap.parse_args()

    if not SRC.is_dir():
        sys.exit(f"source texture root not found: {SRC}")

    written, skipped, cache = [], [], {}
    for path in sorted(SRC.rglob("*.png")):
        rel = path.relative_to(SRC)
        if rel.name in SKIP:
            skipped.append(f"{rel}  (in-world art)")
            continue

        img = Image.open(path).convert("RGBA")
        if not is_baked(img):
            skipped.append(f"{rel}  (greyscale, recolours at runtime via Modulate)")
            continue

        out = Image.new("RGBA", img.size)
        out.putdata([
            (*(cache.setdefault(px[:3], reproject(px[:3]))), px[3]) if px[3] else px
            for px in pixels(img)
        ])

        target = DST / rel
        if not args.dry_run:
            target.parent.mkdir(parents=True, exist_ok=True)
            out.save(target, "PNG", optimize=True)
            # `sample: filter: true` sidecars control texture filtering. Without them the
            # recoloured chrome renders aliased.
            sidecar = path.with_suffix(path.suffix + ".yml")
            if sidecar.is_file():
                shutil.copy2(sidecar, target.with_suffix(target.suffix + ".yml"))
        written.append(str(rel))

    if not args.dry_run:
        DST.mkdir(parents=True, exist_ok=True)
        files = ",\n           ".join(f'"{f}"' for f in written)
        (DST / "attributions.yml").write_text(ATTRIBUTION.format(files=files), encoding="utf-8")

    verb = "would recolour" if args.dry_run else "recoloured"
    print(f"{verb} {len(written)} baked texture(s) -> {DST.relative_to(REPO)}")
    for f in written:
        print(f"  + {f}")
    print(f"\nskipped {len(skipped)}:")
    for s in skipped:
        print(f"  - {s}")


if __name__ == "__main__":
    main()
