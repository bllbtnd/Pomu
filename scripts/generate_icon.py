#!/usr/bin/env python3
import math
import os
import sys
from PIL import Image, ImageDraw

MASTER = 1024
SUPERSAMPLE = 2
ICONSET_SIZES = [16, 32, 64, 128, 256, 512, 1024]


def rounded_mask(size, radius):
    mask = Image.new("L", (size, size), 0)
    d = ImageDraw.Draw(mask)
    d.rounded_rectangle([0, 0, size - 1, size - 1], radius=radius, fill=255)
    return mask


def point_on_circle(cx, cy, r, deg):
    rad = math.radians(deg)
    return (cx + r * math.cos(rad), cy + r * math.sin(rad))


def draw_calyx(d, cx, cy, inner, outer, leaves, fill):
    pts = []
    for i in range(leaves):
        a = -90 + i * (360 / leaves)
        pts.append(point_on_circle(cx, cy, outer, a))
        pts.append(point_on_circle(cx, cy, inner, a + 360 / (2 * leaves)))
    d.polygon(pts, fill=fill)


def render_master():
    s = MASTER * SUPERSAMPLE
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    d.rounded_rectangle([0, 0, s - 1, s - 1], radius=int(s * 0.225),
                        fill=(255, 244, 232, 255))

    cx, cy = s / 2, s * 0.56
    rx, ry = s * 0.34, s * 0.30

    d.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(214, 48, 49, 255))

    hx, hy = cx - rx * 0.35, cy - ry * 0.40
    hr = rx * 0.42
    highlight = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    hd = ImageDraw.Draw(highlight)
    hd.ellipse([hx - hr, hy - hr, hx + hr, hy + hr], fill=(255, 255, 255, 70))
    img.alpha_composite(highlight)

    d = ImageDraw.Draw(img)
    leaf_cy = cy - ry * 0.92
    draw_calyx(d, cx, leaf_cy, inner=rx * 0.16, outer=rx * 0.46,
               leaves=6, fill=(46, 160, 67, 255))
    d.ellipse([cx - s * 0.018, leaf_cy - ry * 0.34,
               cx + s * 0.018, leaf_cy - ry * 0.10],
              fill=(38, 132, 56, 255))

    mask = rounded_mask(s, int(s * 0.225))
    img.putalpha(mask)
    return img.resize((MASTER, MASTER), Image.LANCZOS)


def main():
    out_dir = sys.argv[1] if len(sys.argv) > 1 else "."
    iconset = os.path.join(out_dir, "Pomu.iconset")
    os.makedirs(iconset, exist_ok=True)

    master = render_master()

    for size in ICONSET_SIZES:
        scaled = master.resize((size, size), Image.LANCZOS)
        if size == 16:
            scaled.save(os.path.join(iconset, "icon_16x16.png"))
        if size == 32:
            scaled.save(os.path.join(iconset, "icon_16x16@2x.png"))
            scaled.save(os.path.join(iconset, "icon_32x32.png"))
        if size == 64:
            scaled.save(os.path.join(iconset, "icon_32x32@2x.png"))
        if size == 128:
            scaled.save(os.path.join(iconset, "icon_128x128.png"))
        if size == 256:
            scaled.save(os.path.join(iconset, "icon_128x128@2x.png"))
            scaled.save(os.path.join(iconset, "icon_256x256.png"))
        if size == 512:
            scaled.save(os.path.join(iconset, "icon_256x256@2x.png"))
            scaled.save(os.path.join(iconset, "icon_512x512.png"))
        if size == 1024:
            scaled.save(os.path.join(iconset, "icon_512x512@2x.png"))

    print(iconset)


if __name__ == "__main__":
    main()
