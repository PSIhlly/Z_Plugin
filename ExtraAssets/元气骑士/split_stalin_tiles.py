from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
from collections import deque
import json, hashlib
root=Path(r'D:/Works/Game/Z_Plugin/ExtraAssets/元气骑士')
src=root/'斯大林制地砖.png'
out=root/'斯大林制地砖_拆分'
out.mkdir(exist_ok=True)
im=Image.open(src).convert('RGBA')
items=[]
masked={}
def add(cat,name,box):
    patch=im.crop(box)
    if name=='altar':
        for py in range(patch.height):
            for px in range(patch.width):
                if box[0]+px<96 and box[1]+py<135:patch.putpixel((px,py),(0,0,0,0))
    if name=='audio_crate':
        for py in range(patch.height):
            for px in range(patch.width):
                if box[0]+px>=256 and (box[1]+py>=138 or box[1]+py<121):patch.putpixel((px,py),(0,0,0,0))
    bb=patch.getbbox()
    if not bb:return
    actual=(box[0]+bb[0],box[1]+bb[1],box[0]+bb[2],box[1]+bb[3])
    masked[name]=patch.crop(bb)
    items.append(dict(category=cat,name=name,region=list(box),bounds=list(actual),size=[actual[2]-actual[0],actual[3]-actual[1]]))
# Tiles are authored on a 16 px horizontal pitch, while walls use 24 px height.
for row in range(7):
    for col in range(6):add('01_floor',f'floor_{row+1:02}_{col+1:02}',(col*16,row*16,col*16+16,row*16+16))
for col in range(4):add('01_floor',f'floor_08_{col+1:02}',(col*16,112,col*16+16,128))
add('01_floor','floor_lava',(79,112,96,129))
for row in range(2):
    for col in range(10):add('02_walls',f'wall_{row+1:02}_{col+1:02}',(96+col*16,row*24,112+col*16,row*24+24))
for col in range(6):add('02_walls',f'block_{col+1:02}',(160+col*16,48,176+col*16,76))
for col in range(4):add('03_props',f'crate_top_{col+1:02}',(256+col*16,0,272+col*16+(1 if col==3 else 0),24))
for col in range(5):add('03_props',f'barrel_a_{col+1:02}',(256+col*16,24,272+col*16,48))
for col in range(5):add('03_props',f'container_b_{col+1:02}',(256+col*16,48,272+col*16,72))
for col in range(5):add('03_props',f'container_c_{col+1:02}',(256+col*16,72,272+col*16,96))
for col in range(5):add('06_swatches',f'color_{col+1:02}',(256+col*16,96,272+col*16,112))
for row in range(2):
    for col in range(3):add('06_swatches',f'gray_yellow_{row}_{col}',(288+col*16,112+row*16,304+col*16,128+row*16+(1 if row==1 else 0)))
for name,box in [
('tree_green',(96,48,112,76)),('stump',(112,48,129,76)),('training_dummy',(129,48,160,76)),
('tree_snow',(96,76,128,114)),('ice_rock',(128,76,145,98)),
('rail_curve',(142,96,160,146)),('minecart_a',(159,74,192,114)),('minecart_b',(192,80,224,114)),('ice_lantern_cluster',(224,74,252,114)),
('brazier_small',(160,122,176,145)),('brazier_wide',(176,120,194,145)),('blue_flame',(194,118,208,145)),('torch',(208,115,225,145)),
('audio_crate',(225,112,263,146)),('crate_wood',(256,138,273,154)),('gate_spike',(273,120,289,154)),
('machine_blue',(225,146,257,179)),('machine_lava',(132,146,161,179)),('machine_small',(161,146,179,179)),
('altar',(88,114,132,176)),('statue_dark',(53,169,90,224)),('dead_tree',(90,176,145,224)),('statues_stone',(145,190,189,224)),
('barricade',(189,192,221,212)),('barrel_blue',(222,181,240,212)),('mushrooms',(240,181,261,212)),
('pumpkin_candles',(179,145,225,192)),('log',(267,171,284,192)),
('cactus_small',(289,148,315,168)),('cactus_large',(283,166,315,192)),('pine_tree',(315,159,336,192)),
('grass_patch_a',(261,192,294,208)),('grass_patch_b',(261,208,294,224)),
('grass_sprig',(294,203,307,214)),('fence',(307,201,336,214)),
('carpet',(0,160,52,192)),('banner',(0,192,16,224)),('bookshelf',(16,192,53,224)),
]:add('03_props',name,box)
for name,box in [('arrow_blue',(0,128,16,144)),('arrow_green',(16,128,32,144)),('arrow_yellow',(0,144,16,160)),('arrow_red',(16,144,32,160)),('floor_holes',(48,128,64,144)),('spikes_up',(64,128,80,144)),('trap_eyes',(48,144,64,160)),('spikes_single',(64,144,80,160))]:add('04_traps',name,box)
add('06_swatches','swatch_dark_visible',(256,112,288,121))
# Retain any remaining source pixels as separately identified fragments, never silently discard them.
covered=Image.new('1',im.size)
d=ImageDraw.Draw(covered)
for it in items:
    x,y,r,b=it['region'];d.rectangle((x,y,r-1,b-1),fill=1)
