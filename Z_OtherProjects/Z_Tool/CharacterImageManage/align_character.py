#!/usr/bin/env python3
"""Align PNG images in the launcher directory to reference content rows."""
import argparse
from image_files import image_files
import os
from pathlib import Path
import sys
import tempfile

BOUNDARY_TOLERANCE = 1
RESIZE_RETRY_RADIUS = 3

try:
    from PIL import Image
except ImportError:
    sys.exit('需要 Pillow：python -m pip install Pillow')

def content_box(image):
    alpha = image.getchannel('A').point(lambda a: 255 if a >= 128 else 0)
    box = alpha.getbbox()
    if box is None:
        raise ValueError('没有透明度大于或等于 128 的内容')
    return box


def bounds(image):
    """Rows containing at least one pixel with alpha >= 128, inclusive."""
    box = content_box(image)
    return box[1], box[3] - 1


def read_png(path):
    with Image.open(path) as im:
        if getattr(im, 'n_frames', 1) != 1:
            raise ValueError('不支持动画 PNG')
        return im.convert('RGBA')


def align(image, target):
    top, bottom = bounds(image)
    target_top, target_bottom = target
    if (top, bottom) == target:
        return None
    w, h = image.size
    scale = (target_bottom - target_top + 1) / (bottom - top + 1)
    initial_height = max(1, round(h * scale))
    heights = [initial_height]
    for delta in range(1, RESIZE_RETRY_RADIUS + 1):
        heights.extend((initial_height - delta, initial_height + delta))

    best = None
    best_error = BOUNDARY_TOLERANCE + 1
    first_failure = None
    for new_h in heights:
        if new_h < 1:
            continue
        width_scale = scale if new_h == initial_height else new_h / h
        new_w = max(1, round(w * width_scale))
        resized = image.resize((new_w, new_h), Image.Resampling.LANCZOS)
        try:
            left, resized_top, right, resized_bottom = content_box(resized)
        except ValueError as exc:
            first_failure = first_failure or str(exc)
            continue
        x = round((w - new_w) / 2)
        y = target_top - resized_top
        if left + x < 0 or right + x > w or resized_top + y < 0 or resized_bottom + y > h:
            first_failure = first_failure or '缩放和平移会裁切透明度大于或等于 128 的像素'
            continue
        actual = (target_top, resized_bottom + y - 1)
        error = abs(actual[1] - target_bottom)
        if error > BOUNDARY_TOLERANCE:
            first_failure = first_failure or (
                f'缩放后边界 {actual} 与目标 {target} 相差超过 {BOUNDARY_TOLERANCE} 像素')
            continue
        if error < best_error:
            best = resized, x, y
            best_error = error
        if new_h == initial_height or error == 0:
            break

    if best is None:
        raise ValueError(f'试算相邻尺寸后仍无法对齐：{first_failure}')
    resized, x, y = best
    result = Image.new('RGBA', (w, h))
    result.paste(resized, (x, y))
    return result


def replace_png(image, path):
    fd, name = tempfile.mkstemp(prefix='.align-', suffix='.png', dir=path.parent)
    os.close(fd)
    try:
        image.save(name, format='PNG')
        os.replace(name, path)
    finally:
        if os.path.exists(name):
            os.unlink(name)


def run(reference, directory=None):
    if not reference.is_file() or reference.suffix.lower() != '.png':
        raise ValueError(f'标准图不是有效的 PNG 文件：{reference}')
    reference = reference.resolve()
    image = read_png(reference)
    size, target = image.size, bounds(image)
    reference_image = image
    targets = {size: target}
    directory = Path(directory).resolve() if directory is not None else Path(__file__).resolve().parent
    files = image_files(directory, {'.png'})
    print(f'标准图：{reference.name}：{size[0]}×{size[1]}，'
          f'上方空行 {target[0]}，下方空行 {size[1] - target[1] - 1}')
    print(f'处理目录（含所有子文件夹）：{directory}')
    changed = unchanged = failed = 0
    for path in sorted(files, key=lambda p: p.name):
        if path.resolve() == reference:
            continue
        try:
            image = read_png(path)
            if image.size not in targets:
                scaled_reference = reference_image.resize(image.size, Image.Resampling.LANCZOS)
                targets[image.size] = bounds(scaled_reference)
                print(f'参考标准已按目标画布 {image.width}×{image.height} 重新计算')
            result = align(image, targets[image.size])
            if result is None:
                unchanged += 1
                print(f'已对齐：{path.relative_to(directory)}')
            else:
                replace_png(result, path)
                changed += 1
                print(f'已覆盖：{path.relative_to(directory)}，内容行 {bounds(result)}')
        except (OSError, ValueError) as exc:
            failed += 1
            print(f'跳过：{path.relative_to(directory)}：{exc}')
    print(f'完成：覆盖 {changed} 张，已对齐 {unchanged} 张，失败 {failed} 张。')
    return 1 if failed else 0


def main():
    parser = argparse.ArgumentParser(description='按标准图对齐工具目录的 PNG 并覆盖原图')
    parser.add_argument('reference', nargs='?', help='标准 PNG 图片；省略时交互输入')
    parser.add_argument('--directory', type=Path, default=Path(__file__).resolve().parent,
                        help='待处理 PNG 目录，默认工具所在目录；bat 自动传入自身目录')
    args = parser.parse_args()
    reference = args.reference or input('请输入标准 PNG 图片路径（也可将图片拖到本窗口）：').strip().strip('"')
    if not reference:
        parser.error('未指定标准图片')
    try:
        return run(Path(reference).expanduser(), args.directory)
    except (OSError, ValueError) as exc:
        print(f'无法处理：{exc}', file=sys.stderr)
        return 1


if __name__ == '__main__':
    sys.exit(main())
