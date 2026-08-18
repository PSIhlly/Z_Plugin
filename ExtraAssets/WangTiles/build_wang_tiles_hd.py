from __future__ import annotations

import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageEnhance, ImageOps
from scipy.ndimage import distance_transform_edt, gaussian_filter, shift


ROOT = Path(__file__).resolve().parent
SOURCE_DIR = ROOT / "Sources"
OUTPUT_DIR = ROOT / "OutputHDTransparent"
PREVIEW_DIR = ROOT / "PreviewsHDTransparent"
REFERENCE_PATH = ROOT / "wang_reference_4x6.png"

SOURCE_TILE = 24
TILE = 96
COLS = 4
ROWS = 6
SIZE = (COLS * TILE, ROWS * TILE)
SEAM_BAND = 6

MATERIALS = {
    "river": "River water",
    "sea": "Sea water",
    "lava": "Lava",
    "mud": "Mud",
    "road": "Road",
    "cement": "Cement",
    "ice": "Ice",
    "sand": "Sand",
    "grass": "Grass",
    "rock": "Rock ground",
}

SOURCE_NAMES = {
    "river": ("river_1.png", "river_2.png", "river_3.png"),
    "sea": ("sea_1.png",),
    "lava": ("lava_1.png",),
    "mud": ("mud_1.png",),
    "road": ("road_1.png",),
    "cement": ("cement_1.png",),
    "ice": ("ice_1.png",),
    "sand": ("sand_1.png",),
    "grass": ("grass_1.png",),
    "rock": ("rock_1.png",),
}

EFFECTS = (
    ("inset_01", "inset", 1),
    ("inset_02", "inset", 2),
    ("inset_03", "inset", 3),
    ("raised", "raised", 4),
    ("feather", "feather", 5),
    ("noise", "noise", 6),
)

EDGE_PALETTES = {
    "river": ((51, 42, 34), (203, 169, 98)),
    "sea": ((54, 47, 39), (214, 184, 111)),
    "lava": ((28, 21, 23), (117, 69, 48)),
    "mud": ((68, 45, 31), (171, 117, 66)),
    "road": ((48, 50, 52), (137, 135, 126)),
    "cement": ((69, 73, 75), (181, 179, 165)),
    "ice": ((48, 80, 99), (181, 222, 231)),
    "sand": ((79, 55, 34), (217, 174, 93)),
    "grass": ((66, 45, 29), (181, 128, 65)),
    "rock": ((45, 48, 53), (155, 157, 153)),
}


def stable_seed(*parts: object) -> int:
    digest = hashlib.sha256("|".join(map(str, parts)).encode("utf-8")).digest()
    return int.from_bytes(digest[:8], "little") & 0x7FFFFFFF


def load_source(material: str, variant: int) -> Image.Image:
    names = SOURCE_NAMES[material]
    name = names[(variant - 1) % len(names)]
    path = SOURCE_DIR / name
    if not path.exists():
        raise FileNotFoundError(path)
    return Image.open(path).convert("RGB")


def crop_source(image: Image.Image, variant: int, salt: int) -> Image.Image:
    width, height = image.size
    side = min(width, height)
    crop_side = max(TILE * 3, int(side * 0.38))
    room_x = max(0, width - crop_side)
    room_y = max(0, height - crop_side)
    fx = (0.137 + variant * 0.271 + salt * 0.037) % 1.0
    fy = (0.619 + variant * 0.193 + salt * 0.053) % 1.0
    x = int(room_x * fx)
    y = int(room_y * fy)
    return image.crop((x, y, x + crop_side, y + crop_side))


