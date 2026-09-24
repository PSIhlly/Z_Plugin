#!/usr/bin/env python3
"""Convert a local WebM animation to a looping GIF and report its properties."""

from __future__ import annotations

import argparse
import io
import re
import shutil
import subprocess
from pathlib import Path


def fail(message: str) -> "NoReturn":
    raise SystemExit(f"error: {message}")


def resolve_ffmpeg(explicit: Path | None) -> str:
    if explicit is not None:
        if not explicit.is_file():
            fail(f"FFmpeg executable does not exist: {explicit}")
        return str(explicit)

    try:
        import imageio_ffmpeg

        return imageio_ffmpeg.get_ffmpeg_exe()
    except (ImportError, RuntimeError):
        path = shutil.which("ffmpeg")
        if path:
            return path
        fail("Could not find FFmpeg. Install imageio-ffmpeg or put ffmpeg on PATH.")


def parse_scale(value: str) -> tuple[int, int]:
    try:
        width_text, height_text = value.lower().split("x", 1)
        width, height = int(width_text), int(height_text)
    except ValueError as exc:
        raise argparse.ArgumentTypeError("scale must use WIDTHxHEIGHT, for example 1086x1448") from exc
    if width <= 0 or height <= 0:
        raise argparse.ArgumentTypeError("scale dimensions must be positive")
    return width, height


def run_checked(command: list[str]) -> subprocess.CompletedProcess[bytes]:
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False)
    if result.returncode != 0:
        details = result.stderr.decode("utf-8", errors="replace").strip()
        fail(f"FFmpeg failed with exit code {result.returncode}:\n{details[-4000:]}")
    return result


def inspect_source(ffmpeg: str, source: Path) -> tuple[tuple[int, int], int, int]:
    """Return (size, transparent pixels, partially transparent pixels) for frame one."""
    from PIL import Image

    result = run_checked(
        [
            ffmpeg,
            "-hide_banner",
            "-loglevel",
            "error",
            "-i",
            str(source),
            "-frames:v",
            "1",
            "-f",
            "image2pipe",
            "-vcodec",
            "png",
            "-",
        ]
    )
    with Image.open(io.BytesIO(result.stdout)) as image:
        rgba = image.convert("RGBA")
        alpha_histogram = rgba.getchannel("A").histogram()
        transparent = sum(alpha_histogram[:1])
        partial = sum(alpha_histogram[1:255])
        return rgba.size, transparent, partial


def inspect_stream(ffmpeg: str, source: Path) -> tuple[int, float]:
    """Return decoded video frame count and duration in seconds."""
    result = subprocess.run(
        [
            ffmpeg,
            "-hide_banner",
            "-loglevel",
            "error",
            "-i",
            str(source),
            "-map",
            "0:v:0",
            "-f",
            "null",
            "NUL",
            "-progress",
            "pipe:1",
            "-nostats",
        ],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        check=False,
    )
    if result.returncode != 0:
        details = result.stderr.decode("utf-8", errors="replace").strip()
        fail(f"FFmpeg stream inspection failed with exit code {result.returncode}:\n{details[-4000:]}")

    progress = result.stdout.decode("ascii", errors="replace")
    frame_matches = re.findall(r"(?:^|\n)frame=(\d+)", progress)
    duration_matches = re.findall(r"(?:^|\n)out_time_us=(\d+)", progress)
    if not frame_matches or not duration_matches:
        fail("Could not determine the source frame count and duration from FFmpeg")
    frame_count = int(frame_matches[-1])
    duration = int(duration_matches[-1]) / 1_000_000
    if frame_count <= 0 or duration <= 0:
        fail("Source video has no usable frames or duration")
    return frame_count, duration


