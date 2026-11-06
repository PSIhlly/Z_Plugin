# -*- coding: utf-8 -*-
"""HD sprites -> 128x128 pixel-art variants.

Pipeline per image:
  1. sRGB -> linear, premultiply alpha
  2. area (BOX) downsample to the target pixel grid in linear light
     -> avoids the darkening/fringe artefacts of naive sRGB averaging
  3. unpremultiply, back to sRGB
  4. mild saturation/contrast lift (pixel art reads better with punchier colour)
  5. palette reduction by weighted k-means (deterministic, alpha-weighted)
  6. nearest-neighbour upscale to exactly 128x128 so pixels stay square blocks
"""
import os
import sys
import numpy as np
from PIL import Image, ImageEnhance, ImageDraw, ImageFont

SRC = r"D:\Works\Unity's\Personal\Z_Plugin\Z_Plugin\ExtraAssets\使用中\伊吹萃香\newVersionHD"
ROOT = r"D:\Works\Unity's\Personal\Z_Plugin\Z_Plugin\ExtraAssets\使用中\伊吹萃香\newVersion"
FONT_PATH = r"C:\Windows\Fonts\msyh.ttc"

TARGET = 128

# name, pixel grid, palette size, saturation, contrast
VARIANTS = [
    ("64_平衡像素", 64, 28, 1.12, 1.06),
    ("32_复古像素", 32, 16, 1.16, 1.10),
    ("128_高细节像素", 128, 48, 1.08, 1.03),
]


# ---------------------------------------------------------------- colour math
def srgb_to_linear(c):
    c = np.asarray(c, dtype=np.float32) / 255.0
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def linear_to_srgb(lin):
    lin = np.clip(lin, 0.0, 1.0)
    s = np.where(lin <= 0.0031308, lin * 12.92,
                 1.055 * np.power(np.maximum(lin, 1e-8), 1.0 / 2.4) - 0.055)
    return np.clip(s, 0.0, 1.0) * 255.0


def _resize_f(arr, size):
    ch = Image.fromarray(np.ascontiguousarray(arr, dtype=np.float32), mode="F")
    return np.asarray(ch.resize((size, size), Image.BOX), dtype=np.float32)


def downsample_premultiplied(im, grid):
    """Area-average in linear light with premultiplied alpha."""
    a = np.asarray(im.convert("RGBA"), dtype=np.uint8).astype(np.float32)
    lin = srgb_to_linear(a[:, :, :3])
    alpha = (a[:, :, 3:4] / 255.0)
    premul = np.concatenate([lin * alpha, alpha], axis=2)

    out = np.empty((grid, grid, 4), dtype=np.float32)
    for c in range(4):
        out[:, :, c] = _resize_f(premul[:, :, c], grid)

    A = np.clip(out[:, :, 3:4], 0.0, 1.0)
    safe = np.maximum(A, 1e-6)
    rgb = linear_to_srgb(out[:, :, :3] / safe)
    rgb = np.where(A > 1e-3, rgb, 0.0)
    return rgb, A


# -------------------------------------------------------------------- palette
def kmeans_palette(rgb, alpha, k, seed=20240819, iters=25):
    """Alpha-weighted k-means++ over the visible pixels only."""
    mask = alpha[:, :, 0] > 0.12
    data = rgb[mask]
    if data.shape[0] == 0:
        return np.zeros((1, 3), np.float32)
    w = alpha[:, :, 0][mask].astype(np.float64)
    k = max(1, min(k, data.shape[0]))

    rng = np.random.default_rng(seed)
    centers = np.empty((k, 3), np.float32)
    centers[0] = data[rng.integers(len(data))]
    d2 = ((data - centers[0]) ** 2).sum(1)
    for i in range(1, k):
        p = d2 * w
        tot = p.sum()
        if tot <= 0:
            centers[i] = data[rng.integers(len(data))]
        else:
            centers[i] = data[rng.choice(len(data), p=p / tot)]
        d2 = np.minimum(d2, ((data - centers[i]) ** 2).sum(1))

    for _ in range(iters):
        lab = ((data[:, None, :] - centers[None, :, :]) ** 2).sum(2).argmin(1)
        moved = 0.0
        for j in range(k):
            sel = lab == j
            if not sel.any():
                continue
            wj = w[sel]
            newc = (data[sel] * wj[:, None]).sum(0) / wj.sum()
            moved = max(moved, float(np.abs(newc - centers[j]).max()))
            centers[j] = newc
        if moved < 0.35:
            break
    return centers


def map_to_palette(rgb, centers, step=4096):
    flat = rgb.reshape(-1, 3)
    out = np.empty_like(flat)
    for i in range(0, len(flat), step):
        chunk = flat[i:i + step]
        d = ((chunk[:, None, :] - centers[None, :, :]) ** 2).sum(2)
        out[i:i + step] = centers[d.argmin(1)]
    return out.reshape(rgb.shape)


# ------------------------------------------------------------------- pipeline
def pixelize(path, grid, colors, sat, contrast):
    im = Image.open(path)
    icc = im.info.get("icc_profile")
    rgb, A = downsample_premultiplied(im, grid)

    # fill transparent pixels with the mean colour so grading/quantising is stable
    mean_col = (rgb * A).reshape(-1, 3).sum(0) / max(float(A.sum()), 1e-6)
    fill = np.where(A > 1e-3, rgb, mean_col.reshape(1, 1, 3))
    img = Image.fromarray(np.clip(fill, 0, 255).astype(np.uint8), "RGB")
    if sat != 1.0:
        img = ImageEnhance.Color(img).enhance(sat)
    if contrast != 1.0:
        img = ImageEnhance.Contrast(img).enhance(contrast)
    graded = np.asarray(img, dtype=np.float32)

    centers = kmeans_palette(graded, A, colors)
    quant = map_to_palette(graded, centers)

    rgb8 = np.clip(quant, 0, 255).astype(np.uint8)
    a8 = np.clip(A[:, :, 0] * 255.0 + 0.5, 0, 255).astype(np.uint8)

    if grid != TARGET:
        rgb8 = np.asarray(Image.fromarray(rgb8, "RGB").resize((TARGET, TARGET), Image.NEAREST))
        a8 = np.asarray(Image.fromarray(a8, "L").resize((TARGET, TARGET), Image.NEAREST))

    out = np.dstack([rgb8, a8])
    result = Image.fromarray(out, "RGBA")
    return result, len(centers), icc


def human(path):
    return os.path.basename(path)


# ---------------------------------------------------------------------- main
def main():
    files = sorted(f for f in os.listdir(SRC) if f.lower().endswith(".png"))
    if not files:
        print("no source images"); return 1

    report = []
    for vname, grid, colors, sat, contrast in VARIANTS:
        vdir = os.path.join(ROOT, vname)
        os.makedirs(vdir, exist_ok=True)
        for f in files:
            src = os.path.join(SRC, f)
            dst = os.path.join(vdir, f)
            res, used, icc = pixelize(src, grid, colors, sat, contrast)
            kw = {"optimize": True}
            if icc:
                kw["icc_profile"] = icc
            res.save(dst, "PNG", **kw)
            report.append({
                "variant": vname, "file": f, "size": res.size,
                "mode": res.mode, "palette_used": used,
                "colors": len(res.getcolors(maxcolors=1 << 24) or []),
                "bytes": os.path.getsize(dst),
            })
            print(f"  {vname:14s} {f}  -> {res.size[0]}x{res.size[1]} "
                  f"{res.mode} palette={used} colors={report[-1]['colors']}")
    return report


if __name__ == "__main__":
    main()
