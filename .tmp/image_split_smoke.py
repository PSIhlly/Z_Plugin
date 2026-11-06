from pathlib import Path
from tempfile import TemporaryDirectory
import hashlib
import os
import shutil
import subprocess
import sys
from PIL import Image

PROJECT=Path(__file__).resolve().parent.parent
TOOL=PROJECT/'Z_OtherProjects/Z_Tool/ImageSplit'
os.environ['PYTHONUTF8']='1'
with TemporaryDirectory(prefix="Image split's ! & 测试_",dir=PROJECT/'.tmp') as temp:
    folder=Path(temp)
    for name in ('split_png.py','切图.bat'):
        shutil.copy2(TOOL/name,folder/name)
    source=Image.new('RGBA',(7,5))
    source.putdata([(x*30,y*40,(x+y)*17,(x*31+y*43)%256) for y in range(5) for x in range(7)])
    source.save(folder/"原图 ! & ' .PNG")
    palette=Image.new('P',(12,8));palette.putpalette([i for n in range(256) for i in (n,255-n,n//2)])
    palette.putdata([i%4 for i in range(96)]);palette.info['transparency']=0
    palette.save(folder/'透明索引.png')
    (folder/'nested').mkdir();source.save(folder/'nested/ignored.png')
    originals={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in folder.glob('*') if p.suffix.lower()=='.png'}
    command_line=f'"{os.environ.get("COMSPEC","cmd.exe")}" /d /c call "{folder / "切图.bat"}"'
    bat=subprocess.run(command_line,
                       input='0\nx\n2\n3\n\n',encoding='utf-8',errors='replace',capture_output=True,cwd=PROJECT,timeout=60)
    if bat.returncode:
        raise AssertionError((bat.returncode,bat.stdout,bat.stderr))
    outputs=list((folder/'output').iterdir())
    assert len(outputs)==2,(outputs,bat.stdout,bat.stderr)
    for filename in originals:
        with Image.open(folder/filename) as im:
            expected=im.convert('RGBA')
        destination=folder/'output'/f'{Path(filename).stem}_2x3'
        tiles=list(destination.glob('*.png'));assert len(tiles)==6
        restored=Image.new('RGBA',expected.size)
        widths=[];heights=[]
        for row in range(2):
            for col in range(3):
                with Image.open(destination/f'r{row+1:02}_c{col+1:02}.png') as tile:
                    restored.paste(tile.convert('RGBA'),(col*expected.width//3,row*expected.height//2))
                    widths.append(tile.width);heights.append(tile.height)
        assert restored.tobytes()==expected.tobytes()
        assert max(widths)-min(widths)<=1 and max(heights)-min(heights)<=1
    command=[sys.executable,str(folder/'split_png.py'),'2','3']
    second=subprocess.run(command,capture_output=True,text=True,encoding='utf-8',timeout=30)
    assert second.returncode==0,(second.stdout,second.stderr)
    assert len(list((folder/'output').iterdir()))==4
    assert all(hashlib.sha256((folder/name).read_bytes()).hexdigest()==digest for name,digest in originals.items())
    (folder/'bad.png').write_bytes(b'not an image')
    failed=subprocess.run(command,capture_output=True,text=True,encoding='utf-8',timeout=30)
    assert failed.returncode==1 and 'bad.png' in failed.stderr
    assert len(list((folder/'output').iterdir()))==6
    invalid=subprocess.run([sys.executable,str(folder/'split_png.py'),'0','3'],capture_output=True,timeout=30)
    assert invalid.returncode==2
    oversized=subprocess.run([sys.executable,str(folder/'split_png.py'),'99','99'],capture_output=True,timeout=30)
    assert oversized.returncode==1
    assert len(list((folder/'output').iterdir()))==6
    print('PASS: BAT interactive entry, invalid input retry, batch PNG/PNG casing, Chinese/spaces/apostrophe/!/&, nondivisible complete pixel reconstruction, RGBA and palette transparency, no recursion, source protection, versioned outputs, corrupt file continuation, invalid and oversized grids.')