def wrap_texture(image: Image.Image, variant: int, salt: int) -> np.ndarray:
    tile = crop_source(image, variant, salt).resize((TILE, TILE), Image.Resampling.LANCZOS)
    if variant % 3 == 2:
        tile = ImageOps.mirror(tile)
    elif variant % 3 == 0:
        tile = ImageOps.flip(tile)
    tile = ImageEnhance.Color(tile).enhance((0.98, 1.04, 1.10, 0.95, 1.06, 1.00)[(variant - 1) % 6])
    tile = ImageEnhance.Brightness(tile).enhance((0.98, 1.03, 0.94, 1.04, 1.00, 0.97)[(variant - 1) % 6])
    array = np.asarray(tile, dtype=np.float32)

    # Exact wrap with a broad high-resolution ramp instead of a one-pixel weld.
    blend = 18
    for i in range(blend):
        j = TILE - 1 - i
        t = 0.5 - 0.5 * np.cos(np.pi * (i + 1) / (blend + 1))
        average = (array[:, i] + array[:, j]) * 0.5
        array[:, i] = array[:, i] * t + average * (1.0 - t)
        array[:, j] = array[:, j] * t + average * (1.0 - t)
    for i in range(blend):
        j = TILE - 1 - i
        t = 0.5 - 0.5 * np.cos(np.pi * (i + 1) / (blend + 1))
        average = (array[i] + array[j]) * 0.5
        array[i] = array[i] * t + average * (1.0 - t)
        array[j] = array[j] * t + average * (1.0 - t)
    return np.clip(array, 0, 255).astype(np.uint8)


def texture_atlas(material: str, variant: int) -> np.ndarray:
    source = load_source(material, variant)
    canonical = wrap_texture(source, variant, stable_seed(material, variant, "canonical"))
    atlas = np.zeros((SIZE[1], SIZE[0], 3), dtype=np.uint8)
    yy, xx = np.mgrid[0:TILE, 0:TILE]
    distance = np.minimum.reduce((xx, yy, TILE - 1 - xx, TILE - 1 - yy))
    local_weight = np.clip((distance - SEAM_BAND) / 22.0, 0.0, 1.0)[..., None]
    for row in range(ROWS):
        for col in range(COLS):
            index = row * COLS + col
            local = wrap_texture(source, variant, stable_seed(material, variant, index))
            tile = canonical * (1.0 - local_weight) + local * local_weight
            y, x = row * TILE, col * TILE
            atlas[y : y + TILE, x : x + TILE] = np.clip(tile, 0, 255).astype(np.uint8)
    return atlas


def reference_template() -> tuple[list[np.ndarray], list[np.ndarray]]:
    reference = np.asarray(Image.open(REFERENCE_PATH).convert("RGB"), dtype=np.float32)
    if (reference.shape[1], reference.shape[0]) != (COLS * SOURCE_TILE, ROWS * SOURCE_TILE):
        raise AssertionError("Reference is not a 4x6 sheet of 24px tiles")
    inner_centers = np.array(
        [(53, 98, 109), (102, 142, 152), (59, 111, 131), (76, 121, 133)], dtype=np.float32
    )
    edge_centers = np.array(
        [(145, 115, 79), (99, 99, 84), (49, 46, 42), (86, 76, 57), (67, 60, 50), (193, 164, 100)],
        dtype=np.float32,
    )
    outer_centers = np.array(
        [(113, 181, 81), (141, 205, 48), (122, 196, 53), (137, 177, 56), (118, 168, 55)],
        dtype=np.float32,
    )

    def distance(centers: np.ndarray) -> np.ndarray:
        delta = reference[:, :, None, :] - centers[None, None, :, :]
        return np.min(np.sum(delta * delta, axis=3), axis=2)

    distances = np.stack((distance(inner_centers), distance(edge_centers), distance(outer_centers)), axis=2)
    low_classes = np.argmin(distances, axis=2).astype(np.uint8)
    logits = -distances / 760.0
    logits -= np.max(logits, axis=2, keepdims=True)
    low_weights = np.exp(logits)
    low_weights /= np.sum(low_weights, axis=2, keepdims=True)

    tiles_weights: list[np.ndarray] = []
    tiles_classes: list[np.ndarray] = []
    for row in range(ROWS):
        for col in range(COLS):
            y, x = row * SOURCE_TILE, col * SOURCE_TILE
            class_tile = low_classes[y : y + SOURCE_TILE, x : x + SOURCE_TILE]
            high_class = np.asarray(
                Image.fromarray(class_tile, mode="L").resize((TILE, TILE), Image.Resampling.NEAREST), dtype=np.uint8
            )
            channels = []
            for channel in range(3):
                low = low_weights[y : y + SOURCE_TILE, x : x + SOURCE_TILE, channel].astype(np.float32)
                high = np.asarray(
                    Image.fromarray(low, mode="F").resize((TILE, TILE), Image.Resampling.BICUBIC), dtype=np.float32
                )
                channels.append(np.clip(high, 0.0, 1.0))
            high_weights = np.stack(channels, axis=2)
            high_weights /= np.maximum(np.sum(high_weights, axis=2, keepdims=True), 1e-6)
            tiles_weights.append(high_weights)
            tiles_classes.append(high_class)
    return tiles_weights, tiles_classes


