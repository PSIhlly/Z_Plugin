from __future__ import annotations

import argparse
import os
import tempfile
from collections import deque
from dataclasses import dataclass
from pathlib import Path

try:
    import numpy as np
    from PIL import Image
except ImportError as error:
    raise SystemExit(
        "Pillow and NumPy are required. Install them with: "
        "python -m pip install Pillow numpy"
    ) from error


MAGENTA = np.array([255, 0, 255, 255], dtype=np.uint8)


@dataclass(frozen=True)
class Alignment:
    scale: float
    offset_x: int
    offset_y: int
    score: float


def open_rgba(path: Path) -> Image.Image:
    if not path.is_file():
        raise FileNotFoundError(f"Image not found: {path}")
    with Image.open(path) as image:
        return image.convert("RGBA")


def find_magenta_mask(source: np.ndarray) -> np.ndarray:
    rgb = source[:, :, :3]
    alpha = source[:, :, 3]
    return (
        (rgb[:, :, 0] >= 245)
        & (rgb[:, :, 1] <= 10)
        & (rgb[:, :, 2] >= 245)
        & (alpha >= 245)
    )


def dilate_mask(mask: np.ndarray, radius: int) -> np.ndarray:
    if radius <= 0:
        return mask.copy()
    result = mask.copy()
    height, width = mask.shape
    for offset_y in range(-radius, radius + 1):
        for offset_x in range(-radius, radius + 1):
            source_y0 = max(0, -offset_y)
            source_y1 = min(height, height - offset_y)
            source_x0 = max(0, -offset_x)
            source_x1 = min(width, width - offset_x)
            target_y0 = source_y0 + offset_y
            target_y1 = source_y1 + offset_y
            target_x0 = source_x0 + offset_x
            target_x1 = source_x1 + offset_x
            result[target_y0:target_y1, target_x0:target_x1] |= mask[
                source_y0:source_y1, source_x0:source_x1
            ]
    return result


def resize_mask(mask: np.ndarray, size: tuple[int, int]) -> np.ndarray:
    image = Image.fromarray(mask.astype(np.uint8) * 255, mode="L")
    return np.asarray(image.resize(size, Image.Resampling.NEAREST)) > 0


def image_features(image: Image.Image) -> np.ndarray:
    rgb = np.asarray(image.convert("RGB"), dtype=np.float32) / 255.0
    gray = rgb[:, :, 0] * 0.299 + rgb[:, :, 1] * 0.587 + rgb[:, :, 2] * 0.114
    gradient_x = np.zeros_like(gray)
    gradient_y = np.zeros_like(gray)
    gradient_x[:, 1:] = gray[:, 1:] - gray[:, :-1]
    gradient_y[1:, :] = gray[1:, :] - gray[:-1, :]
    return np.dstack((rgb * 0.45, gradient_x[:, :, None], gradient_y[:, :, None]))


def transform_image(
    image: Image.Image,
    size: tuple[int, int],
    scale: float,
    offset_x: float,
    offset_y: float,
    resample: Image.Resampling,
) -> Image.Image:
    width, height = size
    center_x = (width - 1) / 2.0
    center_y = (height - 1) / 2.0
    inverse_scale = 1.0 / scale
    coefficients = (
        inverse_scale,
        0.0,
        center_x * (1.0 - inverse_scale) - offset_x * inverse_scale,
        0.0,
        inverse_scale,
        center_y * (1.0 - inverse_scale) - offset_y * inverse_scale,
    )
    return image.transform(
        size,
        Image.Transform.AFFINE,
        coefficients,
        resample=resample,
        fillcolor=(0, 0, 0, 0),
    )


def score_shift(
    source_features: np.ndarray,
    candidate_features: np.ndarray,
    valid_mask: np.ndarray,
    offset_x: int,
    offset_y: int,
) -> float:
    height, width = valid_mask.shape
    source_x0 = max(0, offset_x)
    source_x1 = min(width, width + offset_x)
    source_y0 = max(0, offset_y)
    source_y1 = min(height, height + offset_y)
    candidate_x0 = max(0, -offset_x)
    candidate_x1 = min(width, width - offset_x)
    candidate_y0 = max(0, -offset_y)
    candidate_y1 = min(height, height - offset_y)
    if source_x0 >= source_x1 or source_y0 >= source_y1:
        return float("inf")

    compared = valid_mask[source_y0:source_y1, source_x0:source_x1]
    if np.count_nonzero(compared) < max(256, int(np.count_nonzero(valid_mask) * 0.55)):
        return float("inf")

    source_part = source_features[source_y0:source_y1, source_x0:source_x1]
    candidate_part = candidate_features[
        candidate_y0:candidate_y1, candidate_x0:candidate_x1
    ]
    difference = np.abs(source_part - candidate_part)
    per_pixel = np.mean(difference, axis=2)
    # Clipping keeps a small generated detail from dominating the global registration.
    return float(np.mean(np.minimum(per_pixel[compared], 0.35)))