alpha=im.getchannel('A');seen=set(); residual=[]
for y in range(im.height):
 for x in range(im.width):
  if (x,y) in seen or covered.getpixel((x,y)) or not alpha.getpixel((x,y)):continue
  q=[(x,y)];seen.add((x,y));pts=[]
  while q:
   xx,yy=q.pop();pts.append((xx,yy))
   for nx,ny in ((xx-1,yy),(xx+1,yy),(xx,yy-1),(xx,yy+1)):
    if 0<=nx<im.width and 0<=ny<im.height and (nx,ny) not in seen and not covered.getpixel((nx,ny)) and alpha.getpixel((nx,ny)):
     seen.add((nx,ny));q.append((nx,ny))
  box=(min(p[0] for p in pts),min(p[1] for p in pts),max(p[0] for p in pts)+1,max(p[1] for p in pts)+1)
  residual.append((len(pts),box))
print('RESIDUAL',residual)
for j,(n,box) in enumerate(residual):add('05_remaining',f'fragment_{j+1:02}',box)
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',13)
cols=8;cw=164;ch=160
sheet=Image.new('RGB',(cols*cw,((len(items)+cols-1)//cols)*ch),(35,38,44));draw=ImageDraw.Draw(sheet)
for i,it in enumerate(items):
    rel=Path(it['category'])/(f'{i+1:03}_'+it['name']+'.png');it['file']=rel.as_posix()
    dest=out/rel;dest.parent.mkdir(exist_ok=True)
    crop=masked[it['name']];crop.save(dest)
    assert Image.open(dest).tobytes()==crop.tobytes()
    xx=(i%cols)*cw;yy=(i//cols)*ch
    for a in range(0,120,8):
     for b in range(0,152,8):draw.rectangle((xx+b+4,yy+a+4,xx+b+11,yy+a+11),fill=(60,64,72) if (a//8+b//8)%2 else (48,52,59))
    scale=max(1,min(4,148//crop.width,112//crop.height));large=crop.resize((crop.width*scale,crop.height*scale),Image.Resampling.NEAREST)
    sheet.paste(large,(xx+8,yy+8),large)
    draw.text((xx+6,yy+126),f'{i+1:03} {it["name"]}',font=font,fill='white')
    draw.text((xx+6,yy+144),f'{crop.width} x {crop.height}',font=font,fill=(190,195,200))
sheet.save(out/'preview.png')
(out/'manifest.json').write_text(json.dumps(dict(source=src.name,source_size=list(im.size),sha256=hashlib.sha256(src.read_bytes()).hexdigest(),coordinate_system='top-left, x/y, right/bottom exclusive',items=items),ensure_ascii=False,indent=2),encoding='utf-8')
print('COUNT',len(items),'OUT',out)


# Verify retained pixels and reconstruct the source without compositing changes.
reconstructed=Image.new('RGBA',im.size)
for it in items:
    patch=Image.open(out/it['file']).convert('RGBA'); x,y=it['bounds'][:2]
    for py in range(patch.height):
        for px in range(patch.width):
            pixel=patch.getpixel((px,py))
            if pixel[3]:
                assert pixel==im.getpixel((x+px,y+py))
                reconstructed.putpixel((x+px,y+py),pixel)
assert reconstructed.tobytes()==im.tobytes(), 'Pixel reconstruction failed'
print('Exact pixel reconstruction: PASS')
Image.open(out/'preview.png').crop((0,2080,1312,3040)).save(out/'preview_props.png')
import csv
with (out/'index.csv').open('w',encoding='utf-8-sig',newline='') as f:
    writer=csv.writer(f);writer.writerow(['ID','Category','Name','Width','Height','SourceX','SourceY','File'])
    for n,it in enumerate(items,1):writer.writerow([n,it['category'],it['name'],*it['size'],*it['bounds'][:2],it['file']])
annotated=im.resize((2016,1344),Image.Resampling.NEAREST);ad=ImageDraw.Draw(annotated)
for n,it in enumerate(items,1):
    x,y,r,b=it['bounds'];ad.rectangle((x*6,y*6,r*6-1,b*6-1),outline=(255,70,170,255),width=1)
    ad.text((x*6+1,y*6+1),str(n),font=font,fill='white',stroke_width=1,stroke_fill='black')
annotated.save(out/'source_regions.png')