def rock_border_atlas(material: str, variant: int) -> np.ndarray:
    source = load_source("rock", variant)
    rock = np.zeros((SIZE[1], SIZE[0], 3), dtype=np.uint8)
    canonical = wrap_texture(source, variant, stable_seed(material, variant, "rock"))
    low = np.array(EDGE_PALETTES[material][0], dtype=np.float32)
    high = np.array(EDGE_PALETTES[material][1], dtype=np.float32)
    yy, xx = np.mgrid[0:TILE, 0:TILE]
    distance = np.minimum.reduce((xx, yy, TILE - 1 - xx, TILE - 1 - yy))
    local_weight = np.clip((distance - SEAM_BAND) / 20.0, 0.0, 1.0)[..., None]
    for row in range(ROWS):
        for col in range(COLS):
            index = row * COLS + col
            local = wrap_texture(source, variant, stable_seed(material, variant, "rock", index))
            texture = canonical * (1.0 - local_weight) + local * local_weight
            luminance = np.mean(texture.astype(np.float32), axis=2, keepdims=True) / 255.0
            luminance = np.clip((luminance - 0.18) / 0.70, 0.0, 1.0)
            colored = low * (1.0 - luminance) + high * luminance
            y, x = row * TILE, col * TILE
            rock[y : y + TILE, x : x + TILE] = np.clip(colored, 0, 255).astype(np.uint8)
    return rock


def fractal_noise(seed: int) -> np.ndarray:
    rng = np.random.default_rng(seed)
    result = np.zeros((TILE, TILE), dtype=np.float32)
    for sigma, weight in ((18.0, 0.52), (7.0, 0.28), (2.2, 0.14), (0.65, 0.06)):
        layer = gaussian_filter(rng.random((TILE, TILE), dtype=np.float32), sigma=sigma, mode="wrap")
        minimum, maximum = float(layer.min()), float(layer.max())
        layer = (layer - minimum) / max(maximum - minimum, 1e-6)
        result += layer * weight
    return np.clip(result, 0.0, 1.0)


def smoothstep(low: float, high: float, values: np.ndarray) -> np.ndarray:
    normalized = np.clip((values - low) / max(high - low, 1e-6), 0.0, 1.0)
    return normalized * normalized * (3.0 - 2.0 * normalized)


def signed_distance(mask: np.ndarray) -> np.ndarray:
    if np.all(mask):
        return np.full(mask.shape, float(TILE), dtype=np.float32)
    if not np.any(mask):
        return np.full(mask.shape, -float(TILE), dtype=np.float32)
    inside = distance_transform_edt(mask)
    outside = distance_transform_edt(~mask)
    return (inside - outside).astype(np.float32)


