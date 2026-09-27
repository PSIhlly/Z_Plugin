"""Make a 5-second transparent breathing loop from the original Marisa art."""
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
PROJECT = ROOT.parents[2]
SOURCE = ROOT.parent / "魔理沙立绘-平静.png"
VIDEO = ROOT / "魔理沙立绘-平静-呼吸5秒60帧.webm"
GIF = VIDEO.with_suffix(".gif")
FRAMES, FPS = 60, 12


def bell(x, y, cx, cy, sx, sy):
    return np.exp(-0.5 * (((x-cx)/sx)**2 + ((y-cy)/sy)**2))


def smoothstep(low, high, value):
    t = np.clip((value-low)/(high-low), 0.0, 1.0)
    return t*t*(3.0-2.0*t)


def main():
    rgba = np.asarray(Image.open(SOURCE).convert("RGBA"), dtype=np.uint8)
    height, width = rgba.shape[:2]
    y, x = np.mgrid[0:height, 0:width].astype(np.float32)
    # Smooth fields fit this standing pose; feet and broom stay anchored.
    upper = 1.0-smoothstep(1010, 1200, y)
    broom_line = 783 + (y-750)*0.335
    cloth_side = 1.0-smoothstep(broom_line-33, broom_line+16, x)
    chest = bell(x, y, 521, 473, 110, 110)
    skirt = bell(x, y, 505, 891, 259, 137)*cloth_side
    hem = bell(x, y, 505, 1022, 295, 77)*cloth_side
    apron = bell(x, y, 486, 758, 134, 156)
    sleeves = bell(x,y,364,454,57,61)+bell(x,y,638,454,54,67)
    waist_bow = bell(x,y,608,603,54,52)
    hair = bell(x,y,373,423,83,121)+bell(x,y,712,445,85,130)
    braid_bow = bell(x,y,583,410,40,33)
    hat_tip = bell(x,y,697,94,66,60)
    premultiplied = rgba.astype(np.float32)
    premultiplied[:,:,:3] *= rgba[:,:,3,None].astype(np.float32)/255.0
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
    command = [ffmpeg, "-hide_banner", "-loglevel", "warning", "-y",
        "-f", "rawvideo", "-pixel_format", "rgba", "-video_size", f"{width}x{height}",
        "-framerate", str(FPS), "-i", "pipe:0", "-an", "-c:v", "libvpx-vp9",
        "-pix_fmt", "yuva420p", "-lossless", "1", "-auto-alt-ref", "0",
        "-deadline", "good", "-cpu-used", "4", "-threads", "4", "-frames:v", str(FRAMES),
        "-metadata:s:v:0", "alpha_mode=1", str(VIDEO)]
    process = subprocess.Popen(command, stdin=subprocess.PIPE)
    hashes, snapshots, bounds, frame_deltas = [], [], [], []
    first = previous = None
    try:
        for index in range(FRAMES):
            phase = 2*math.pi*index/FRAMES
            breath = math.sin(phase)
            sway = math.sin(phase-.55)-math.sin(-.55)
            flutter = math.sin(phase*2-.85)-math.sin(-.85)
            dx = chest*(x-521)*.010*breath
            dy = -3.0*breath*upper + chest*(y-553)*.009*breath
            dx += skirt*(3.8*sway+1.2*flutter*(y-670)/380)
            dx += skirt*(x-505)*.0032*breath
            dy += hem*(2.0*sway*(x-505)/295+1.1*flutter)
            dx += apron*1.8*sway
            dy += apron*1.2*breath
            dx += sleeves*.9*sway + waist_bow*2.5*sway + braid_bow*1.1*sway
            dy += sleeves*.8*breath + waist_bow*.9*flutter
            dx += hair*2.8*sway + hat_tip*.9*sway
            dy += hair*.6*flutter
            # Avoid applying clothing deformation to the adjacent broom.
            dx *= cloth_side
            dy *= cloth_side
            qx, qy = x-dx, y-dy
            for _ in range(2):
                coords = np.array([qy,qx])
                qx = x-map_coordinates(dx,coords,order=1,mode="nearest",prefilter=False)
                qy = y-map_coordinates(dy,coords,order=1,mode="nearest",prefilter=False)
            coords = np.array([qy,qx])
            warped = np.stack([map_coordinates(premultiplied[:,:,ch],coords,order=1,
                mode="constant",cval=0,prefilter=False) for ch in range(4)],axis=2)
            out_alpha = warped[:,:,3]/255.0
            warped[:,:,:3] /= np.maximum(out_alpha[:,:,None],1e-6)
            warped[out_alpha < 1/255.0,:3] = 0
            frame = np.clip(np.rint(warped),0,255).astype(np.uint8)
            if first is None:
                first = frame.copy()
            if previous is not None:
                frame_deltas.append(float(np.abs(frame.astype(np.float32)-previous).mean()))
            previous = frame.astype(np.float32)
            hashes.append(hashlib.sha256(frame.tobytes()).hexdigest())
            vy,vx = np.nonzero(frame[:,:,3]>128)
            bounds.append([int(vx.min()),int(vy.min()),int(vx.max()),int(vy.max())])
            if index in (0,15,30,45):
                snapshots.append(Image.fromarray(frame,"RGBA"))
            process.stdin.write(frame.tobytes())
            print(f"rendered {index+1}/{FRAMES}",flush=True)
        process.stdin.close()
        if process.wait() != 0:
            raise RuntimeError("WebM encoding failed")
    finally:
        if process.poll() is None:
            process.kill()
            process.wait()

    assert len(set(hashes)) == FRAMES
    assert all(b[0]>0 and b[1]>0 and b[2]<width-1 and b[3]<height-1 for b in bounds)
    decoded_result = subprocess.run([ffmpeg,"-hide_banner","-loglevel","error",
        "-c:v","libvpx-vp9","-i",str(VIDEO),"-frames:v","1","-f","image2pipe",
        "-vcodec","png","-"],capture_output=True,check=True)
    decoded = Image.open(io.BytesIO(decoded_result.stdout)).convert("RGBA")
    source_transparent = decoded.getchannel("A").histogram()[0]
    assert source_transparent>0
    # Invoke the actual project skill, keeping its local edits untouched.
    subprocess.run([sys.executable,str(PROJECT/".agents/skills/webm-to-gif/scripts/convert_webm_to_gif.py"),
        str(VIDEO),"--output",str(GIF),"--frames",str(FRAMES)],check=True)
    with Image.open(GIF) as image:
        gif_hashes, durations, transparent = [],[],[]
        for index in range(image.n_frames):
            image.seek(index)
            frame = image.convert("RGBA")
            gif_hashes.append(hashlib.sha256(frame.tobytes()).hexdigest())
            durations.append(image.info.get("duration",0))
            transparent.append(frame.getchannel("A").histogram()[0])
        gif_report = {"frames":image.n_frames,"unique_decoded_frames":len(set(gif_hashes)),
            "size":list(image.size),"duration_ms":sum(durations),"frame_durations_ms":sorted(set(durations)),
            "loop":image.info.get("loop"),"frames_with_transparency":sum(v>0 for v in transparent),
            "min_transparent_pixels_per_frame":min(transparent)}
    assert gif_report["frames"]==60 and gif_report["unique_decoded_frames"]==60
    assert gif_report["duration_ms"]==5000 and gif_report["frames_with_transparency"]==60
    report = {"source":str(SOURCE),"video":str(VIDEO),"gif":str(GIF),"size":[width,height],
        "frames":FRAMES,"fps":FPS,"duration_seconds":FRAMES/FPS,
        "unique_rendered_frames":len(set(hashes)),"no_visible_canvas_clipping":True,
        "decoded_webm_transparent_pixels_first_frame":source_transparent,
        "loop_boundary_mean_pixel_delta":float(np.abs(first.astype(np.float32)-previous).mean()),
        "adjacent_frame_mean_pixel_delta_range":[min(frame_deltas),max(frame_deltas)],
        "method":"Original-art smooth local deformation; premultiplied-alpha resampling",
        "motion":"Chest and shoulders breathe; feet stay planted; apron, skirt, hair and ribbons sway",
        "conversion_skill":".agents/skills/webm-to-gif/scripts/convert_webm_to_gif.py",
        "gif_validation":gif_report,"gif_bytes":GIF.stat().st_size,"webm_bytes":VIDEO.stat().st_size,
        "alpha_note":"GIF has binary transparency; WebM retains partial alpha."}
    (ROOT/"animation_qa.json").write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
    preview = Image.new("RGB",(720,512),(80,84,92))
    draw = ImageDraw.Draw(preview)
    for i,snapshot in enumerate(snapshots):
        thumbnail = snapshot.resize((180,240),Image.Resampling.LANCZOS)
        for row,color in enumerate(((232,234,238),(60,65,73))):
            tile = Image.new("RGB",thumbnail.size,color)
            tile.paste(thumbnail,mask=thumbnail.getchannel("A"))
            preview.paste(tile,(i*180,row*256))
            draw.text((i*180+6,row*256+242),f"frame {i*15:02d}",fill=(255,255,255))
    preview.save(ROOT/"motion_preview.jpg",quality=92)
    print(json.dumps(report,ensure_ascii=False),flush=True)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
