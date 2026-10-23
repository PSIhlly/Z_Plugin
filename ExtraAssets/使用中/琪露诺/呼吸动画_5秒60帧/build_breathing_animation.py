"""Animate the original transparent standing art without repainting it."""
from __future__ import annotations

import hashlib
import io
import json
import math
import subprocess
import sys
from pathlib import Path

import imageio_ffmpeg
import numpy as np
from PIL import Image, ImageDraw
from scipy.ndimage import map_coordinates


ROOT = Path(__file__).resolve().parent
SOURCE = ROOT.parent / "琪露诺立绘-喜.png"
VIDEO = ROOT / "琪露诺立绘-喜-呼吸5秒60帧.webm"
FRAMES = 60
FPS = 12


def bell(x, y, cx, cy, sx, sy):
    return np.exp(-0.5 * (((x - cx) / sx) ** 2 + ((y - cy) / sy) ** 2))


def main():
    rgba = np.asarray(Image.open(SOURCE).convert("RGBA"), dtype=np.uint8)
    height, width = rgba.shape[:2]
    y, x = np.mgrid[0:height, 0:width].astype(np.float32)
    # All motion fields have broad smooth boundaries, avoiding cut-out seams.
    chest = bell(x, y, 616, 531, 109, 140)
    skirt = bell(x, y, 627, 801, 263, 138)
    hem = bell(x, y, 654, 890, 263, 70)
    sleeves = bell(x, y, 479, 484, 71, 70) + bell(x, y, 726, 394, 57, 68)
    neck_bow = bell(x, y, 646, 486, 66, 51)
    head_bow = bell(x, y, 551, 165, 168, 112)
    hair = bell(x, y, 553, 304, 179, 146)
    alpha = rgba[:, :, 3].astype(np.float32) / 255.0
    premultiplied = rgba.astype(np.float32)
    premultiplied[:, :, :3] *= alpha[:, :, None]
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
    command = [ffmpeg, "-hide_banner", "-loglevel", "warning", "-y",
               "-f", "rawvideo", "-pixel_format", "rgba", "-video_size", f"{width}x{height}",
               "-framerate", str(FPS), "-i", "pipe:0", "-an", "-c:v", "libvpx-vp9",
               "-pix_fmt", "yuva420p", "-lossless", "1", "-auto-alt-ref", "0",
               "-deadline", "good", "-cpu-used", "4", "-threads", "4",
               "-frames:v", str(FRAMES), "-metadata:s:v:0", "alpha_mode=1", str(VIDEO)]
    process = subprocess.Popen(command, stdin=subprocess.PIPE)
    snapshots = []
    hashes = []
    bounds = []
    centers = []
    first = previous = None
    frame_deltas = []
    try:
        for index in range(FRAMES):
            phase = 2.0 * math.pi * index / FRAMES
            breath = math.sin(phase)
            sway = math.sin(phase - 0.52) - math.sin(-0.52)
            flutter = math.sin(phase * 2 - 0.8) - math.sin(-0.8)
            bob = -9.0 * breath
            dx = chest * (x - 616) * 0.0075 * breath
            dy = chest * (y - 560) * 0.0045 * breath
            dx += skirt * (6.0 * sway + 2.0 * flutter * ((y - 665) / 280))
            dy += hem * (2.6 * sway * (x - 627) / 270 + 1.4 * flutter)
            dx += sleeves * 1.8 * sway
            dy += sleeves * 1.35 * breath
            dx += neck_bow * 2.7 * sway + head_bow * 1.6 * sway
            dy += neck_bow * 1.7 * flutter
            dx += hair * 1.3 * sway
            dy += bob
            # Fixed-point inverse warp preserves continuity across regions.
            qx, qy = x - dx, y - dy
            for _ in range(2):
                coords = np.array([qy, qx])
                qx = x - map_coordinates(dx, coords, order=1, mode="nearest", prefilter=False)
                qy = y - map_coordinates(dy, coords, order=1, mode="nearest", prefilter=False)
            coords = np.array([qy, qx])
            warped = np.stack([map_coordinates(premultiplied[:, :, ch], coords,
                order=1, mode="constant", cval=0, prefilter=False) for ch in range(4)], axis=2)
            out_alpha = warped[:, :, 3] / 255.0
            warped[:, :, :3] /= np.maximum(out_alpha[:, :, None], 1e-6)
            warped[out_alpha < 1 / 255.0, :3] = 0
            frame = np.clip(np.rint(warped), 0, 255).astype(np.uint8)
            if index == 0:
                first = frame.copy()
            if previous is not None:
                frame_deltas.append(float(np.abs(frame.astype(np.float32)-previous).mean()))
            previous = frame.astype(np.float32)
            hashes.append(hashlib.sha256(frame.tobytes()).hexdigest())
            visible_y, visible_x = np.nonzero(frame[:, :, 3] > 128)
            bounds.append([int(visible_x.min()), int(visible_y.min()), int(visible_x.max()), int(visible_y.max())])
            centers.append(float(np.average(y, weights=frame[:, :, 3])))
            if index in (0, 15, 30, 45):
                snapshots.append(Image.fromarray(frame, "RGBA"))
            process.stdin.write(frame.tobytes())
            print(f"rendered {index+1}/{FRAMES}", flush=True)
        process.stdin.close()
        if process.wait() != 0:
            raise RuntimeError("WebM encoding failed")
    finally:
        if process.poll() is None:
            process.kill()
            process.wait()

    loop_delta = float(np.abs(first.astype(np.float32) - previous).mean())
    check = subprocess.run([ffmpeg, "-hide_banner", "-loglevel", "error", "-c:v", "libvpx-vp9",
        "-i", str(VIDEO), "-frames:v", "1", "-f", "image2pipe", "-vcodec", "png", "-"],
        check=True, capture_output=True)
    decoded = np.array(Image.open(io.BytesIO(check.stdout)).convert("RGBA"))
    assert np.count_nonzero(decoded[:, :, 3] == 0) > 0
    assert len(set(hashes)) == FRAMES
    assert all(b[0] > 0 and b[1] > 0 and b[2] < width-1 and b[3] < height-1 for b in bounds)
    report = {"source": str(SOURCE), "video": str(VIDEO), "size": [width, height],
        "frames": FRAMES, "fps": FPS, "duration_seconds": FRAMES/FPS,
        "unique_rendered_frames": len(set(hashes)), "decoded_alpha_min": int(decoded[:, :, 3].min()),
        "decoded_transparent_pixels_first_frame": int(np.count_nonzero(decoded[:, :, 3] == 0)),
        "vertical_center_range_px": max(centers)-min(centers),
        "loop_boundary_mean_pixel_delta": loop_delta,
        "adjacent_frame_mean_pixel_delta_range": [min(frame_deltas), max(frame_deltas)],
        "no_canvas_clipping": True, "method": "Original-art smooth deformation; premultiplied-alpha resampling",
        "motion": "5-second periodic breathing with phase-delayed skirt, sleeves, ribbons and hair"}
    (ROOT / "animation_qa.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    preview = Image.new("RGB", (720, 512), (80, 84, 92))
    draw = ImageDraw.Draw(preview)
    for i, snapshot in enumerate(snapshots):
        thumbnail = snapshot.resize((180, 240), Image.Resampling.LANCZOS)
        # Two backgrounds expose dark/light halos and transparency edges.
        for row, color in enumerate(((232, 234, 238), (60, 65, 73))):
            tile = Image.new("RGB", thumbnail.size, color)
            tile.paste(thumbnail, mask=thumbnail.getchannel("A"))
            preview.paste(tile, (i*180, row*256))
            draw.text((i*180+6, row*256+242), f"frame {i*15:02d}", fill=(255,255,255))
    preview.save(ROOT / "motion_preview.jpg", quality=92)
    print(json.dumps(report, ensure_ascii=False), flush=True)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