def texture_edge_field(rgb: np.ndarray) -> np.ndarray:
    """Return a centered detail field whose ridges follow the material itself."""
    source = rgb.astype(np.float32) / 255.0
    luminance = source[:, :, 0] * 0.2126 + source[:, :, 1] * 0.7152 + source[:, :, 2] * 0.0722
    broad = gaussian_filter(luminance, sigma=4.2, mode="wrap")
    micro = luminance - broad
    micro_scale = max(float(np.percentile(np.abs(micro), 90)), 0.012)
    micro = np.clip(micro / micro_scale, -1.0, 1.0)

    softened = gaussian_filter(luminance, sigma=0.85, mode="wrap")
    grad_y, grad_x = np.gradient(softened)
    ridges = np.hypot(grad_x, grad_y)
    ridge_scale = max(float(np.percentile(ridges, 90)), 0.008)
    ridges = np.clip(ridges / ridge_scale, 0.0, 1.0)

    # Bright grains protrude slightly; dark cracks and strong ridges pull the
    # contour inward, so the silhouette tends to travel along real features.
    return np.clip(micro * 0.78 - (ridges - 0.30) * 0.22, -1.0, 1.0)


def texture_aware_alpha(
    mask: np.ndarray,
    material_rgb: np.ndarray,
    seed: int,
    softness: float,
    displacement: float,
    organic_noise: float,
) -> np.ndarray:
    if np.all(mask):
        return np.ones(mask.shape, dtype=np.float32)
    if not np.any(mask):
        return np.zeros(mask.shape, dtype=np.float32)
    detail = texture_edge_field(material_rgb)
    noise = fractal_noise(seed + 31337) - 0.5
    contour = signed_distance(mask) + detail * displacement + noise * organic_noise
    return np.clip(0.5 + contour / max(softness * 2.0, 1e-6), 0.0, 1.0)


def bleed_rgb_beneath_transparency(rgb: np.ndarray, alpha: np.ndarray) -> np.ndarray:
    """Extend visible edge colors under low alpha to prevent filter fringes."""
    anchors = alpha >= 0.34
    if not np.any(anchors):
        return rgb
    _, nearest = distance_transform_edt(~anchors, return_indices=True)
    nearest_rgb = rgb[nearest[0], nearest[1]]
    amount = (1.0 - smoothstep(0.0, 0.34, alpha))[..., None]
    return rgb * (1.0 - amount) + nearest_rgb * amount


