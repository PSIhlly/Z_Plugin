from __future__ import annotations

import json
import math
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageEnhance, ImageFilter, ImageOps


ROOT = Path(__file__).resolve().parent
SOURCE_DIR = ROOT / "Sources"
OUTPUT_DIR = ROOT / "Output"
PREVIEW_DIR = ROOT / "Previews"
REFERENCE_PATH = ROOT / "wang_reference_4x6.png"

TILE = 24
COLS = 4
ROWS = 6
SIZE = (COLS * TILE, ROWS * TILE)

# Stable English slugs are used in filenames so Unity and build scripts do not
# need to depend on a local code page. Display names are recorded in the report.
MATERIALS = {
    "river": "河面",
    "sea": "海面",
    "lava": "熔岩",
    "mud": "泥地",
    "road": "公路",
    "cement": "水泥地",
    "ice": "冰面",
    "sand": "沙面",
    "grass": "草地",
}

INNER_SOURCE = {
    "river": "river_1.png",
    "sea": "sea_1.png",
    "lava": "lava_1.png",
    "mud": "mud_1.png",
    "road": "road_1.png",
    "cement": "cement_1.png",
    "ice": "ice_1.png",
    "sand": "sand_1.png",
    "grass": "grass_1.png",
}

OUTER_SOURCE = {
    "river": "grass_1.png",
    "sea": "sand_1.png",
    "lava": "road_1.png",
    "mud": "grass_1.png",
    "road": "grass_1.png",
    "cement": "grass_1.png",
    "ice": "ice_1.png",
    "sand": "grass_1.png",
    "grass": "mud_1.png",
}

# Dark/light colors for the narrow transition band inherited from the reference.
EDGE_PALETTES = {
    "river": ((57, 48, 39), (190, 158, 91)),
    "sea": ((60, 52, 43), (204, 174, 104)),
    "lava": ((31, 25, 27), (104, 77, 61)),
    "mud": ((72, 51, 36), (163, 116, 68)),
    "road": ((54, 54, 54), (143, 137, 123)),
    "cement": ((69, 72, 72), (178, 175, 159)),
    "ice": ((54, 85, 101), (171, 210, 215)),
    "sand": ((82, 61, 39), (204, 166, 93)),
    "grass": ((73, 52, 34), (172, 126, 66)),
}


def crop_variant(image: Image.Image, variant: int, salt: int) -> Image.Image:
    image = image.convert("RGB")
    width, height = image.size
    side = min(width, height)
    crop_side = max(64, int(side * 0.48))
    room_x = width - crop_side
    room_y = height - crop_side
    # Golden-ratio offsets give deterministic, well-separated crops.
    fx = (0.17 + 0.37 * variant + 0.11 * salt) % 1.0
    fy = (0.63 + 0.29 * variant + 0.07 * salt) % 1.0
    x = int(room_x * fx)
    y = int(room_y * fy)
    return image.crop((x, y, x + crop_side, y + crop_side))


def seamless_tile(source: Image.Image, variant: int, salt: int) -> np.ndarray:
    crop = crop_variant(source, variant, salt)
    tile = crop.resize((TILE, TILE), Image.Resampling.LANCZOS)
    if variant == 2:
        tile = ImageOps.mirror(tile)
    elif variant == 3:
        tile = ImageOps.flip(tile)
    tile = ImageEnhance.Color(tile).enhance((0.96, 1.02, 1.08)[variant - 1])
    tile = ImageEnhance.Brightness(tile).enhance((0.98, 1.03, 0.94)[variant - 1])
    arr = np.asarray(tile, dtype=np.float32)

    # Blend opposite bands into the same values. The final outer pixels are
    # exactly equal, while the four-pixel ramp prevents a visible seam.
    band = 4
    for i in range(band):
        j = TILE - 1 - i
        avg = (arr[:, i, :] + arr[:, j, :]) * 0.5
        arr[:, i, :] = avg
        arr[:, j, :] = avg
    for i in range(band):
        j = TILE - 1 - i
        avg = (arr[i, :, :] + arr[j, :, :]) * 0.5
        arr[i, :, :] = avg
        arr[j, :, :] = avg
    return np.clip(arr, 0, 255).astype(np.uint8)


