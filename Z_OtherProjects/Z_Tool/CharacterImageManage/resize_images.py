"""Resize entire canvases only. Never crop, align, translate, or trim alpha."""
import argparse
from image_files import image_files
import os
from pathlib import Path
import shutil
import sys
import tempfile
from datetime import datetime

try:
    from PIL import Image
except ImportError:
    sys.exit('需要 Pillow，请运行：python -m pip install Pillow')

EXTENSIONS = {'.png', '.jpg', '.jpeg', '.bmp', '.webp', '.tif', '.tiff', '.gif', '.ico'}


def positive(value):
    number = int(value)
    if number <= 0 or number > 32768:
        raise ValueError('尺寸必须为 1 到 32768 的整数')
    return number


def resize_directory(directory, width, height):
    directory = Path(directory).resolve(strict=True)
    files = image_files(directory, EXTENSIONS)
    backup = directory / '.resize-backups' / datetime.now().strftime('%Y%m%d-%H%M%S-%f')
    changed = skipped = failed = 0
    print(f'目录（含所有子文件夹）：{directory}\n目标：宽 {width} × 高 {height}')
    print('仅缩放完整画布（含透明边距）；最近邻采样；不裁切、不平移。')
    for path in files:
        temp = None
        try:
            with Image.open(path) as source:
                if getattr(source, 'n_frames', 1) != 1:
                    raise ValueError('多帧图片不覆盖，请单独处理')
                if source.size == (width, height):
                    skipped += 1
                    print(f'已是目标尺寸：{path.relative_to(directory)}')
                    continue
                # Resolve indexed transparency before resizing; no EXIF orientation transform.
                image = source.convert('RGBA') if source.mode in ('P', 'LA') or 'transparency' in source.info else source.copy()
                result = image.resize((width, height), Image.Resampling.NEAREST)
                fmt = source.format
                options = {}
                if fmt == 'JPEG':
                    result = result.convert('RGB')
                    options = {'quality': 95, 'subsampling': 0}
                elif fmt == 'WEBP':
                    options = {'lossless': True}
                elif fmt == 'ICO':
                    options = {'sizes': [(width, height)]}
            fd, name = tempfile.mkstemp(prefix='.resize-', suffix=path.suffix, dir=path.parent)
            os.close(fd)
            temp = Path(name)
            result.save(temp, format=fmt, **options)
            with Image.open(temp) as check:
                check.load()
                if check.size != (width, height):
                    raise ValueError('输出尺寸验证失败，此格式可能不支持目标尺寸')
            backup.mkdir(parents=True, exist_ok=True)
            backup_path = backup / path.relative_to(directory)
            backup_path.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(path, backup_path)
            os.replace(temp, path)
            print(f'已缩放：{path.relative_to(directory)}')
            changed += 1
        except Exception as exc:
            failed += 1
            print(f'失败，保留原文件：{path.relative_to(directory)}：{exc}')
        finally:
            if temp is not None and temp.exists():
                temp.unlink()
    print(f'完成：缩放 {changed}，尺寸未变 {skipped}，失败 {failed}。')
    if changed:
        print(f'原图备份：{backup}')
    return 1 if failed else 0


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--directory', type=Path, default=Path(__file__).resolve().parent)
    parser.add_argument('--width', type=int)
    parser.add_argument('--height', type=int)
    args = parser.parse_args()
    try:
        width = positive(args.width if args.width is not None else input('请输入宽度（像素）：').strip())
        height = positive(args.height if args.height is not None else input('请输入高度（像素）：').strip())
        return resize_directory(args.directory, width, height)
    except (ValueError, OSError, EOFError) as exc:
        print(f'无法处理：{exc}')
        return 1


if __name__ == '__main__':
    sys.exit(main())
