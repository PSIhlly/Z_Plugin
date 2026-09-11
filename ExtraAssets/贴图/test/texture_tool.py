from __future__ import annotations

import os
import sys
import tempfile
from pathlib import Path

try:
    from PIL import Image, ImageChops, ImageStat
except ImportError:
    print("Pillow is required. Run: python -m pip install Pillow", file=sys.stderr)
    raise SystemExit(1)


FOLDER = Path(__file__).resolve().parent


def crop_closed(image: Image.Image, x1: int, x2: int, y1: int, y2: int) -> Image.Image:
    if x1 < 1 or y1 < 1 or x2 < x1 or y2 < y1 or x2 > image.width or y2 > image.height:
        raise ValueError(f"Crop outside image: [{x1},{x2}] [{y1},{y2}]")
    return image.crop((x1 - 1, y1 - 1, x2, y2))


def crop_half_open(image: Image.Image, x1: int, y1: int, x2: int, y2: int) -> Image.Image:
    if x1 < 0 or y1 < 0 or x2 < x1 or y2 < y1 or x2 > image.width or y2 > image.height:
        raise ValueError(f"Crop outside image: [{x1},{x2}) [{y1},{y2})")
    return image.crop((x1, y1, x2, y2))


def tile(image: Image.Image, columns: int, rows: int) -> Image.Image:
    result = Image.new("RGBA", (image.width * columns, image.height * rows), (0, 0, 0, 0))
    for row in range(rows):
        for column in range(columns):
            result.paste(image, (column * image.width, row * image.height))
    return result


def fill_region(image: Image.Image, x1: int, y1: int, x2: int, y2: int) -> Image.Image:
    result = image.copy()
    result.paste((255, 0, 255, 255), (x1, y1, x2, y2))
    return result


def percentage_box(
    image: Image.Image,
    left: float,
    top: float,
    right: float,
    bottom: float,
) -> tuple[int, int, int, int]:
    return (
        round(image.width * left),
        round(image.height * top),
        round(image.width * right),
        round(image.height * bottom),
    )


def fill_percentage(
    image: Image.Image,
    left: float,
    top: float,
    right: float,
    bottom: float,
) -> Image.Image:
    return fill_region(image, *percentage_box(image, left, top, right, bottom))


def crop_percentage(
    image: Image.Image,
    left: float,
    top: float,
    right: float,
    bottom: float,
) -> Image.Image:
    return image.crop(percentage_box(image, left, top, right, bottom))


def closed_percentage_box(
    image: Image.Image,
    left: float,
    top: float,
    right: float,
    bottom: float,
) -> tuple[int, int, int, int]:
    return (
        round(image.width * left) - 1,
        round(image.height * top) - 1,
        round(image.width * right),
        round(image.height * bottom),
    )


def fill_closed_percentage(
    image: Image.Image,
    left: float,
    top: float,
    right: float,
    bottom: float,
) -> Image.Image:
    return fill_region(image, *closed_percentage_box(image, left, top, right, bottom))


def replace_center_with_ring(
    source: Image.Image,
    replacement: Image.Image,
    inner_box: tuple[int, int, int, int],
    horizontal_thickness: int,
    vertical_thickness: int,
) -> Image.Image:
    result = source.copy()
    inner_left, inner_top, inner_right, inner_bottom = inner_box
    inner_size = (inner_right - inner_left, inner_bottom - inner_top)
    if replacement.size != inner_size:
        replacement = replacement.resize(inner_size, Image.Resampling.LANCZOS)
    result.paste(replacement, (inner_left, inner_top))

    outer_left = max(0, inner_left - horizontal_thickness)
    outer_top = max(0, inner_top - vertical_thickness)
    outer_right = min(result.width, inner_right + horizontal_thickness)
    outer_bottom = min(result.height, inner_bottom + vertical_thickness)
    purple = (255, 0, 255, 255)
    for y in range(outer_top, outer_bottom):
        for x in range(outer_left, outer_right):
            if not (inner_left <= x < inner_right and inner_top <= y < inner_bottom):
                result.putpixel((x, y), purple)
    return result