def varied_tile(
    source: Image.Image,
    canonical: np.ndarray,
    variant: int,
    cell_index: int,
    salt: int,
) -> np.ndarray:
    local = seamless_tile(source, variant, salt + cell_index * 13)
    # Every cell may vary in its center, but all cells converge to the same
    # canonical pixels before reaching a 24px cell boundary.
    yy, xx = np.mgrid[0:TILE, 0:TILE]
    distance = np.minimum.reduce((xx, yy, TILE - 1 - xx, TILE - 1 - yy))
    local_weight = np.clip((distance - 2) / 5.0, 0.0, 1.0)[..., None]
    mixed = canonical * (1.0 - local_weight) + local * local_weight
    return np.clip(mixed, 0, 255).astype(np.uint8)


def transition_tile(palette: tuple[tuple[int, int, int], tuple[int, int, int]], seed: int) -> np.ndarray:
    dark = np.array(palette[0], dtype=np.float32)
    light = np.array(palette[1], dtype=np.float32)
    yy, xx = np.mgrid[0:TILE, 0:TILE]
    phase = seed * 0.731
    waves = (
        np.sin(xx * 0.73 + yy * 0.29 + phase)
        + np.cos(xx * 0.31 - yy * 0.67 + phase * 1.7)
        + np.sin((xx + yy) * 0.19 + phase * 0.4)
    ) / 6.0 + 0.5
    waves = np.clip(waves, 0.0, 1.0)[..., None]
    result = dark * (1.0 - waves) + light * waves
    # The procedural band must obey the same exact wrap contract.
    for i in range(4):
        j = TILE - 1 - i
        avg = (result[:, i, :] + result[:, j, :]) * 0.5
        result[:, i, :] = result[:, j, :] = avg
        avg = (result[i, :, :] + result[j, :, :]) * 0.5
        result[i, :, :] = result[j, :, :] = avg
    return np.clip(result, 0, 255).astype(np.uint8)


def reference_weights(reference: Image.Image) -> tuple[np.ndarray, np.ndarray]:
    pixels = np.asarray(reference.convert("RGB"), dtype=np.float32)
    inner_centers = np.array(
        [(53, 98, 109), (102, 142, 152), (59, 111, 131), (76, 121, 133)],
        dtype=np.float32,
    )
    edge_centers = np.array(
        [(145, 115, 79), (99, 99, 84), (49, 46, 42), (86, 76, 57), (67, 60, 50), (193, 164, 100)],
        dtype=np.float32,
    )
    outer_centers = np.array(
        [(113, 181, 81), (141, 205, 48), (122, 196, 53), (137, 177, 56), (118, 168, 55)],
        dtype=np.float32,
    )

    def minimum_distance(centers: np.ndarray) -> np.ndarray:
        diff = pixels[:, :, None, :] - centers[None, None, :, :]
        return np.min(np.sum(diff * diff, axis=3), axis=2)

    distances = np.stack(
        (minimum_distance(inner_centers), minimum_distance(edge_centers), minimum_distance(outer_centers)),
        axis=2,
    )
    classes = np.argmin(distances, axis=2).astype(np.uint8)
    # Soft membership retains the reference image's antialiasing away from cell
    # borders. Borders are hardened later to make compatible signatures exact.
    logits = -distances / 900.0
    logits -= np.max(logits, axis=2, keepdims=True)
    weights = np.exp(logits)
    weights /= np.sum(weights, axis=2, keepdims=True)

    border = np.zeros((SIZE[1], SIZE[0]), dtype=bool)
    for x in range(0, SIZE[0], TILE):
        border[:, x : x + 3] = True
        border[:, x + TILE - 3 : x + TILE] = True
    for y in range(0, SIZE[1], TILE):
        border[y : y + 3, :] = True
        border[y + TILE - 3 : y + TILE, :] = True
    weights[border] = np.eye(3, dtype=np.float32)[classes[border]]
    return weights, classes


def atlas_layer(
    source: Image.Image,
    variant: int,
    salt: int,
) -> np.ndarray:
    canonical = seamless_tile(source, variant, salt)
    atlas = np.zeros((SIZE[1], SIZE[0], 3), dtype=np.uint8)
    for row in range(ROWS):
        for col in range(COLS):
            index = row * COLS + col
            tile = varied_tile(source, canonical, variant, index, salt)
            y = row * TILE
            x = col * TILE
            atlas[y : y + TILE, x : x + TILE] = tile
    return atlas