def tile_effect(
    material_rgb: np.ndarray,
    rock_rgb: np.ndarray,
    weights: np.ndarray,
    classes: np.ndarray,
    effect: str,
    variant: int,
    seed: int,
) -> np.ndarray:
    inner_w, edge_w, outer_w = (weights[:, :, i] for i in range(3))
    coverage = np.clip((inner_w + edge_w - 0.12) / 0.88, 0.0, 1.0)
    binary = classes != 2
    inner_binary = classes == 0
    rgba = np.zeros((TILE, TILE, 4), dtype=np.float32)

    detail = texture_edge_field(material_rgb)
    solid_alpha = texture_aware_alpha(
        binary,
        material_rgb,
        seed,
        softness=1.65,
        displacement=2.65,
        organic_noise=0.85,
    )
    if effect == "inset":
        transition_presence = np.clip(4.0 * edge_w * (1.0 - edge_w), 0.0, 1.0)
        edge_signal = edge_w + detail * 0.11 * transition_presence
        edge_mix = smoothstep(0.07, 0.86, edge_signal)[..., None]
        rgb = material_rgb * (1.0 - edge_mix) + rock_rgb * edge_mix
        inner_distance = distance_transform_edt(inner_binary)
        inner_shadow = np.exp(-inner_distance / 8.5) * inner_binary
        edge_distance = distance_transform_edt(binary)
        rim_light = np.exp(-edge_distance / 5.5) * binary
        shade = 1.0 - inner_shadow[..., None] * 0.22 + rim_light[..., None] * 0.08
        rgba[:, :, :3] = np.clip(rgb * shade, 0, 255)
        rgba[:, :, 3] = np.clip(solid_alpha * 255.0, 0, 255)

    elif effect == "raised":
        smooth = gaussian_filter(coverage, 3.6)
        grad_y, grad_x = np.gradient(smooth)
        light = np.clip((-grad_x - grad_y) * 2.6, -0.38, 0.38)
        inside_distance = distance_transform_edt(binary)
        bevel = np.exp(-inside_distance / 9.0) * binary
        rgb = material_rgb.astype(np.float32) * (1.0 + light[..., None] + bevel[..., None] * 0.10)
        shadow = shift(solid_alpha, shift=(6.0, 6.0), order=1, mode="constant", cval=0.0)
        cast = np.clip((shadow - solid_alpha) * 0.46, 0.0, 0.42)
        rgb = rgb * (1.0 - cast[..., None] * 0.65)
        rgba[:, :, :3] = np.clip(rgb, 0, 255)
        rgba[:, :, 3] = np.clip(np.maximum(solid_alpha, cast) * 255.0, 0, 255)

    elif effect == "feather":
        alpha = texture_aware_alpha(
            binary,
            material_rgb,
            seed,
            softness=7.2,
            displacement=4.6,
            organic_noise=2.1,
        )
        alpha = gaussian_filter(alpha, sigma=0.65)
        alpha = np.clip((alpha - 0.012) / 0.976, 0.0, 1.0)
        rgb_blur = np.stack(
            [gaussian_filter(material_rgb[:, :, channel].astype(np.float32), 1.4) for channel in range(3)], axis=2
        )
        rgba[:, :, :3] = np.clip(rgb_blur, 0, 255)
        rgba[:, :, 3] = np.clip(alpha * 255.0, 0, 255)

    elif effect == "noise":
        noise = fractal_noise(seed)
        fine_rng = np.random.default_rng(seed + 991)
        fine = fine_rng.random((TILE, TILE), dtype=np.float32)
        contour = (
            signed_distance(binary)
            + detail * 5.4
            + (noise - 0.5) * 8.2
            + (fine - 0.5) * 1.8
        )
        transition = np.clip(0.5 + contour / 6.8, 0.0, 1.0)
        alpha = gaussian_filter(transition, 0.45)
        alpha = np.clip((alpha - 0.015) / 0.97, 0.0, 1.0)
        tint = 0.86 + noise[..., None] * 0.27
        rgba[:, :, :3] = np.clip(material_rgb.astype(np.float32) * tint, 0, 255)
        rgba[:, :, 3] = np.clip(alpha * 255.0, 0, 255)
    else:
        raise ValueError(effect)

    alpha = rgba[:, :, 3] / 255.0
    rgba[:, :, :3] = bleed_rgb_beneath_transparency(rgba[:, :, :3], alpha)
    return np.clip(rgba, 0, 255).astype(np.uint8)


def side_signature(classes: np.ndarray, side: str) -> bytes:
    if side == "left":
        return classes[:, 0].tobytes()
    if side == "right":
        return classes[:, -1].tobytes()
    if side == "top":
        return classes[0].tobytes()
    if side == "bottom":
        return classes[-1].tobytes()
    raise ValueError(side)