def crop_right_fraction(image: Image.Image, fraction: float) -> Image.Image:
    keep_width = round(image.width * (1 - fraction))
    return image.crop((0, 0, keep_width, image.height))


def crop_bottom_fraction(image: Image.Image, fraction: float) -> Image.Image:
    keep_height = round(image.height * (1 - fraction))
    return image.crop((0, 0, image.width, keep_height))


def bottom_half(image: Image.Image) -> Image.Image:
    start = image.height // 2
    return image.crop((0, start, image.width, image.height))


def right_half(image: Image.Image) -> Image.Image:
    start = image.width // 2
    return image.crop((start, 0, image.width, image.height))


def stack_vertical(top: Image.Image, bottom: Image.Image) -> Image.Image:
    result = Image.new(
        "RGBA",
        (max(top.width, bottom.width), top.height + bottom.height),
        (0, 0, 0, 0),
    )
    result.paste(top, (0, 0))
    result.paste(bottom, (0, top.height))
    return result


def stack_horizontal(left: Image.Image, right: Image.Image) -> Image.Image:
    result = Image.new(
        "RGBA",
        (left.width + right.width, max(left.height, right.height)),
        (0, 0, 0, 0),
    )
    result.paste(left, (0, 0))
    result.paste(right, (left.width, 0))
    return result


def save_png(image: Image.Image, name: str) -> None:
    destination = FOLDER / name
    fd, temporary_name = tempfile.mkstemp(prefix=f".{name}.", suffix=".tmp", dir=FOLDER)
    os.close(fd)
    temporary = Path(temporary_name)
    try:
        image.save(temporary, format="PNG")
        os.replace(temporary, destination)
    finally:
        if temporary.exists():
            temporary.unlink()


def composite_3x3(
    center: Image.Image,
    left: Image.Image,
    right: Image.Image,
    up: Image.Image,
    down: Image.Image,
) -> Image.Image:
    cell_width, cell_height = center.size
    for part in (left, right, up, down):
        if part.height != cell_height or part.width not in (cell_width, 2 * cell_width):
            raise ValueError("Invalid component size for a 3x3 image.")

    result = Image.new("RGBA", (3 * cell_width, 3 * cell_height), (0, 0, 0, 0))
    result.paste(center, (cell_width, cell_height))
    result.paste(left, (0, cell_height))
    result.paste(right, (2 * cell_width, cell_height))
    result.paste(up, (0 if up.width == 2 * cell_width else cell_width, 0))
    result.paste(down, (0 if down.width == 2 * cell_width else cell_width, 2 * cell_height))
    result.paste((0, 0, 0, 0), (0, 0, cell_width, cell_height))
    result.paste((0, 0, 0, 0), (2 * cell_width, 0, 3 * cell_width, cell_height))
    result.paste((0, 0, 0, 0), (0, 2 * cell_height, cell_width, 3 * cell_height))
    result.paste((0, 0, 0, 0), (2 * cell_width, 2 * cell_height, 3 * cell_width, 3 * cell_height))
    return result


def load_source() -> Image.Image:
    path = FOLDER / "0.png"
    if not path.is_file():
        raise FileNotFoundError(f"Input file not found: {path}")
    with Image.open(path) as image:
        return image.convert("RGBA")


def require_size(image: Image.Image) -> tuple[int, int, int, int]:
    x, y = image.size
    if x % 4 != 0 or y % 6 != 0:
        raise ValueError(f"0.png is {x}x{y}; width must be divisible by 4 and height by 6.")
    return x, y, x // 4, y // 6