def search_stage(
    source: Image.Image,
    generated: Image.Image,
    valid_mask: np.ndarray,
    max_dimension: int,
    scales: np.ndarray,
    center_offset: tuple[float, float],
    offset_radius: int,
    offset_step: int,
) -> Alignment:
    ratio = min(1.0, max_dimension / max(source.size))
    size = (
        max(1, round(source.width * ratio)),
        max(1, round(source.height * ratio)),
    )
    source_small = source.resize(size, Image.Resampling.BILINEAR)
    generated_small = generated.resize(size, Image.Resampling.BILINEAR)
    mask_small = resize_mask(valid_mask, size)
    source_features = image_features(source_small)
    center_x = round(center_offset[0] * size[0] / source.width)
    center_y = round(center_offset[1] * size[1] / source.height)

    best = Alignment(1.0, center_x, center_y, float("inf"))
    for scale in scales:
        scaled = transform_image(
            generated_small,
            size,
            float(scale),
            0,
            0,
            Image.Resampling.BILINEAR,
        )
        candidate_features = image_features(scaled)
        for offset_y in range(
            center_y - offset_radius,
            center_y + offset_radius + 1,
            offset_step,
        ):
            for offset_x in range(
                center_x - offset_radius,
                center_x + offset_radius + 1,
                offset_step,
            ):
                score = score_shift(
                    source_features,
                    candidate_features,
                    mask_small,
                    offset_x,
                    offset_y,
                )
                if score < best.score:
                    best = Alignment(float(scale), offset_x, offset_y, score)

    return Alignment(
        best.scale,
        round(best.offset_x * source.width / size[0]),
        round(best.offset_y * source.height / size[1]),
        best.score,
    )


def estimate_alignment(
    source: Image.Image,
    generated: Image.Image,
    edit_mask: np.ndarray,
    scale_range: float,
    max_shift: int,
    mask_padding: int,
) -> tuple[Alignment, float]:
    source_array = np.asarray(source)
    excluded = dilate_mask(edit_mask, mask_padding)
    valid_mask = (~excluded) & (source_array[:, :, 3] >= 32)
    if np.count_nonzero(valid_mask) < 1024:
        raise ValueError("Not enough visible pixels outside the magenta mask for alignment.")

    normalized_generated = generated.resize(source.size, Image.Resampling.LANCZOS)
    baseline_source = image_features(source)
    baseline_generated = image_features(normalized_generated)
    baseline_score = score_shift(
        baseline_source, baseline_generated, valid_mask, 0, 0
    )

    coarse_scales = np.arange(
        1.0 - scale_range,
        1.0 + scale_range + 0.0001,
        0.01,
        dtype=np.float32,
    )
    coarse_ratio = min(1.0, 160 / max(source.size))
    coarse = search_stage(
        source,
        normalized_generated,
        valid_mask,
        160,
        coarse_scales,
        (0.0, 0.0),
        max(1, round(max_shift * coarse_ratio)),
        2,
    )

    fine_scales = np.arange(
        coarse.scale - 0.012,
        coarse.scale + 0.0121,
        0.002,
        dtype=np.float32,
    )
    fine = search_stage(
        source,
        normalized_generated,
        valid_mask,
        384,
        fine_scales,
        (coarse.offset_x, coarse.offset_y),
        4,
        1,
    )

    final_candidates: list[Alignment] = [Alignment(1.0, 0, 0, baseline_score)]
    for scale in (fine.scale - 0.001, fine.scale, fine.scale + 0.001):
        transformed = transform_image(
            normalized_generated,
            source.size,
            scale,
            0,
            0,
            Image.Resampling.BICUBIC,
        )
        candidate_features = image_features(transformed)
        for offset_y in range(fine.offset_y - 2, fine.offset_y + 3):
            for offset_x in range(fine.offset_x - 2, fine.offset_x + 3):
                score = score_shift(
                    baseline_source,
                    candidate_features,
                    valid_mask,
                    offset_x,
                    offset_y,
                )
                final_candidates.append(Alignment(scale, offset_x, offset_y, score))

    return min(final_candidates, key=lambda item: item.score), baseline_score


def recover_baked_transparency(
    aligned: np.ndarray,
    edit_mask: np.ndarray,
    source: np.ndarray,
) -> int:
    """Recover baked backgrounds connected to a known transparent boundary."""
    rgb = aligned[:, :, :3].astype(np.int16)
    channel_max = rgb.max(axis=2)
    channel_min = rgb.min(axis=2)
    dark_background = channel_max <= 8
    checker_background = (channel_max - channel_min <= 12) & (channel_min >= 150)
    eligible = edit_mask & (dark_background | checker_background)
    height, width = edit_mask.shape
    transparent = np.zeros_like(edit_mask)
    queue: deque[tuple[int, int]] = deque()

    source_transparent = source[:, :, 3] == 0
    for y, x in zip(*np.nonzero(eligible), strict=True):
        touches_canvas = x == 0 or y == 0 or x == width - 1 or y == height - 1
        touches_known_transparency = False
        if not touches_canvas:
            for neighbor_x, neighbor_y in (
                (x - 1, y),
                (x + 1, y),
                (x, y - 1),
                (x, y + 1),
            ):
                if not edit_mask[neighbor_y, neighbor_x] and source_transparent[
                    neighbor_y, neighbor_x
                ]:
                    touches_known_transparency = True
                    break
        if touches_canvas or touches_known_transparency:
            transparent[y, x] = True
            queue.append((x, y))

    while queue:
        x, y = queue.popleft()
        for neighbor_x, neighbor_y in (
            (x - 1, y),
            (x + 1, y),
            (x, y - 1),
            (x, y + 1),
        ):
            if (
                0 <= neighbor_x < width
                and 0 <= neighbor_y < height
                and eligible[neighbor_y, neighbor_x]
                and not transparent[neighbor_y, neighbor_x]
            ):
                transparent[neighbor_y, neighbor_x] = True
                queue.append((neighbor_x, neighbor_y))

    aligned[transparent] = (0, 0, 0, 0)
    return int(np.count_nonzero(transparent))