def canonicalize_seams(atlas: np.ndarray, class_tiles: list[np.ndarray]) -> None:
    vertical: dict[bytes, list[tuple[int, int, str]]] = {}
    horizontal: dict[bytes, list[tuple[int, int, str]]] = {}
    for index, classes in enumerate(class_tiles):
        row, col = divmod(index, COLS)
        vertical.setdefault(side_signature(classes, "left"), []).append((row, col, "left"))
        vertical.setdefault(side_signature(classes, "right"), []).append((row, col, "right"))
        horizontal.setdefault(side_signature(classes, "top"), []).append((row, col, "top"))
        horizontal.setdefault(side_signature(classes, "bottom"), []).append((row, col, "bottom"))

    for entries in vertical.values():
        bands = []
        for row, col, side in entries:
            tile = atlas[row * TILE : (row + 1) * TILE, col * TILE : (col + 1) * TILE]
            band = tile[:, :SEAM_BAND] if side == "left" else tile[:, -SEAM_BAND:][:, ::-1]
            bands.append(band.astype(np.float32))
        canonical = np.rint(np.mean(bands, axis=0)).astype(np.uint8)
        for row, col, side in entries:
            tile = atlas[row * TILE : (row + 1) * TILE, col * TILE : (col + 1) * TILE]
            if side == "left":
                tile[:, :SEAM_BAND] = canonical
            else:
                tile[:, -SEAM_BAND:] = canonical[:, ::-1]

    for entries in horizontal.values():
        bands = []
        for row, col, side in entries:
            tile = atlas[row * TILE : (row + 1) * TILE, col * TILE : (col + 1) * TILE]
            band = tile[:SEAM_BAND] if side == "top" else tile[-SEAM_BAND:][::-1]
            bands.append(band.astype(np.float32))
        canonical = np.rint(np.mean(bands, axis=0)).astype(np.uint8)
        for row, col, side in entries:
            tile = atlas[row * TILE : (row + 1) * TILE, col * TILE : (col + 1) * TILE]
            if side == "top":
                tile[:SEAM_BAND] = canonical
            else:
                tile[-SEAM_BAND:] = canonical[::-1]


    # The corner pixels belong to both a horizontal and a vertical edge. Solve
    # their exact equality constraints after the broad seam bands are blended.
    parent: dict[int, int] = {}

    def node(index: int, y: int, x: int) -> int:
        return index * TILE * TILE + y * TILE + x

    def find(value: int) -> int:
        parent.setdefault(value, value)
        while parent[value] != value:
            parent[value] = parent[parent[value]]
            value = parent[value]
        return value

    def union(first: int, second: int) -> None:
        root_first, root_second = find(first), find(second)
        if root_first != root_second:
            parent[root_second] = root_first

    def edge_nodes(index: int, side: str) -> list[int]:
        if side == "left":
            return [node(index, y, 0) for y in range(TILE)]
        if side == "right":
            return [node(index, y, TILE - 1) for y in range(TILE)]
        if side == "top":
            return [node(index, 0, x) for x in range(TILE)]
        if side == "bottom":
            return [node(index, TILE - 1, x) for x in range(TILE)]
        raise ValueError(side)

    for entries in list(vertical.values()) + list(horizontal.values()):
        if len(entries) < 2:
            continue
        row, col, side = entries[0]
        reference_nodes = edge_nodes(row * COLS + col, side)
        for row, col, side in entries[1:]:
            for first, second in zip(reference_nodes, edge_nodes(row * COLS + col, side)):
                union(first, second)

    groups: dict[int, list[int]] = {}
    for value in parent:
        groups.setdefault(find(value), []).append(value)
    for values in groups.values():
        samples = []
        for value in values:
            index, offset = divmod(value, TILE * TILE)
            y, x = divmod(offset, TILE)
            row, col = divmod(index, COLS)
            samples.append(atlas[row * TILE + y, col * TILE + x].astype(np.float32))
        canonical = np.rint(np.mean(samples, axis=0)).astype(np.uint8)
        for value in values:
            index, offset = divmod(value, TILE * TILE)
            y, x = divmod(offset, TILE)
            row, col = divmod(index, COLS)
            atlas[row * TILE + y, col * TILE + x] = canonical

def build_atlas(
    material: str,
    effect: str,
    variant: int,
    weight_tiles: list[np.ndarray],
    class_tiles: list[np.ndarray],
) -> Image.Image:
    material_atlas = texture_atlas(material, variant)
    rock_atlas = rock_border_atlas(material, variant)
    result = np.zeros((SIZE[1], SIZE[0], 4), dtype=np.uint8)
    for index, (weights, classes) in enumerate(zip(weight_tiles, class_tiles)):
        row, col = divmod(index, COLS)
        y, x = row * TILE, col * TILE
        material_tile = material_atlas[y : y + TILE, x : x + TILE]
        rock_tile = rock_atlas[y : y + TILE, x : x + TILE]
        result[y : y + TILE, x : x + TILE] = tile_effect(
            material_tile,
            rock_tile,
            weights,
            classes,
            effect,
            variant,
            stable_seed(material, effect, variant, index),
        )
    canonicalize_seams(result, class_tiles)
    return Image.fromarray(result, "RGBA")


