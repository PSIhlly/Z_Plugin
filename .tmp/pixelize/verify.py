# -*- coding: utf-8 -*-
"""Verify the three pixel-art variants and render comparison sheets."""
import os
import numpy as np
from PIL import Image, ImageDraw, ImageFont

SRC = r"D:\Works\Unity's\Personal\Z_Plugin\Z_Plugin\ExtraAssets\使用中\伊吹萃香\newVersionHD"
ROOT = r"D:\Works\Unity's\Personal\Z_Plugin\Z_Plugin\ExtraAssets\使用中\伊吹萃香\newVersion"
OUT = r"D:\Works\Unity's\Personal\Z_Plugin\Z_Plugin\.tmp\pixelize\preview"
FONT = r"C:\Windows\Fonts\msyh.ttc"

VARIANTS = [("64_平衡像素", 64), ("32_复古像素", 32), ("128_高细节像素", 128)]
CELL = 256          # 2x of 128 for legible pixel blocks
GRID_DARK, GRID_LIGHT = (74, 74, 74), (104, 104, 104)


def premul_resize(im, w, h, flt=Image.LANCZOS):
    a = np.asarray(im.convert("RGBA"), np.float32) / 255.0
    alpha = a[:, :, 3:4]
    pm = np.concatenate([a[:, :, :3] * alpha, alpha], 2)
    out = np.empty((h, w, 4), np.float32)
    for c in range(4):
        ch = Image.fromarray(np.ascontiguousarray(pm[:, :, c], np.float32), "F")
        out[:, :, c] = np.asarray(ch.resize((w, h), flt), np.float32)
    A = np.clip(out[:, :, 3:4], 0, 1)
    rgb = out[:, :, :3] / np.maximum(A, 1e-6)
    return np.clip(np.concatenate([rgb, A], 2) * 255.0 + 0.5, 0, 255).astype(np.uint8)


def checker(size):
    bg = Image.new("RGB", (size, size), GRID_DARK)
    d = ImageDraw.Draw(bg)
    s = size // 16 or 1
    for y in range(0, size, s):
        for x in range(0, size, s):
            if (x // s + y // s) % 2:
                d.rectangle([x, y, x + s - 1, y + s - 1], fill=GRID_LIGHT)
    return bg


def place(cell_bg, img):
    cell_bg = cell_bg.copy()
    cell_bg.paste(img, (0, 0), img)
    return cell_bg


def main():
    os.makedirs(OUT, exist_ok=True)
    files = sorted(f for f in os.listdir(SRC) if f.lower().endswith(".png"))
    problems = []
    rows_stats = []

    for f in files:
        src_im = Image.open(os.path.join(SRC, f))
        sa = np.asarray(src_im.convert("RGBA"))[:, :, 3]
        s_cov = float((sa > 8).mean()) * 100

        for vname, grid in VARIANTS:
            p = os.path.join(ROOT, vname, f)
            if not os.path.exists(p):
                problems.append(f"缺文件 {vname}/{f}")
                continue
            im = Image.open(p)
            arr = np.asarray(im.convert("RGBA"))
            if im.size != (128, 128):
                problems.append(f"尺寸错误 {vname}/{f} = {im.size}")
            if im.mode != "RGBA":
                problems.append(f"模式错误 {vname}/{f} = {im.mode}")

            # block structure: block x block must be uniform in both colour and alpha
            b = 128 // grid
            if b > 1:
                blk = arr.reshape(grid, b, grid, b, 4).transpose(0, 2, 1, 3, 4)
                blk = blk.reshape(grid, grid, b * b, 4)
                if not (blk == blk[:, :, :1, :]).all():
                    problems.append(f"像素块不均匀 {vname}/{f}")

            cov = float((arr[:, :, 3] > 8).mean()) * 100
            # fringe test: semi-transparent edge pixels must not be much darker
            a = arr[:, :, 3].astype(np.float32)
            lum = arr[:, :, :3].astype(np.float32) @ np.array([.299, .587, .114])
            edge = lum[(a > 51) & (a < 204)]
            solid = lum[a > 230]
            ratio = float(edge.mean() / max(solid.mean(), 1e-6)) if edge.size and solid.size else float("nan")
            if not np.isnan(ratio) and ratio < 0.55:
                problems.append(f"疑似暗边 {vname}/{f} 边缘/主体亮度={ratio:.2f}")

            if vname == "64_平衡像素":
                rows_stats.append((f, s_cov, cov))

    # ---- comparison sheets (2 sheets x 10 rows, 4 columns) ----
    try:
        f_head = ImageFont.truetype(FONT, 17)
        f_row = ImageFont.truetype(FONT, 15)
        f_tag = ImageFont.truetype(FONT, 14)
    except OSError:
        f_head = f_row = f_tag = ImageFont.load_default()

    heads = ["原图（平滑缩到 256）", "64 网格 · 28 色（2×2 块）",
             "32 网格 · 16 色（4×4 块）", "128 直转 · 48 色"]
    pad, gap, label_h, head_h = 10, 8, 22, 34
    sheet_w = pad * 2 + len(heads) * CELL + (len(heads) - 1) * gap

    for page in range(2):
        chunk = files[page * 10:(page + 1) * 10]
        if not chunk:
            continue
        sheet_h = head_h + len(chunk) * (label_h + CELL + gap) + pad
        sheet = Image.new("RGB", (sheet_w, sheet_h), (32, 32, 36))
        d = ImageDraw.Draw(sheet)

        for c, h in enumerate(heads):
            d.text((pad + c * (CELL + gap), 9), h, font=f_head, fill=(236, 236, 240))

        for r, f in enumerate(chunk):
            y = head_h + r * (label_h + CELL + gap)
            label = f"{page * 10 + r + 1:02d}   {f}"
            d.text((pad, y + 2), label[:64], font=f_row, fill=(200, 200, 210))
            y += label_h

            cols = [Image.fromarray(
                premul_resize(Image.open(os.path.join(SRC, f)), CELL, CELL), "RGBA")]
            for vname, _ in VARIANTS:
                im = Image.open(os.path.join(ROOT, vname, f)).convert("RGBA")
                cols.append(im.resize((CELL, CELL), Image.NEAREST))

            for c, img in enumerate(cols):
                x = pad + c * (CELL + gap)
                sheet.paste(place(checker(CELL), img), (x, y))
                d.rectangle([x, y, x + CELL - 1, y + CELL - 1], outline=(20, 20, 22))
                d.text((x + 3, y + CELL - 18), f"{c}", font=f_tag, fill=(255, 255, 255))

        out_png = os.path.join(OUT, f"对比图_{page + 1}.png")
        sheet.save(out_png, "PNG", optimize=True)
        print("SHEET", out_png, sheet.size)

    print("\n检查结果:")
    if problems:
        for p in problems:
            print("  ✗", p)
    else:
        print("  ✓ 60 个文件全部 128x128 RGBA，像素块均匀，无暗边")

    print("\n覆盖率对比 (源图 vs 64网格输出, %, 允许 ±3):")
    worst = max(abs(s - c) for _, s, c in rows_stats)
    for f, s, c in rows_stats[:3]:
        print(f"  {f[:34]:36s} 源={s:5.1f} 输出={c:5.1f}")
    print(f"  ... 最大偏差 {worst:.1f} 个百分点")


if __name__ == "__main__":
    main()
