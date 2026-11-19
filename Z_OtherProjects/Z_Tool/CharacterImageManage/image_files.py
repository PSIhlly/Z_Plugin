"""Recursive image inventory; keep backup trees and linked directories untouched."""
import os
from pathlib import Path

EXCLUDED = {'.resize-backups', '.tool-backups', '__pycache__'}


def image_files(directory, extensions):
    directory = Path(directory)
    files = []
    def linked(path):
        return bool(path.lstat().st_file_attributes & 0x400) if os.name == 'nt' else path.is_symlink()
    for current, folders, names in os.walk(directory, followlinks=False):
        parent = Path(current)
        folders[:] = sorted(name for name in folders
                            if name.casefold() not in EXCLUDED and not linked(parent / name))
        for name in sorted(names):
            path = parent / name
            if (path.suffix.lower() in extensions and not linked(path)
                    and not name.startswith(('.resize-', '.align-'))):
                files.append(path)
    return sorted(files, key=lambda p: str(p.relative_to(directory)).casefold())