def atomic_save_png(image: Image.Image, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    descriptor, temporary_name = tempfile.mkstemp(
        prefix=f".{destination.name}.", suffix=".tmp", dir=destination.parent
    )
    os.close(descriptor)
    temporary = Path(temporary_name)
    try:
        image.save(temporary, format="PNG")
        os.replace(temporary, destination)
    finally:
        if temporary.exists():
            temporary.unlink()


def align_and_fill(args: argparse.Namespace) -> None:
    source = open_rgba(args.source)
    generated = open_rgba(args.generated)
    source_array = np.asarray(source).copy()
    edit_mask = find_magenta_mask(source_array)
    mask_pixels = int(np.count_nonzero(edit_mask))
    if mask_pixels == 0:
        raise ValueError("The source image contains no opaque magenta mask pixels.")

    alignment, baseline_score = estimate_alignment(
        source,
        generated,
        edit_mask,
        args.scale_range,
        args.max_shift,
        args.mask_padding,
    )
    normalized_generated = generated.resize(source.size, Image.Resampling.LANCZOS)
    aligned_image = transform_image(
        normalized_generated,
        source.size,
        alignment.scale,
        alignment.offset_x,
        alignment.offset_y,
        Image.Resampling.BICUBIC,
    )
    aligned_array = np.asarray(aligned_image).copy()
    recovered_transparent = 0
    if args.recover_transparency:
        recovered_transparent = recover_baked_transparency(
            aligned_array, edit_mask, source_array
        )

    output_array = source_array.copy()
    output_array[edit_mask] = aligned_array[edit_mask]
    output = Image.fromarray(output_array, mode="RGBA")

    outside_difference = int(
        np.count_nonzero(np.any(output_array[~edit_mask] != source_array[~edit_mask], axis=1))
    )
    remaining_magenta = int(np.count_nonzero(find_magenta_mask(output_array)))
    if output.size != source.size or outside_difference != 0 or remaining_magenta != 0:
        raise RuntimeError(
            "Output verification failed: "
            f"size={output.size}, outside_difference={outside_difference}, "
            f"remaining_magenta={remaining_magenta}"
        )

    atomic_save_png(output, args.output)
    ys, xs = np.nonzero(edit_mask)
    print(f"source_size={source.width}x{source.height}")
    print(f"mask_box={xs.min()},{ys.min()}..{xs.max()},{ys.max()}")
    print(f"mask_pixels={mask_pixels}")
    print(
        "alignment="
        f"scale:{alignment.scale:.6f},"
        f"offset_x:{alignment.offset_x},offset_y:{alignment.offset_y}"
    )
    print(f"alignment_score_before={baseline_score:.8f}")
    print(f"alignment_score_after={alignment.score:.8f}")
    print(f"recovered_transparent_pixels={recovered_transparent}")
    print(f"outside_pixel_difference={outside_difference}")
    print(f"remaining_magenta_pixels={remaining_magenta}")
    print(f"output={args.output}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Align an AI-generated image to unchanged source pixels, then replace only "
            "the source's opaque magenta mask."
        )
    )
    parser.add_argument("source", type=Path, help="Original PNG containing the magenta mask")
    parser.add_argument("generated", type=Path, help="Generated full-image PNG")
    parser.add_argument("output", type=Path, help="Aligned, mask-only output PNG")
    parser.add_argument(
        "--scale-range",
        type=float,
        default=0.08,
        help="Residual uniform scale range around the initial canvas fit (default: 0.08)",
    )
    parser.add_argument(
        "--max-shift",
        type=int,
        default=48,
        help="Maximum absolute X/Y translation in source pixels (default: 48)",
    )
    parser.add_argument(
        "--mask-padding",
        type=int,
        default=4,
        help="Extra source pixels excluded around the magenta mask during alignment (default: 4)",
    )
    parser.add_argument(
        "--no-recover-transparency",
        dest="recover_transparency",
        action="store_false",
        help="Do not convert edge-connected black/checkerboard backgrounds back to alpha 0",
    )
    parser.set_defaults(recover_transparency=True)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    try:
        align_and_fill(args)
    except Exception as error:
        print(f"Processing failed: {error}")
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
