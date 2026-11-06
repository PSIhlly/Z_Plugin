from pathlib import Path
import shutil
import sys
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parent.parent
TARGET = PROJECT / 'ExtraAssets' / '使用中' / '贴图' / 'test'
GENERATED = Path(r'C:\Users\LH1\.codex\generated_images\01a01a15-c56f-78e0-ba3f-90961e9be347')
FILES = {
    22: 'exec-c6723d7d-b568-401a-b6f0-eb8b94d21bf2.png',
    23: 'exec-8fcfea73-40f5-4b8c-b13d-03811c061752.png',
    24: 'exec-75a27eae-e9bb-4dca-bd98-1c027bc873f0.png',
    25: 'exec-59f3e6f0-1cbc-4133-814d-239f5d71264d.png',
}


def validate(original, result):
    a, b = np.asarray(original), np.asarray(result)
    mask = np.all(a[:, :, :3] == (255, 0, 255), axis=2)
    assert original.size == result.size == (768, 768)
    assert mask.sum() == 256 * 256
    assert np.array_equal(a[~mask], b[~mask]), 'Pixels outside mask changed'
    assert np.array_equal(a[:, :, 3], b[:, :, 3]), 'Alpha changed'
    assert not np.any(np.all(b[:, :, :3] == (255, 0, 255), axis=2)), 'Magenta remains'


def prepare():
    (ROOT / 'originals').mkdir(parents=True, exist_ok=True)
    (ROOT / 'prepared').mkdir(exist_ok=True)
    for number, filename in FILES.items():
        name = f'{number}.png'
        backup = ROOT / 'originals' / name
        if not backup.exists():
            shutil.copy2(TARGET / name, backup)
        original = Image.open(backup).convert('RGBA')
        candidate = Image.open(GENERATED / filename).convert('RGBA')
        candidate = candidate.resize(original.size, Image.Resampling.NEAREST)
        a = np.asarray(original)
        mask = np.all(a[:, :, :3] == (255, 0, 255), axis=2)
        # Generated transparent-edge artifacts must not enter the opaque masked area.
        pixels = np.array(candidate)
        bad = mask & ((pixels[:, :, 3] < 250) | (np.ptp(pixels[:, :, :3].astype(int), axis=2) > 70))
        valid_positions = np.argwhere(mask & ~bad)
        for y, x in np.argwhere(bad):
            nearest = valid_positions[np.argmin(np.sum((valid_positions - (y, x)) ** 2, axis=1))]
            pixels[y, x] = pixels[nearest[0], nearest[1]]
        candidate = Image.fromarray(pixels)
        result = original.copy()
        result.paste(candidate, (0, 0), Image.fromarray(mask.astype('uint8') * 255))
        result.putalpha(original.getchannel('A'))
        validate(original, result)
        result.save(ROOT / 'prepared' / name)
        print(f'{name}: prepared; outside-mask pixels and alpha identical')


def apply():
    for number in FILES:
        name = f'{number}.png'
        backup = ROOT / 'originals' / name
        original = Image.open(backup).convert('RGBA')
        assert (TARGET / name).read_bytes() == backup.read_bytes(), f'{name} changed after backup'
        validate(original, Image.open(ROOT / 'prepared' / name).convert('RGBA'))
    for number in FILES:
        name = f'{number}.png'
        shutil.copy2(ROOT / 'prepared' / name, TARGET / name)
        validate(Image.open(ROOT / 'originals' / name).convert('RGBA'), Image.open(TARGET / name).convert('RGBA'))
        print(f'{name}: replaced and verified')


if __name__ == '__main__':
    {'prepare': prepare, 'apply': apply}[sys.argv[1]]()