def split_image() -> None:
    source = load_source()
    x, y, quarter, sixth = require_size(source)
    half_x = x // 2
    three_quarters_x = x * 3 // 4
    third_y = y // 3
    half_y = y // 2
    two_thirds_y = y * 2 // 3
    five_sixths_y = y * 5 // 6

    center_strip = crop_closed(source, quarter + 1, three_quarters_x, half_y + 1, five_sixths_y)
    image_1 = tile(center_strip, 3, 3)
    save_png(image_1, "1.png")
    save_png(center_part(image_1, center_strip.width, center_strip.height), "50.png")
    image_50 = open_required("50.png")
    save_png(
        replace_center_with_ring(
            source,
            image_50,
            (quarter, half_y, three_quarters_x, five_sixths_y),
            round(x / 8),
            round(y / 12),
        ),
        "51.png",
    )

    image_10 = crop_half_open(source, 0, third_y, half_x, two_thirds_y)
    save_png(image_10, "10.png")
    save_png(fill_percentage(image_10, 0, 0, 1 / 2, 1 / 2), "26.png")

    image_11 = crop_half_open(source, half_x, third_y, x, two_thirds_y)
    save_png(image_11, "11.png")
    save_png(fill_percentage(image_11, 1 / 2, 0, 1, 1 / 2), "27.png")

    image_12 = crop_half_open(source, 0, two_thirds_y, half_x, y)
    save_png(image_12, "12.png")
    save_png(fill_percentage(image_12, 0, 1 / 2, 1 / 2, 1), "28.png")

    image_13 = crop_half_open(source, half_x, two_thirds_y, x, y)
    save_png(image_13, "13.png")
    save_png(fill_percentage(image_13, 1 / 2, 1 / 2, 1, 1), "29.png")

    save_png(fill_percentage(image_1, 1 / 3, 1 / 3, 1 / 2, 1 / 2), "14.png")
    save_png(fill_percentage(image_1, 1 / 2, 1 / 3, 2 / 3, 1 / 2), "15.png")
    save_png(fill_percentage(image_1, 1 / 3, 1 / 2, 1 / 2, 2 / 3), "16.png")
    save_png(fill_percentage(image_1, 1 / 2, 1 / 2, 2 / 3, 2 / 3), "17.png")

    top_strip = crop_closed(source, quarter + 1, three_quarters_x, third_y + 1, half_y)
    image_2 = tile(top_strip, 2, 1)
    save_png(image_2, "2.png")
    image_14_marked = fill_percentage(image_2, 1 / 4, 0, 1 / 2, 1)
    image_14_base = crop_right_fraction(image_14_marked, 1 / 4)
    image_14_added = crop_closed(source, quarter, three_quarters_x, half_y, two_thirds_y)
    image_18 = stack_vertical(image_14_base, image_14_added)
    image_18 = fill_missing_corner(
        image_18,
        image_14_added,
        (0, 0, quarter, sixth),
        (image_14_added.width, image_14_base.height, image_18.width, image_18.height),
    )
    save_png(image_18, "18.png")

    bottom_strip = crop_closed(source, quarter + 1, three_quarters_x, five_sixths_y + 1, y)
    image_3 = tile(bottom_strip, 2, 1)
    save_png(image_3, "3.png")
    image_15_marked = fill_percentage(image_3, 1 / 4, 0, 1 / 2, 1)
    image_15_base = crop_right_fraction(image_15_marked, 1 / 4)
    image_15_added = crop_closed(source, quarter, three_quarters_x, two_thirds_y, five_sixths_y)
    image_19 = stack_vertical(image_15_added, image_15_base)
    image_19 = fill_missing_corner(
        image_19,
        image_15_added,
        (0, 0, quarter, sixth),
        (image_15_added.width, 0, image_19.width, image_15_added.height),
    )
    save_png(image_19, "19.png")

    left_strip = crop_closed(source, 1, quarter, half_y + 1, five_sixths_y)
    image_4 = tile(left_strip, 1, 2)
    save_png(image_4, "4.png")
    image_16_marked = fill_percentage(image_4, 0, 1 / 4, 1, 1 / 2)
    image_16_base = crop_bottom_fraction(image_16_marked, 1 / 4)
    image_16_added = crop_closed(source, quarter, half_x, half_y, five_sixths_y)
    image_20 = stack_horizontal(image_16_base, image_16_added)
    image_20 = fill_missing_corner(
        image_20,
        image_16_added,
        (0, 0, quarter, sixth),
        (image_16_base.width, image_16_added.height, image_20.width, image_20.height),
    )
    save_png(image_20, "20.png")

    right_strip = crop_closed(source, three_quarters_x + 1, x, half_y + 1, five_sixths_y)
    image_5 = tile(right_strip, 1, 2)
    save_png(image_5, "5.png")
    image_17_marked = fill_percentage(image_5, 0, 1 / 4, 1, 1 / 2)
    image_17_base = crop_bottom_fraction(image_17_marked, 1 / 4)
    image_17_added = crop_closed(source, half_x, three_quarters_x, half_y, five_sixths_y)
    image_21 = stack_horizontal(image_17_added, image_17_base)
    image_21 = fill_missing_corner(
        image_21,
        image_17_added,
        (0, 0, quarter, sixth),
        (0, image_17_added.height, image_21.width - image_17_base.width, image_21.height),
    )
    save_png(image_21, "21.png")

    image_6 = composite_3x3(
        crop_closed(source, half_x + 1, three_quarters_x, 1, sixth),
        crop_closed(source, quarter + 1, half_x, third_y + 1, half_y),
        crop_closed(source, quarter + 1, half_x, half_y + 1, two_thirds_y),
        crop_closed(source, 1, quarter, two_thirds_y + 1, five_sixths_y),
        crop_closed(source, half_x + 1, three_quarters_x, half_y + 1, two_thirds_y),
    )
    save_png(image_6, "6.png")
    image_22 = complete_wang_corners(image_6, source, (0, 0))
    save_png(fill_percentage(image_22, 1 / 3, 1 / 3, 2 / 3, 2 / 3), "22.png")

    image_7 = composite_3x3(
        crop_closed(source, three_quarters_x + 1, x, 1, sixth),
        crop_closed(source, half_x + 1, three_quarters_x, two_thirds_y + 1, five_sixths_y),
        crop_closed(source, half_x + 1, three_quarters_x, third_y + 1, half_y),
        crop_closed(source, three_quarters_x + 1, x, half_y + 1, two_thirds_y),
        crop_closed(source, quarter + 1, half_x, half_y + 1, two_thirds_y),
    )
    save_png(image_7, "7.png")
    image_23 = complete_wang_corners(image_7, source, (0, 2))
    save_png(fill_percentage(image_23, 1 / 3, 1 / 3, 2 / 3, 2 / 3), "23.png")

    image_8 = composite_3x3(
        crop_closed(source, half_x + 1, three_quarters_x, sixth + 1, third_y),
        crop_closed(source, quarter + 1, half_x, five_sixths_y + 1, y),
        crop_closed(source, quarter + 1, half_x, half_y + 1, two_thirds_y),
        crop_closed(source, half_x + 1, three_quarters_x, two_thirds_y + 1, five_sixths_y),
        crop_closed(source, 1, quarter, two_thirds_y + 1, five_sixths_y),
    )
    save_png(image_8, "8.png")
    image_24 = complete_wang_corners(image_8, source, (2, 0))
    save_png(fill_percentage(image_24, 1 / 3, 1 / 3, 2 / 3, 2 / 3), "24.png")

    image_9 = composite_3x3(
        crop_closed(source, three_quarters_x + 1, x, sixth + 1, third_y),
        crop_closed(source, half_x + 1, three_quarters_x, half_y + 1, two_thirds_y),
        crop_closed(source, half_x + 1, three_quarters_x, five_sixths_y + 1, y),
        crop_closed(source, quarter + 1, half_x, two_thirds_y + 1, five_sixths_y),
        crop_closed(source, three_quarters_x + 1, x, two_thirds_y + 1, five_sixths_y),
    )
    save_png(image_9, "9.png")
    image_25 = complete_wang_corners(image_9, source, (2, 2))
    image_25.paste(
        crop_half_open(source, 2 * quarter, 3 * sixth, 3 * quarter, 4 * sixth),
        (2 * quarter, 0),
    )
    save_png(fill_percentage(image_25, 1 / 3, 1 / 3, 2 / 3, 2 / 3), "25.png")


