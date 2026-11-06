import importlib.util
import json
from pathlib import Path
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent.parent
FOLDER = ROOT / 'ExtraAssets' / '使用中' / '贴图' / 'test'
spec = importlib.util.spec_from_file_location('audited_texture_tool', FOLDER/'texture_tool.py')
t = importlib.util.module_from_spec(spec)
spec.loader.exec_module(t)
source = t.load_source()
q,s = source.width//4,source.height//6
def cell(c,r):
    return source.crop(((c-1)*q,(r-1)*s,c*q,r*s))
def eq(a,b):
    return a.size == b.size and a.tobytes() == b.tobytes()
source_cells = {(c,r):cell(c,r) for r in range(1,7) for c in range(1,5)}

generated = {}
original_open = t.open_required
t.save_png = lambda image,name: generated.__setitem__(name,image.copy())
t.open_required = lambda name: generated[name].copy() if name in generated else original_open(name)
t.split_image()
summary=[]
for path in sorted(FOLDER.glob('*.png'),key=lambda p:int(p.stem) if p.stem.isdigit() else 10000):
    im=Image.open(path).convert('RGBA')
    entry={'file':path.name,'size':im.size,'purple_pixels':int(np.all(np.asarray(im)[:,:,:3]==(255,0,255),axis=2).sum())}
    expected=generated.get(path.name)
    if expected is not None:
        entry['generated_size']=expected.size
        entry['same_as_current_script']=eq(im,expected)
        if im.size==expected.size and not entry['same_as_current_script']:
            diff=np.any(np.asarray(im)!=np.asarray(expected),axis=2)
            yy,xx=np.where(diff)
            entry['difference_box']=[int(xx.min()),int(yy.min()),int(xx.max()+1),int(yy.max()+1)]
            entry['different_pixels']=int(diff.sum())
    summary.append(entry)

# Canonical surrounding phases for a repeating 2x2 interior (source columns 2-3, rows 4-5).
# Inner TL/TR/BL/BR: transparent outer corner; other corners use interior A/B/C/D.
maps={
    6:[[None,(1,4),(2,4)],[(2,3),(3,1),(2,5)],[(2,4),(3,4),(2,4)]],
    7:[[(3,4),(4,4),None],[(3,5),(4,1),(3,3)],[(3,4),(2,4),(3,4)]],
    8:[[(2,5),(3,5),(2,5)],[(2,6),(3,2),(2,4)],[None,(1,5),(2,5)]],
    9:[[(3,5),(2,5),(3,5)],[(3,4),(4,2),(3,6)],[(3,5),(4,5),None]],
}
# Independently derive the same 3x3 neighborhoods using TileHelper.cs's
# GetPartId table and the eight-neighbor occupancy convention.
part_rules={(0,0):(19,5,9,17,11),(1,0):(18,6,12,20,10),
            (0,1):(15,7,21,13,23),(1,1):(14,8,24,16,22)}
def part_cell(part):
    if part<=8:
        return (3+(part-5)%2,1+(part-5)//2)
    return (1+(part-9)%4,3+(part-9)//4)
for n,(cx,cy) in zip(range(6,10),[(0,0),(1,0),(0,1),(1,1)]):
    empty=(-1 if cx==0 else 1,-1 if cy==0 else 1)
    derived=[]
    for row in range(3):
        line=[]
        for col in range(3):
            gx,gy=cx+col-1,cy+row-1
            tx,ty=gx//2,gy//2
            px,py=gx%2,gy%2
            if (tx,ty)==empty:
                line.append(None); continue
            dx,dy=(-1 if px==0 else 1),(-1 if py==0 else 1)
            horizontal=(tx+dx,ty)!=empty
            vertical=(tx,ty+dy)!=empty
            diagonal=(tx+dx,ty+dy)!=empty
            all_same,diag_different,neither,vertical_only,horizontal_only=part_rules[(px,py)]
            part=(all_same if diagonal else diag_different) if horizontal and vertical else neither if not horizontal and not vertical else vertical_only if vertical else horizontal_only
            line.append(part_cell(part))
        derived.append(line)
    assert derived==maps[n], (n,derived,maps[n])
positions=[['top-left','top-middle','top-right'],['middle-left','center','middle-right'],['bottom-left','bottom-middle','bottom-right']]
findings=[]
for n,grid in maps.items():
    for number in (n,n+16):
        im=original_open(f'{number}.png')
        for r in range(3):
            for c in range(3):
                if r==1 and c==1:
                    continue
                expected=grid[r][c]
                actual=im.crop((c*q,r*s,(c+1)*q,(r+1)*s))
                if expected is None:
                    continue
                if not eq(actual,source_cells[expected]):
                    matches=[list(k) for k,v in source_cells.items() if eq(actual,v)]
                    findings.append({'file':f'{number}.png','position':positions[r][c],'actual_source_cells':matches,'expected_source_cell':expected})

pairs=[]
for a,b in [(6,22),(7,23),(8,24),(9,25)]:
    x=np.asarray(original_open(f'{a}.png')); y=np.asarray(original_open(f'{b}.png'))
    mask=np.ones(x.shape[:2],dtype=bool); mask[s:2*s,q:2*q]=False
    pairs.append({'pair':[a,b],'outside_center_identical':bool(np.array_equal(x[mask],y[mask]))})
central_checks={}
if (FOLDER/'50.png').exists():
    central_checks['50_equals_source_center']=eq(original_open('50.png'),source.crop((q,3*s,3*q,5*s)))
if (FOLDER/'101.png').exists():
    central_checks['101_center_equals_50']=eq(t.center_tile_3x3(original_open('101.png'),(2*q,2*s)),original_open('50.png'))
print(json.dumps({'files':summary,'corner_findings':findings,'masked_pairs':pairs,'central_checks':central_checks},ensure_ascii=False,indent=2))