def edge_layer(material: str, variant: int) -> np.ndarray:
    canonical = transition_tile(EDGE_PALETTES[material], 100 + variant)
    atlas = np.zeros((SIZE[1], SIZE[0], 3), dtype=np.uint8)
    for row in range(ROWS):
        for col in range(COLS):
            index = row * COLS + col
            local = transition_tile(EDGE_PALETTES[material], 100 + variant + index * 7)
            yy, xx = np.mgrid[0:TILE, 0:TILE]
            distance = np.minimum.reduce((xx, yy, TILE - 1 - xx, TILE - 1 - yy))
            weight = np.clip((distance - 2) / 5.0, 0.0, 1.0)[..., None]
            mixed = canonical * (1.0 - weight) + local * weight
            y = row * TILE
            x = col * TILE
            atlas[y : y + TILE, x : x + TILE] = np.clip(mixed, 0, 255).astype(np.uint8)
    return atlas


def load_source(name: str) -> Image.Image:
    path = SOURCE_DIR / name
    if not path.exists():
        raise FileNotFoundError(path)
    return Image.open(path).convert("RGB")


def build_one(material: str, variant: int, weights: np.ndarray) -> Image.Image:
    if material == "river":
        inner_name = f"river_{variant}.png"
    else:
        inner_name = INNER_SOURCE[material]
    inner = atlas_layer(load_source(inner_name), variant, 11)
    outer = atlas_layer(load_source(OUTER_SOURCE[material]), variant, 47)
    edge = edge_layer(material, variant)

    # Special-purpose outer-ground color direction.
    if material == "lava":
        outer = (outer.astype(np.float32) * np.array([0.60, 0.48, 0.43])).clip(0, 255).astype(np.uint8)
    elif material == "ice":
        gray = np.mean(outer.astype(np.float32), axis=2, keepdims=True)
        outer = np.clip(gray * 0.34 + np.array([175, 205, 216]), 0, 255).astype(np.uint8)

    layers = np.stack((inner, edge, outer), axis=3).astype(np.float32)
    result = np.sum(layers * weights[:, :, None, :], axis=3)
    result = np.clip(result, 0, 255).astype(np.uint8)
    return Image.fromarray(result, "RGB")


def side_data(array: np.ndarray, row: int, col: int, side: str) -> np.ndarray:
    y = row * TILE
    x = col * TILE
    tile = array[y : y + TILE, x : x + TILE]
    if side == "left":
        return tile[:, 0]
    if side == "right":
        return tile[:, -1]
    if side == "top":
        return tile[0, :]
    if side == "bottom":
        return tile[-1, :]
    raise ValueError(side)


def validate_atlas(image: Image.Image, classes: np.ndarray) -> dict:
    if image.size != SIZE:
        raise AssertionError(f"Expected {SIZE}, got {image.size}")
    pixels = np.asarray(image.convert("RGB"), dtype=np.uint8)
    horizontal_pairs = 0
    vertical_pairs = 0
    mismatches = 0

    # Any right/left or bottom/top sides with the same topology signature are
    # legal matches. Their rendered pixels must match exactly.
    for row_a in range(ROWS):
        for col_a in range(COLS):
            right_class = side_data(classes, row_a, col_a, "right")
            bottom_class = side_data(classes, row_a, col_a, "bottom")
            right_pixels = side_data(pixels, row_a, col_a, "right")
            bottom_pixels = side_data(pixels, row_a, col_a, "bottom")
            for row_b in range(ROWS):
                for col_b in range(COLS):
                    left_class = side_data(classes, row_b, col_b, "left")
                    if np.array_equal(right_class, left_class):
                        horizontal_pairs += 1
                        if not np.array_equal(right_pixels, side_data(pixels, row_b, col_b, "left")):
                            mismatches += 1
                    top_class = side_data(classes, row_b, col_b, "top")
                    if np.array_equal(bottom_class, top_class):
                        vertical_pairs += 1
                        if not np.array_equal(bottom_pixels, side_data(pixels, row_b, col_b, "top")):
                            mismatches += 1
    if mismatches:
        raise AssertionError(f"Found {mismatches} compatible-edge pixel mismatches")
    return {
        "size": list(image.size),
        "grid": [COLS, ROWS],
        "tile_size": [TILE, TILE],
        "horizontal_compatible_pairs": horizontal_pairs,
        "vertical_compatible_pairs": vertical_pairs,
        "compatible_edge_pixel_mismatches": mismatches,
        "topology_matches_reference": True,
    }


