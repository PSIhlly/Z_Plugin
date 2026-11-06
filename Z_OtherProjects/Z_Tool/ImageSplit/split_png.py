"""Split every PNG beside this script into an evenly distributed row/column grid."""
from __future__ import annotations

import argparse
from pathlib import Path
import sys

try:
    from PIL import Image
except ImportError:
    print("缺少 Pillow，请先运行：python -m pip install Pillow", file=sys.stderr)
    raise SystemExit(1)


def positive_int(text: str) -> int:
    try:
        value = int(text)
    except ValueError:
        raise argparse.ArgumentTypeError("请输入正整数。") from None
    if value <= 0:
        raise argparse.ArgumentTypeError("行数和列数必须大于 0。")
    return value


def ask_count(label: str) -> int:
    while True:
        try:
            return positive_int(input(f"请输入{label}：").strip())
        except argparse.ArgumentTypeError as error:
            print(error)


def new_output_directory(root: Path, name: str) -> Path:
    root.mkdir(parents=True, exist_ok=True)
    version = 1
    while True:
        target = root / (name if version == 1 else f"{name}_{version}")
        try:
            target.mkdir()
            return target
        except FileExistsError:
            version += 1


def split_png(source: Path, rows: int, columns: int, output_root: Path) -> Path:
    if rows <= 0 or columns <= 0:
        raise ValueError("行数和列数必须大于 0。")
    with Image.open(source) as image:
        if image.format != "PNG":
            raise ValueError("文件内容不是 PNG。")
        image.load()
        width, height = image.size
        if columns > width or rows > height:
            raise ValueError(f"图片为 {width}×{height}，行数不能超过高度，列数不能超过宽度。")
        x_edges = [column * width // columns for column in range(columns + 1)]
        y_edges = [row * height // rows for row in range(rows + 1)]
        output = new_output_directory(output_root, f"{source.stem}_{rows}x{columns}")
        row_digits, column_digits = max(2, len(str(rows))), max(2, len(str(columns)))
        try:
            for row in range(rows):
                for column in range(columns):
                    box = (x_edges[column], y_edges[row], x_edges[column + 1], y_edges[row + 1])
                    name = f"r{row + 1:0{row_digits}d}_c{column + 1:0{column_digits}d}.png"
                    image.crop(box).save(output / name, format="PNG")
        except OSError as error:
            raise OSError(f"输出未完成，已生成的部分切片保留在 {output}；{error}") from error
    return output


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="按行列数批量等分同目录的 PNG。")
    parser.add_argument("rows", nargs="?", type=positive_int, help="行数，省略时交互输入")
    parser.add_argument("columns", nargs="?", type=positive_int, help="列数，省略时交互输入")
    parser.add_argument("--directory", type=Path, default=Path(__file__).resolve().parent,
                        help="输入图片目录，默认是脚本所在目录")
    args = parser.parse_args(argv)
    folder = args.directory.resolve()
    if not folder.is_dir():
        print(f"目录不存在：{folder}", file=sys.stderr)
        return 1
    rows = args.rows if args.rows is not None else ask_count("行数")
    columns = args.columns if args.columns is not None else ask_count("列数")
    sources = sorted((p for p in folder.iterdir() if p.is_file() and p.suffix.lower() == ".png"),
                     key=lambda p: p.name.casefold())
    if not sources:
        print(f"同目录下没有 PNG：{folder}")
        return 0
    output_root = folder / "output"
    succeeded = 0
    print(f"共 {len(sources)} 张 PNG，每张裁成 {rows} 行 × {columns} 列 = {rows * columns} 张。")
    for source in sources:
        try:
            output = split_png(source, rows, columns, output_root)
        except (OSError, ValueError) as error:
            print(f"[失败] {source.name}：{error}", file=sys.stderr)
            continue
        succeeded += 1
        print(f"[完成] {source.name} → {output}")
    print(f"完成：成功 {succeeded} 张，失败 {len(sources) - succeeded} 张，生成 {succeeded * rows * columns} 张切片。")
    return 0 if succeeded == len(sources) else 1


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (KeyboardInterrupt, EOFError):
        print("\n已取消。")
        raise SystemExit(130)