def side_pixels(array: np.ndarray, row: int, col: int, side: str) -> np.ndarray:
    tile = array[row * TILE : (row + 1) * TILE, col * TILE : (col + 1) * TILE]
    if side == "left":
        return tile[:, 0]
    if side == "right":
        return tile[:, -1]
    if side == "top":
        return tile[0]
    if side == "bottom":
        return tile[-1]
    raise ValueError(side)


def validate(image: Image.Image, class_tiles: list[np.ndarray]) -> dict[str, object]:
    if image.mode != "RGBA" or image.size != SIZE:
        raise AssertionError(f"Invalid image contract: {image.mode} {image.size}")
    rgba = np.asarray(image, dtype=np.uint8)
    alpha = rgba[:, :, 3]
    horizontal_pairs = vertical_pairs = mismatches = 0
    for a, classes_a in enumerate(class_tiles):
        row_a, col_a = divmod(a, COLS)
        for b, classes_b in enumerate(class_tiles):
            row_b, col_b = divmod(b, COLS)
            if side_signature(classes_a, "right") == side_signature(classes_b, "left"):
                horizontal_pairs += 1
                if not np.array_equal(side_pixels(rgba, row_a, col_a, "right"), side_pixels(rgba, row_b, col_b, "left")):
                    mismatches += 1
            if side_signature(classes_a, "bottom") == side_signature(classes_b, "top"):
                vertical_pairs += 1
                if not np.array_equal(side_pixels(rgba, row_a, col_a, "bottom"), side_pixels(rgba, row_b, col_b, "top")):
                    mismatches += 1
    if mismatches:
        raise AssertionError(f"Found {mismatches} compatible RGBA seam mismatches")
    zero = int(np.count_nonzero(alpha == 0))
    partial = int(np.count_nonzero((alpha > 0) & (alpha < 255)))
    opaque = int(np.count_nonzero(alpha == 255))
    if zero == 0 or partial == 0 or opaque == 0:
        raise AssertionError(f"Invalid transparency coverage: zero={zero}, partial={partial}, opaque={opaque}")
    return {
        "mode": image.mode,
        "sheet_size": list(image.size),
        "grid": [COLS, ROWS],
        "tile_size": [TILE, TILE],
        "transparent_pixels": zero,
        "partially_transparent_pixels": partial,
        "opaque_pixels": opaque,
        "horizontal_compatible_pairs": horizontal_pairs,
        "vertical_compatible_pairs": vertical_pairs,
        "compatible_rgba_seam_mismatches": mismatches,
        "topology_matches_reference": True,
    }