def choose_frame_indices(source_frames: int, target_frames: int, keep_frame: int | None) -> list[int]:
    if target_frames <= 0:
        fail("--frames must be positive")
    if target_frames > source_frames:
        fail(f"--frames ({target_frames}) cannot exceed source frames ({source_frames})")
    if keep_frame is not None and not 0 <= keep_frame < source_frames:
        fail(f"--keep-frame-index must be between 0 and {source_frames - 1}")

    # Floor sampling gives the first frame of each equal source interval. For
    # 120 -> 30 this is 0, 4, 8, ..., 116, which preserves frame 68.
    indices = [(index * source_frames) // target_frames for index in range(target_frames)]
    if keep_frame is not None and keep_frame not in indices:
        replace_positions = list(range(target_frames))
        if target_frames > 2:
            replace_positions = replace_positions[1:-1]
        replace = min(replace_positions, key=lambda position: abs(indices[position] - keep_frame))
        indices[replace] = keep_frame
        indices.sort()
    return indices


def inspect_gif(output: Path) -> dict[str, object]:
    from PIL import Image

    with Image.open(output) as image:
        durations: list[int] = []
        transparent_pixels = 0
        transparency_frames = 0
        for index in range(image.n_frames):
            image.seek(index)
            duration = image.info.get("duration")
            if duration is not None:
                durations.append(int(duration))

            transparent_index = image.info.get("transparency")
            if transparent_index is not None:
                frame_transparent_pixels = sum(
                    1 for pixel in image.convert("P").getdata() if pixel == transparent_index
                )
                if frame_transparent_pixels:
                    transparency_frames += 1
                    transparent_pixels += frame_transparent_pixels

        return {
            "size": image.size,
            "frames": image.n_frames,
            "duration_ms": sum(durations),
            "duration_values_ms": sorted(set(durations)),
            "loop": image.info.get("loop", 0),
            "transparent_pixels": transparent_pixels,
            "transparency_frames": transparency_frames,
        }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("input", type=Path, help="source WebM file")
    parser.add_argument("-o", "--output", type=Path, help="output GIF; defaults to input with a .gif suffix")
    parser.add_argument("--ffmpeg", type=Path, help="explicit FFmpeg executable")
    parser.add_argument("--fps", type=float, help="override output FPS")
    parser.add_argument("--frames", type=int, help="output exactly this many frames while preserving total duration")
    parser.add_argument(
        "--keep-frame-index",
        type=int,
        help="source frame index that must be included; requires --frames",
    )
    parser.add_argument("--scale", type=parse_scale, help="override output size, for example 1086x1448")
    parser.add_argument("--force", action="store_true", help="overwrite an existing output")
    args = parser.parse_args()

    source = args.input.expanduser().resolve()
    if not source.is_file():
        fail(f"input file does not exist: {source}")

    output = (args.output or source.with_suffix(".gif")).expanduser().resolve()
    if output.exists() and not args.force:
        fail(f"output already exists; pass --force to overwrite: {output}")
    output.parent.mkdir(parents=True, exist_ok=True)

    if args.fps is not None and args.fps <= 0:
        fail("FPS must be positive")
    if args.frames is not None and args.frames <= 0:
        fail("--frames must be positive")
    if args.frames is None and args.keep_frame_index is not None:
        fail("--keep-frame-index requires --frames")
    if args.frames is not None and args.fps is not None:
        fail("--frames and --fps cannot be used together")

    try:
        from PIL import Image  # noqa: F401
    except ImportError:
        fail("Pillow is required for alpha and GIF frame inspection")

    ffmpeg = resolve_ffmpeg(args.ffmpeg)
    source_size, source_transparent, source_partial = inspect_source(ffmpeg, source)
    selected_frames: list[int] | None = None
    source_frame_count: int | None = None
    source_duration: float | None = None
    target_fps: float | None = None
    if args.frames is not None:
        source_frame_count, source_duration = inspect_stream(ffmpeg, source)
        selected_frames = choose_frame_indices(source_frame_count, args.frames, args.keep_frame_index)

    transforms = ["format=rgba"]
    if selected_frames is not None:
        selection = "+".join(f"eq(n\\,{index})" for index in selected_frames)
        target_fps = args.frames / source_duration
        transforms.extend([f"select='{selection}'", f"setpts=N/({target_fps:g}*TB)"])
    elif args.fps is not None:
        transforms.append(f"fps={args.fps:g}")
    if args.scale is not None:
        transforms.append(f"scale={args.scale[0]}:{args.scale[1]}:flags=lanczos")
    source_filter = ",".join(transforms)
    filter_complex = (
        f"[0:v]{source_filter},split=2[main][palette];"
        "[palette]palettegen=reserve_transparent=1:stats_mode=full[p];"
        "[main][p]paletteuse=dither=sierra2_4a"
    )

    command = [
        ffmpeg,
        "-hide_banner",
        "-loglevel",
        "warning",
        "-y" if args.force else "-n",
        "-i",
        str(source),
        "-filter_complex",
        filter_complex,
        "-loop",
        "0",
        "-an",
        *( ["-r", f"{target_fps:g}"] if target_fps is not None else [] ),
        str(output),
    ]
    run_checked(command)
    report = inspect_gif(output)

    print(f"output: {output}")
    print(f"size: {report['size'][0]}x{report['size'][1]}")
    print(f"frames: {report['frames']}")
    print(f"duration_ms: {report['duration_ms']}")
    print(f"loop: {report['loop']}")
    print(f"file_bytes: {output.stat().st_size}")
    print(f"source_size: {source_size[0]}x{source_size[1]}")
    print(f"source_transparent_pixels_in_first_frame: {source_transparent}")
    print(f"source_partial_alpha_pixels_in_first_frame: {source_partial}")
    if selected_frames is not None:
        print(f"source_frames: {source_frame_count}")
        print(f"selected_source_frames: {selected_frames}")
    print(f"gif_transparent_pixels: {report['transparent_pixels']}")
    print(f"gif_transparency_frames: {report['transparency_frames']}")
    if source_partial:
        print("warning: GIF only supports binary transparency; partial alpha was quantized.")
    elif not source_transparent:
        print("note: decoded source frame has no transparency; the GIF preserves opaque pixels.")
    return 0


if __name__ == "__main__":
    main()
