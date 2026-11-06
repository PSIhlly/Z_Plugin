# -*- coding: utf-8 -*-
"""Decisive alpha/fringe check: compare each output against a correctly
resampled reference by compositing both over the same opaque background.
A dark fringe would show up as a large, systematic difference in the alpha
edge band. Also count classic fringe pixels (dark colour carried by partial
alpha), which a correct premultiplied pipeline never produces."""
import os
import numpy as np
from PIL import Image

SRC = r"D:\Works\Unity's\Personal\Z_Plugin\Z_Plugin\ExtraAssets\使用中\伊吹萃香\newVersionHD"
ROOT = r"D:\Works\Unity's\Personal\Z_Plugin\Z_Plugin\ExtraAssets\使用中\伊吹萃香\newVersion"
VARIANTS = ["64_平衡像素", "32_复古像素", "128_高细节像素"]
BG = 128.0


def reference(im, size=128):
    """Correct premultiplied linear-light resize -> straight RGBA float 0..1."""
    a = np.asarray(im.convert("RGBA"), np.float64) / 255.0
    lin = np.where(a[:, :, :3] <= 0.04045, a[:, :, :3] / 12.92,
                   ((a[:, :, :3] + 0.055) / 1.055) ** 2.4)
    alpha = a[:, :, 3]
    pm = np.concatenate([lin * alpha[..., None], alpha[..., None]], 2)
    out = np.empty((size, size, 4))
    for c in range(4):
        ch = Image.fromarray(np.ascontiguousarray(pm[:, :, c], np.float32), "F")
        out[:, :, c] = np.asarray(ch.resize((size, size), Image.BOX), np.float32)
    A = np.clip(out[:, :, 3], 0, 1)
    lin_rgb = out[:, :, :3] / np.maximum(A, 1e-6)[..., None]
    srgb = np.where(lin_rgb <= 0.0031308, lin_rgb * 12.92,
                    1.055 * np.power(np.maximum(lin_rgb, 1e-9), 1 / 2.4) - 0.055)
    return np.clip(srgb, 0, 1), A


def over_bg(rgb, a):
    return rgb * a[..., None] + (BG / 255.0) * (1 - a[..., None])


rows = []
files = sorted(f for f in os.listdir(SRC) if f.lower().endswith(".png"))
for f in files:
    ref_rgb, ref_a = reference(Image.open(os.path.join(SRC, f)))
    ref_c = over_bg(ref_rgb, ref_a)
    interior_ref = ref_a > 0.95

    for v in VARIANTS:
        arr = np.asarray(Image.open(os.path.join(ROOT, v, f)).convert("RGBA"), np.float64) / 255.0
        rgb, a = arr[:, :, :3], arr[:, :, 3]
        comp = over_bg(rgb, a)
        edge = (a > 0.10) & (a < 0.90)
        d_edge = float(np.abs(comp - ref_c)[edge].mean() * 255) if edge.any() else 0.0
        d_all = float(np.abs(comp - ref_c).mean() * 255)

        internal = np.abs(rgb - ref_rgb)[interior_ref].mean() * 255 if interior_ref.any() else 0.0

        lum = rgb @ np.array([.299, .587, .114])
        fringe = int(((a > 0.10) & (a < 0.80) & (lum < 0.12)).sum())
        ref_fringe = int(((ref_a > 0.10) & (ref_a < 0.80) &
                          ((ref_rgb @ np.array([.299, .587, .114])) < 0.12)).sum())
        rows.append((v, f, d_edge, d_all, internal, fringe, ref_fringe))

print(f"{'变体':14s} {'边缘带Δ':>8s} {'整体Δ':>7s} {'主体色Δ':>8s} {'暗色半透明px':>13s}")
for v in VARIANTS:
    sub = [r for r in rows if r[0] == v]
    print(f"{v:14s} {np.mean([r[2] for r in sub]):8.2f} {np.mean([r[3] for r in sub]):7.2f} "
          f"{np.mean([r[4] for r in sub]):8.2f} "
          f"{sum(r[5] for r in sub):6d} (参考 {sum(r[6] for r in sub)})")
    worst = max(sub, key=lambda r: r[2])
    print(f"   最差边缘带: {worst[2]:.2f}  {worst[1][:40]}")
