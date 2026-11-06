from pathlib import Path
import sys
import shutil
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parent.parent
TARGET = PROJECT / 'ExtraAssets' / '使用中' / '贴图' / 'test'
GEN = Path(r'C:\Users\LH1\.codex\generated_images\01a01a15-c56f-78e0-ba3f-90961e9be347')
FILES = {
    22: 'exec-28f9139b-0b6e-413d-b4f0-aa382050cc2a.png',
    23: 'exec-85d7cd5c-641f-441d-9653-4468c77182bd.png',
    24: 'exec-7e7536af-60c9-4e36-9fc7-867919e873bf.png',
    25: 'exec-c0c7bc26-b1fa-4e3f-b68b-becda1251d8f.png',
}


def harmonize(patch, original):
    # Dirichlet correction inside the patch only: retain generated texture
    # gradients, while exactly matching the fixed neighboring pixel colors.
    c = patch.astype(float)
    target = c.copy()
    weight = np.zeros((256, 256, 1))
    total = np.zeros_like(c)
    edges = [
        ((0, slice(None)), original[255,256:512]),
        ((-1, slice(None)), original[512,256:512]),
        ((slice(None),0), original[256:512,255]),
        ((slice(None),-1), original[256:512,512]),
    ]
    for index, neighboring in edges:
        valid = (neighboring[:,3:4] == 255)
        total[index] += neighboring[:,:3] * valid
        weight[index] += valid
    constrained = weight[:,:,0] > 0
    target[constrained] = (total / np.maximum(weight,1))[constrained]
    correction = target - c
    n = 254
    rhs = np.zeros((n,n,3))
    rhs[0] += correction[0,1:-1]
    rhs[-1] += correction[-1,1:-1]
    rhs[:,0] += correction[1:-1,0]
    rhs[:,-1] += correction[1:-1,-1]
    indices = np.arange(1,n+1)
    sine = np.sin(np.pi*np.outer(indices,indices)/(n+1))*np.sqrt(2/(n+1))
    eigen = 2-2*np.cos(np.pi*indices/(n+1))
    denominator = eigen[:,None] + eigen[None,:]
    for channel in range(3):
        correction[1:-1,1:-1,channel] = sine @ ((sine.T @ rhs[:,:,channel] @ sine)/denominator) @ sine.T
    return np.clip(np.rint(c+correction),0,255).astype(np.uint8)


def validate(original, result):
    a,b = np.asarray(original), np.asarray(result)
    mask = np.all(a[:,:,:3] == (255,0,255),axis=2)
    assert original.size == result.size == (768,768)
    assert mask.sum() == 65536
    assert np.array_equal(a[~mask],b[~mask]), 'Outside mask changed'
    assert np.array_equal(a[:,:,3],b[:,:,3]), 'Alpha changed'
    assert not np.any(np.all(b[:,:,:3] == (255,0,255),axis=2))


def prepare():
    (ROOT/'originals').mkdir(parents=True, exist_ok=True)
    (ROOT/'prepared').mkdir(exist_ok=True)
    for number, filename in FILES.items():
        name = f'{number}.png'
        backup = ROOT/'originals'/name
        if not backup.exists():
            shutil.copy2(TARGET/name,backup)
        original = Image.open(backup).convert('RGBA')
        a = np.asarray(original)
        candidate = Image.open(GEN/filename).convert('RGBA').resize(original.size,Image.Resampling.NEAREST)
        c = np.array(candidate)[256:512,256:512]
        invalid = (c[:,:,3] < 250) | (np.ptp(c[:,:,:3].astype(int),axis=2) > 70)
        good = np.argwhere(~invalid)
        for y,x in np.argwhere(invalid):
            nearest = good[np.argmin(np.sum((good-(y,x))**2,axis=1))]
            c[y,x] = c[nearest[0],nearest[1]]
        patch = harmonize(c[:,:,:3],a)
        result = a.copy()
        result[256:512,256:512,:3] = patch
        output = Image.fromarray(result)
        validate(original,output)
        output.save(ROOT/'prepared'/name)
        print(f'{name}: prepared, fixed pixels identical')


def apply():
    for n in FILES:
        name = f'{n}.png'
        assert (TARGET/name).read_bytes() == (ROOT/'originals'/name).read_bytes(), f'{name} changed during editing'
        validate(Image.open(ROOT/'originals'/name).convert('RGBA'), Image.open(ROOT/'prepared'/name).convert('RGBA'))
    for n in FILES:
        name = f'{n}.png'
        shutil.copy2(ROOT/'prepared'/name,TARGET/name)
        validate(Image.open(ROOT/'originals'/name).convert('RGBA'), Image.open(TARGET/name).convert('RGBA'))
        print(f'{name}: replaced, outside-mask changed pixels: 0')


if __name__ == '__main__':
    {'prepare':prepare,'apply':apply}[sys.argv[1]]()