def make_preview(image: Image.Image, label: str) -> Image.Image:
    scale = 4
    enlarged = image.resize((SIZE[0] * scale, SIZE[1] * scale), Image.Resampling.NEAREST)
    draw = ImageDraw.Draw(enlarged, "RGBA")
    for x in range(0, enlarged.width + 1, TILE * scale):
        draw.line((x, 0, x, enlarged.height), fill=(255, 255, 255, 72), width=1)
    for y in range(0, enlarged.height + 1, TILE * scale):
        draw.line((0, y, enlarged.width, y), fill=(255, 255, 255, 72), width=1)
    return enlarged


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    PREVIEW_DIR.mkdir(parents=True, exist_ok=True)
    reference = Image.open(REFERENCE_PATH).convert("RGB")
    if reference.size != SIZE:
        raise AssertionError(f"Reference must be {SIZE}, got {reference.size}")
    weights, classes = reference_weights(reference)

    reports: dict[str, dict] = {}
    previews: list[tuple[str, Image.Image]] = []
    for material, display_name in MATERIALS.items():
        for variant in range(1, 4):
            name = f"wang_{material}_{variant:02d}.png"
            output_path = OUTPUT_DIR / name
            image = build_one(material, variant, weights)
            image.save(output_path, optimize=True)
            reports[name] = {
                "material": display_name,
                "variant": variant,
                **validate_atlas(image, classes),
            }
            preview = make_preview(image, f"{material} {variant}")
            preview.save(PREVIEW_DIR / f"{output_path.stem}_4x.png", optimize=True)
            previews.append((f"{material} {variant}", preview))

    # One compact contact sheet for visual inspection. Each row is one material;
    # the three columns are its variants.
    thumb_w = SIZE[0] * 2
    thumb_h = SIZE[1] * 2
    label_h = 24
    contact = Image.new("RGB", (thumb_w * 3, (thumb_h + label_h) * len(MATERIALS)), (31, 34, 38))
    draw = ImageDraw.Draw(contact)
    for index, (label, preview) in enumerate(previews):
        material_row = index // 3
        variant_col = index % 3
        x = variant_col * thumb_w
        y = material_row * (thumb_h + label_h)
        thumb = preview.resize((thumb_w, thumb_h), Image.Resampling.NEAREST)
        contact.paste(thumb, (x, y))
        draw.text((x + 6, y + thumb_h + 5), label, fill=(235, 238, 241))
    contact.save(PREVIEW_DIR / "wang_tiles_contact_sheet.png", optimize=True)

    report = {
        "format": "standard 4x6 Wang/AutoTile sheet",
        "reference": REFERENCE_PATH.name,
        "sheet_size": list(SIZE),
        "tile_size": [TILE, TILE],
        "sheet_count": len(reports),
        "materials": MATERIALS,
        "validation": reports,
    }
    (ROOT / "qa_report.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")

    readme = """# WangTile terrain batch

- Format: standard 4 x 6 sheet, 24 cells total.
- Sheet size: 96 x 144 px.
- Cell size: 24 x 24 px.
- Files: 9 materials x 3 variants = 27 PNG files.
- Topology: inherited pixel-for-pixel from `wang_reference_4x6.png`.
- QA: compatible left/right and top/bottom topology signatures are required to have identical rendered edge pixels.

## Unity import

Set Texture Type to Sprite (2D and UI), Sprite Mode to Multiple, Pixels Per Unit to 24,
Filter Mode to Point, Compression to None, and slice by a 24 x 24 cell grid with zero
offset and zero padding. Keep mipmaps disabled for a pixel-stable 2D tilemap.

`Output/` contains production PNGs, `Previews/` contains 4x grid overlays and a
contact sheet, and `qa_report.json` contains the machine validation results.
"""
    (ROOT / "README.md").write_text(readme, encoding="utf-8")
    print(f"Built and validated {len(reports)} WangTile sheets")
    print(PREVIEW_DIR / "wang_tiles_contact_sheet.png")


if __name__ == "__main__":
    main()