def open_required(name: str) -> Image.Image:
    path = FOLDER / name
    if not path.is_file():
        raise FileNotFoundError(f"Input file not found: {path}")
    with Image.open(path) as image:
        return image.convert("RGBA")


def center_part(image: Image.Image, cell_width: int, cell_height: int) -> Image.Image:
    expected = (3 * cell_width, 3 * cell_height)
    if image.size != expected:
        image = image.resize(expected, Image.Resampling.LANCZOS)
    return crop_closed(image, cell_width + 1, 2 * cell_width, cell_height + 1, 2 * cell_height)


def resize_to(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    if image.size == size:
        return image
    return image.resize(size, Image.Resampling.LANCZOS)


def fill_missing_corner(
    image: Image.Image,
    tile_source: Image.Image,
    tile_box: tuple[int, int, int, int],
    destination_box: tuple[int, int, int, int],
) -> Image.Image:
    destination_left, destination_top, destination_right, destination_bottom = destination_box
    destination_size = (
        destination_right - destination_left,
        destination_bottom - destination_top,
    )
    tile = resize_to(tile_source.crop(tile_box), destination_size)
    result = image.copy()
    result.paste(tile, (destination_left, destination_top))
    return result


def tile_water_score(tile: Image.Image) -> float:
    pixels = list(tile.convert("RGBA").getdata())
    visible = [pixel for pixel in pixels if pixel[3] > 32]
    if not visible:
        return -1e9
    blue_bias = sum(pixel[2] - pixel[0] for pixel in visible) / len(visible)
    return blue_bias + 0.25 * sum(pixel[2] - pixel[1] for pixel in visible) / len(visible)


def seam_score(first: Image.Image, second: Image.Image, direction: str) -> float:
    strip = max(1, min(first.width, first.height) // 16)
    if direction == "horizontal":
        first_edge = first.crop((first.width - strip, 0, first.width, first.height))
        second_edge = second.crop((0, 0, strip, second.height))
    else:
        first_edge = first.crop((0, first.height - strip, first.width, first.height))
        second_edge = second.crop((0, 0, second.width, strip))
    difference = ImageChops.difference(first_edge, second_edge)
    means = ImageStat.Stat(difference).mean
    return sum(means[:3]) / 3 + means[3] * 0.25


def complete_wang_corners(
    image: Image.Image,
    source: Image.Image,
    keep_corner: tuple[int, int],
) -> Image.Image:
    cell_width = image.width // 3
    cell_height = image.height // 3
    candidates: list[Image.Image] = []
    for row in range(6):
        for column in range(4):
            candidate = crop_half_open(
                source,
                column * cell_width,
                row * cell_height,
                (column + 1) * cell_width,
                (row + 1) * cell_height,
            )
            if tile_water_score(candidate) > 0:
                candidates.append(candidate)
    if not candidates:
        candidates = [
            crop_half_open(
                source,
                column * cell_width,
                row * cell_height,
                (column + 1) * cell_width,
                (row + 1) * cell_height,
            )
            for row in range(6)
            for column in range(4)
        ]

    def tile_at(row: int, column: int) -> Image.Image:
        return crop_half_open(
            image,
            column * cell_width,
            row * cell_height,
            (column + 1) * cell_width,
            (row + 1) * cell_height,
        )

    corner_rules = {
        (0, 0): (tile_at(0, 1), tile_at(1, 0)),
        (0, 2): (tile_at(0, 1), tile_at(1, 2)),
        (2, 0): (tile_at(1, 0), tile_at(2, 1)),
        (2, 2): (tile_at(1, 2), tile_at(2, 1)),
    }
    result = image.copy()
    for corner, (first_neighbor, second_neighbor) in corner_rules.items():
        if corner == keep_corner:
            continue
        if corner == (0, 0):
            score = lambda candidate: seam_score(candidate, first_neighbor, "horizontal") + seam_score(candidate, second_neighbor, "vertical")
        elif corner == (0, 2):
            score = lambda candidate: seam_score(first_neighbor, candidate, "horizontal") + seam_score(second_neighbor, candidate, "vertical")
        elif corner == (2, 0):
            score = lambda candidate: seam_score(first_neighbor, candidate, "vertical") + seam_score(candidate, second_neighbor, "horizontal")
        else:
            score = lambda candidate: seam_score(first_neighbor, candidate, "vertical") + seam_score(second_neighbor, candidate, "horizontal")
        best = min(candidates, key=score)
        result.paste(best, (corner[1] * cell_width, corner[0] * cell_height))
    return result


def join_horizontal_strip(
    image: Image.Image,
    expected_size: tuple[int, int],
    target_size: tuple[int, int],
    use_bottom: bool,
) -> Image.Image:
    if image.size == expected_size:
        cropped = crop_percentage(image, 0, 0, 1 / 2, 1)
    elif use_bottom:
        cropped = crop_percentage(image, 0, 1 / 2, 2 / 3, 1)
    else:
        cropped = crop_percentage(image, 0, 0, 2 / 3, 1 / 2)
    return resize_to(cropped, target_size)


def join_vertical_strip(
    image: Image.Image,
    expected_size: tuple[int, int],
    target_size: tuple[int, int],
    use_right: bool,
) -> Image.Image:
    if image.size == expected_size:
        cropped = crop_percentage(image, 0, 0, 1, 1 / 2)
    elif use_right:
        cropped = crop_percentage(image, 1 / 2, 0, 1, 2 / 3)
    else:
        cropped = crop_percentage(image, 0, 0, 1 / 2, 2 / 3)
    return resize_to(cropped, target_size)


def join_image() -> None:
    result = load_source()
    _, _, quarter, sixth = require_size(result)
    third_y = 2 * sixth
    images = [None] + [open_required(f"{index}.png") for index in range(1, 10)]
    corner_images = [None] + [open_required(f"{index}.png") for index in range(10, 14)]

    image_1_for_join = resize_to(images[1], (6 * quarter, 6 * sixth))
    center_tile = crop_half_open(image_1_for_join, 2 * quarter, 2 * sixth, 4 * quarter, 4 * sixth)
    replacement_path = FOLDER / "100.png"
    if replacement_path.is_file():
        center_tile = resize_to(open_required("100.png"), center_tile.size)
    result.paste(center_tile, (quarter, 3 * sixth))
    result.paste(
        join_horizontal_strip(images[2], (4 * quarter, sixth), (2 * quarter, sixth), False),
        (quarter, 2 * sixth),
    )
    result.paste(
        join_horizontal_strip(images[3], (4 * quarter, sixth), (2 * quarter, sixth), True),
        (quarter, 5 * sixth),
    )
    result.paste(
        join_vertical_strip(images[4], (quarter, 4 * sixth), (quarter, 2 * sixth), False),
        (0, 3 * sixth),
    )
    result.paste(
        join_vertical_strip(images[5], (quarter, 4 * sixth), (quarter, 2 * sixth), True),
        (3 * quarter, 3 * sixth),
    )
    result.paste(center_part(images[6], quarter, sixth), (2 * quarter, 0))
    result.paste(center_part(images[7], quarter, sixth), (3 * quarter, 0))
    result.paste(center_part(images[8], quarter, sixth), (2 * quarter, sixth))
    result.paste(center_part(images[9], quarter, sixth), (3 * quarter, sixth))
    result.paste(crop_half_open(corner_images[1], 0, 0, quarter, sixth), (0, third_y))
    result.paste(crop_half_open(corner_images[2], quarter, 0, 2 * quarter, sixth), (3 * quarter, third_y))
    result.paste(crop_half_open(corner_images[3], 0, sixth, quarter, 2 * sixth), (0, 5 * sixth))
    result.paste(crop_half_open(corner_images[4], quarter, sixth, 2 * quarter, 2 * sixth), (3 * quarter, 5 * sixth))
    save_png(result, "0.png")


def main() -> int:
    if len(sys.argv) != 2 or sys.argv[1] not in {"split", "join"}:
        print("Usage: texture_tool.py split|join", file=sys.stderr)
        return 2
    try:
        split_image() if sys.argv[1] == "split" else join_image()
    except Exception as error:
        print(f"Processing failed: {error}", file=sys.stderr)
        return 1
    print("Processing completed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
