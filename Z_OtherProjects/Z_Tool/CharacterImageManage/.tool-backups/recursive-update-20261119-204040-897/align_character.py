#!/usr/bin/env python3
"""Align PNG images in the launcher directory to reference content rows."""
import argparse
import os
from pathlib import Path
import sys
import tempfile

try:
    from PIL import Image
except ImportError:
    sys.exit('需要 Pillow：python -m pip install Pillow')

def bounds(image):
    """Rows containing at least one pixel with alpha >= 128, inclusive."""
    alpha = image.getchannel('A').point(lambda a: 255 if a >= 128 else 0)
    box = alpha.getbbox()
    if box is None:
        raise ValueError('没有透明度大于或等于 128 的内容')
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
    new_w, new_h = max(1, round(w * scale)), max(1, round(h * scale))
    resized = image.resize((new_w, new_h), Image.Resampling.LANCZOS)
    resized_top, _ = bounds(resized)
    x = round((w - new_w) / 2)
    y = target_top - resized_top
    visible = resized.getchannel('A').point(lambda a: 255 if a >= 128 else 0).getbbox()
    if visible and (visible[0] + x < 0 or visible[2] + x > w or
                    visible[1] + y < 0 or visible[3] + y > h):
        raise ValueError('缩放和平移会裁切透明度大于或等于 128 的像素')
    result = Image.new('RGBA', (w, h))
    result.paste(resized, (x, y))
    actual = bounds(result)
    if max(abs(actual[0] - target_top), abs(actual[1] - target_bottom)) > 1:
        raise ValueError(f'缩放后边界 {actual} 与目标 {target} 相差超过 1 像素')
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
    files = [p for p in directory.iterdir() if p.is_file() and p.suffix.lower() == '.png']
    print(f'标准图：{reference.name}：{size[0]}×{size[1]}，'
          f'上方空行 {target[0]}，下方空行 {size[1] - target[1] - 1}')
    print(f'处理目录：{directory}')
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
                print(f'已对齐：{path.name}')
            else:
                replace_png(result, path)
                changed += 1
                print(f'已覆盖：{path.name}，内容行 {bounds(result)}')
        except (OSError, ValueError) as exc:
            failed += 1
            print(f'跳过：{path.name}：{exc}')
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
