from __future__ import annotations

import os
import sys
import tempfile
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("Pillow is required. Run: python -m pip install Pillow", file=sys.stderr)
    raise SystemExit(1)


FOLDER = Path(__file__).resolve().parent


def crop_closed(image: Image.Image, x1: int, x2: int, y1: int, y2: int) -> Image.Image:
    if x1 < 1 or y1 < 1 or x2 < x1 or y2 < y1 or x2 > image.width or y2 > image.height:
        raise ValueError(f"Crop outside image: [{x1},{x2}] [{y1},{y2}]")
    return image.crop((x1 - 1, y1 - 1, x2, y2))


def tile(image: Image.Image, columns: int, rows: int) -> Image.Image:
    result = Image.new("RGBA", (image.width * columns, image.height * rows), (0, 0, 0, 0))
    for row in range(rows):
        for column in range(columns):
            result.paste(image, (column * image.width, row * image.height))
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

    save_png(tile(crop_closed(source, quarter + 1, three_quarters_x, half_y + 1, five_sixths_y), 3, 3), "1.png")
    save_png(tile(crop_closed(source, quarter + 1, three_quarters_x, third_y + 1, half_y), 2, 1), "2.png")
    save_png(tile(crop_closed(source, quarter + 1, three_quarters_x, five_sixths_y + 1, y), 2, 1), "3.png")
    save_png(tile(crop_closed(source, 1, quarter, half_y + 1, five_sixths_y), 1, 2), "4.png")
    save_png(tile(crop_closed(source, three_quarters_x + 1, x, half_y + 1, five_sixths_y), 1, 2), "5.png")

    save_png(
        composite_3x3(
            crop_closed(source, half_x + 1, three_quarters_x, 1, sixth),
            crop_closed(source, quarter + 1, half_x, third_y + 1, half_y),
            crop_closed(source, quarter + 1, half_x, half_y + 1, two_thirds_y),
            crop_closed(source, 1, quarter, two_thirds_y + 1, five_sixths_y),
            crop_closed(source, half_x + 1, three_quarters_x, half_y + 1, two_thirds_y),
        ),
        "6.png",
    )
    save_png(
        composite_3x3(
            crop_closed(source, three_quarters_x + 1, x, 1, sixth),
            crop_closed(source, half_x + 1, three_quarters_x, two_thirds_y + 1, five_sixths_y),
            crop_closed(source, half_x + 1, three_quarters_x, third_y + 1, half_y),
            crop_closed(source, three_quarters_x + 1, x, half_y + 1, two_thirds_y),
            crop_closed(source, quarter + 1, half_x, half_y + 1, two_thirds_y),
        ),
        "7.png",
    )
    save_png(
        composite_3x3(
            crop_closed(source, half_x + 1, three_quarters_x, sixth + 1, third_y),
            crop_closed(source, quarter + 1, half_x, five_sixths_y + 1, y),
            crop_closed(source, quarter + 1, half_x, half_y + 1, two_thirds_y),
            crop_closed(source, half_x + 1, three_quarters_x, two_thirds_y + 1, five_sixths_y),
            crop_closed(source, 1, quarter, two_thirds_y + 1, five_sixths_y),
        ),
        "8.png",
    )
    save_png(
        composite_3x3(
            crop_closed(source, three_quarters_x + 1, x, sixth + 1, third_y),
            crop_closed(source, half_x + 1, three_quarters_x, half_y + 1, two_thirds_y),
            crop_closed(source, half_x + 1, three_quarters_x, five_sixths_y + 1, y),
            crop_closed(source, quarter + 1, half_x, two_thirds_y + 1, five_sixths_y),
            crop_closed(source, three_quarters_x + 1, x, two_thirds_y + 1, five_sixths_y),
        ),
        "9.png",
    )


def open_required(name: str) -> Image.Image:
    path = FOLDER / name
    if not path.is_file():
        raise FileNotFoundError(f"Input file not found: {path}")
    with Image.open(path) as image:
        return image.convert("RGBA")


def center_part(image: Image.Image, cell_width: int, cell_height: int) -> Image.Image:
    expected = (3 * cell_width, 3 * cell_height)
    if image.size != expected:
        raise ValueError(f"Expected a 3x3 image of {expected}, got {image.size}.")
    return crop_closed(image, cell_width + 1, 2 * cell_width, cell_height + 1, 2 * cell_height)


def join_image() -> None:
    result = load_source()
    _, _, quarter, sixth = require_size(result)
    images = [None] + [open_required(f"{index}.png") for index in range(1, 10)]

    result.paste(crop_closed(images[1], 2 * quarter + 1, 4 * quarter, 2 * sixth + 1, 4 * sixth), (quarter, 3 * sixth))
    result.paste(crop_closed(images[2], 1, 2 * quarter, 1, sixth), (quarter, 2 * sixth))
    result.paste(crop_closed(images[3], 1, 2 * quarter, 1, sixth), (quarter, 5 * sixth))
    result.paste(crop_closed(images[4], 1, quarter, 1, 2 * sixth), (0, 3 * sixth))
    result.paste(crop_closed(images[5], 1, quarter, 1, 2 * sixth), (3 * quarter, 3 * sixth))
    result.paste(center_part(images[6], quarter, sixth), (2 * quarter, 0))
    result.paste(center_part(images[7], quarter, sixth), (3 * quarter, 0))
    result.paste(center_part(images[8], quarter, sixth), (2 * quarter, sixth))
    result.paste(center_part(images[9], quarter, sixth), (3 * quarter, sixth))
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
