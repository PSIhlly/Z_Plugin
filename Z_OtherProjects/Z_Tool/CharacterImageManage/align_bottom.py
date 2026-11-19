#!/usr/bin/env python3
"""Translate PNG images vertically to match reference bottom padding."""
import argparse
from image_files import image_files
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
    """Translate vertically to match bottom padding, counting alpha >= 128."""
    top, bottom = bounds(image)
    w, h = image.size
    if target < 0 or target >= h:
        raise ValueError('参考图的下方透明行数超出当前画布范围')
    shift = h - 1 - target - bottom
    if shift == 0:
        return None
    if top + shift < 0 or bottom + shift >= h:
        raise ValueError('上下平移会裁切透明度大于或等于 128 的像素')
    result = Image.new('RGBA', (w, h))
    result.paste(image, (0, shift))
    if h - bounds(result)[1] - 1 != target:
        raise ValueError('平移后的下方透明行数与目标不一致')
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
    size = image.size
    target = size[1] - bounds(image)[1] - 1
    reference_image = image
    targets = {size: target}
    directory = Path(directory).resolve() if directory is not None else Path(__file__).resolve().parent
    files = image_files(directory, {'.png'})
    print(f'标准图：{reference.name}：{size[0]}×{size[1]}，'
          f'下方空行 {target}（透明度 >=128 视为内容）')
    print(f'处理目录（含所有子文件夹）：{directory}')
    changed = unchanged = failed = 0
    for path in sorted(files, key=lambda p: p.name):
        if path.resolve() == reference:
            continue
        try:
            image = read_png(path)
            if image.size not in targets:
                scaled_reference = reference_image.resize(image.size, Image.Resampling.LANCZOS)
                targets[image.size] = image.height - bounds(scaled_reference)[1] - 1
                print(f'参考标准已按目标画布 {image.width}×{image.height} 重新计算')
            result = align(image, targets[image.size])
            if result is None:
                unchanged += 1
                print(f'已对齐：{path.relative_to(directory)}')
            else:
                replace_png(result, path)
                changed += 1
                print(f'已覆盖：{path.relative_to(directory)}，下方空行 {result.height - bounds(result)[1] - 1}')
        except (OSError, ValueError) as exc:
            failed += 1
            print(f'跳过：{path.relative_to(directory)}：{exc}')
    print(f'完成：覆盖 {changed} 张，已对齐 {unchanged} 张，失败 {failed} 张。')
    return 1 if failed else 0


def main():
    parser = argparse.ArgumentParser(description='仅上下平移工具目录的 PNG，使下方透明行数与参考图一致；不缩放')
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