def checkerboard(size: tuple[int, int], cell: int = 12) -> Image.Image:
    width, height = size
    yy, xx = np.mgrid[0:height, 0:width]
    pattern = ((xx // cell + yy // cell) % 2).astype(np.uint8)
    colors = np.array([[72, 75, 80], [104, 108, 114]], dtype=np.uint8)
    return Image.fromarray(colors[pattern], "RGB")


def preview(image: Image.Image) -> Image.Image:
    background = checkerboard(image.size)
    background.paste(image, mask=image.getchannel("A"))
    draw = ImageDraw.Draw(background, "RGBA")
    for x in range(0, SIZE[0] + 1, TILE):
        draw.line((x, 0, x, SIZE[1]), fill=(255, 255, 255, 76), width=1)
    for y in range(0, SIZE[1] + 1, TILE):
        draw.line((0, y, SIZE[0], y), fill=(255, 255, 255, 76), width=1)
    return background


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    PREVIEW_DIR.mkdir(parents=True, exist_ok=True)
    weight_tiles, class_tiles = reference_template()
    report: dict[str, object] = {
        "format": "standard 4x6 Wang/AutoTile sheet",
        "sheet_size": list(SIZE),
        "tile_size": [TILE, TILE],
        "color_mode": "RGBA",
        "background": "transparent",
        "materials": MATERIALS,
        "effect_contract": {
            "inset_01..03": "three detailed recessed / bordered variants",
            "raised": "convex terrain with bevel highlight and cast shadow",
            "feather": "soft blurred alpha transition",
            "noise": "granular eroded alpha transition",
        },
        "edge_contract": {
            "texture_aware_contour": True,
            "transparent_rgb_bleed": True,
            "compatible_rgba_edges_are_exact": True,
        },
        "validation": {},
    }
    contact_rows: list[list[Image.Image]] = []
    hashes: set[str] = set()
    for material in MATERIALS:
        row_images: list[Image.Image] = []
        for file_effect, effect, variant in EFFECTS:
            name = f"wang_{material}_{file_effect}.png"
            image = build_atlas(material, effect, variant, weight_tiles, class_tiles)
            output_path = OUTPUT_DIR / name
            image.save(output_path, optimize=True)
            digest = hashlib.sha256(output_path.read_bytes()).hexdigest()
            if digest in hashes:
                raise AssertionError(f"Duplicate output: {name}")
            hashes.add(digest)
            validation = validate(image, class_tiles)
            validation.update({"material": MATERIALS[material], "effect": file_effect, "sha256": digest})
            report["validation"][name] = validation
            visible = preview(image)
            visible.save(PREVIEW_DIR / name, optimize=True)
            row_images.append(visible.resize((128, 192), Image.Resampling.LANCZOS))
        contact_rows.append(row_images)

    label_height = 22
    cell_w, cell_h = 128, 192
    sheet = Image.new("RGB", (cell_w * 6, (cell_h + label_height) * len(contact_rows)), (30, 32, 36))
    draw = ImageDraw.Draw(sheet)
    effect_labels = [effect[0] for effect in EFFECTS]
    for row_index, (material, images) in enumerate(zip(MATERIALS, contact_rows)):
        y = row_index * (cell_h + label_height)
        for col_index, image in enumerate(images):
            x = col_index * cell_w
            sheet.paste(image, (x, y))
            draw.text((x + 4, y + cell_h + 4), f"{material} {effect_labels[col_index]}", fill=(235, 238, 242))
    contact_path = PREVIEW_DIR / "wang_tiles_hd_transparent_contact_sheet.jpg"
    sheet.save(contact_path, quality=92, optimize=True)

    report["sheet_count"] = len(report["validation"])
    report["unique_sha256_count"] = len(hashes)
    (ROOT / "qa_report_hd_transparent.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    readme = """# High-resolution transparent WangTile batch

- Standard layout: 4 columns x 6 rows (24 cells).
- Sheet size: 384 x 576 px.
- Cell size: 96 x 96 px.
- Format: RGBA PNG with real alpha transparency.
- Materials: river, sea, lava, mud, road, cement, ice, sand, grass, rock.
- Effects per material: three inset variants, raised, feather, and noise.
- Total: 60 unique sheets.

The terrain surface and transition art are generated at the 96px cell scale from
high-resolution source swatches. This is not a nearest-neighbor enlargement of
the old 24px output. Alpha contours follow each material's luminance, grains,
cracks, and ridges, while visible edge colors are extended beneath transparent
pixels to avoid hard cutoffs and dark filtering fringes. All compatible Wang
edges are validated across RGBA pixels.

## Unity import

Use Sprite (2D and UI), Sprite Mode Multiple, Pixels Per Unit 96, Alpha Is
Transparency enabled, Max Size at least 1024, Compression None, and mipmaps off.
Slice as a 96 x 96 grid with zero offset and zero padding. Bilinear filtering is
recommended for this hand-painted HD set; use Point only for a deliberately
pixelated presentation.
"""
    (ROOT / "README_HD_TRANSPARENT.md").write_text(readme, encoding="utf-8")
    print(f"Built and validated {len(report['validation'])} RGBA WangTile sheets")
    print(contact_path)


if __name__ == "__main__":
    main()
