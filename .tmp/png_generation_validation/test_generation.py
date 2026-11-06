import importlib.util
from pathlib import Path
import re
from tempfile import TemporaryDirectory
import unittest
from PIL import Image

PROJECT = Path(__file__).resolve().parent.parent.parent
spec = importlib.util.spec_from_file_location('texture_tool', PROJECT/'ExtraAssets/使用中/贴图/test/texture_tool.py')
t = importlib.util.module_from_spec(spec)
spec.loader.exec_module(t)
Q = 16


class GenerationChecks(unittest.TestCase):
    def setUp(self):
        self.temp = TemporaryDirectory(prefix='texture_generation_check_', dir=Path(__file__).resolve().parent)
        self.addCleanup(self.temp.cleanup)
        old_folder = t.FOLDER
        self.addCleanup(setattr, t, 'FOLDER', old_folder)
        t.FOLDER = Path(self.temp.name)
        self.source = Image.new('RGBA',(4*Q,6*Q))
        for row in range(6):
            for col in range(4):
                self.source.paste((30+col*50,20+row*35,10+row*4+col,255),
                                  (col*Q,row*Q,(col+1)*Q,(row+1)*Q))
        self.source.save(t.FOLDER/'0.png')

    def assertImage(self, actual, expected):
        self.assertEqual(actual.size,expected.size)
        self.assertEqual(actual.tobytes(),expected.tobytes())

    def test_previews_against_actual_runtime_table(self):
        cs = (PROJECT/'Assets/Z_Level3/Z_Map/Core/TileHelper.cs').read_text(encoding='utf-8-sig')
        rules = {}
        for key, name in [((0,0),'topLeft'),((1,0),'topRight'),((0,1),'bottomLeft'),((1,1),'bottomRight')]:
            body = re.search(r'int '+name+r'\s*=\s*GetPartId\((.*?)\);',cs,re.S).group(1)
            rules[key] = list(map(int,re.findall(r'\b\d+\b',body)))
            self.assertEqual(len(rules[key]),5)
        t.split_image()
        for number,(cx,cy) in zip(range(6,10),[(0,0),(1,0),(0,1),(1,1)]):
            empty = (-1 if cx==0 else 1,-1 if cy==0 else 1)
            actual = t.open_required(f'{number}.png')
            marked = t.open_required(f'{number+16}.png')
            for row in range(3):
                for col in range(3):
                    gx,gy = cx+col-1,cy+row-1
                    tx,ty,px,py = gx//2,gy//2,gx%2,gy%2
                    if (tx,ty)==empty:
                        expected = Image.new('RGBA',(Q,Q))
                    else:
                        dx,dy = (-1 if px==0 else 1),(-1 if py==0 else 1)
                        h,v,d = (tx+dx,ty)!=empty,(tx,ty+dy)!=empty,(tx+dx,ty+dy)!=empty
                        all_same,diag_diff,neither,v_only,h_only=rules[(px,py)]
                        part=(all_same if d else diag_diff) if h and v else neither if not h and not v else v_only if v else h_only
                        if part<=8:
                            sc,sr=2+(part-5)%2,(part-5)//2
                        else:
                            sc,sr=(part-9)%4,2+(part-9)//4
                        expected=self.source.crop((sc*Q,sr*Q,(sc+1)*Q,(sr+1)*Q))
                    box=(col*Q,row*Q,(col+1)*Q,(row+1)*Q)
                    self.assertImage(actual.crop(box),expected)
                    if (col,row)==(1,1):
                        expected=Image.new('RGBA',(Q,Q),(255,0,255,255))
                    self.assertImage(marked.crop(box),expected)
        for n in (18,19):
            self.assertEqual(t.open_required(f'{n}.png').size,(3*Q,2*Q))
        for n in (20,21):
            self.assertEqual(t.open_required(f'{n}.png').size,(2*Q,3*Q))

    def test_scaled_strip_selection_and_legacy_expanded_sizes(self):
        red,blue,green=(230,20,30,255),(20,30,230,255),(20,220,30,255)
        normal_h=Image.new('RGBA',(64,16),blue)
        normal_h.paste(red,(0,0,32,16))
        normal_v=Image.new('RGBA',(16,64),blue)
        normal_v.paste(red,(0,0,16,32))
        expanded_h=Image.new('RGBA',(96,64),green)
        expanded_h.paste(red,(0,0,64,32)); expanded_h.paste(blue,(0,32,64,64))
        expanded_v=Image.new('RGBA',(64,96),green)
        expanded_v.paste(red,(0,0,32,64)); expanded_v.paste(blue,(32,0,64,64))
        for factor in (1,2,3):
            scale=lambda im:im.resize((im.width*factor,im.height*factor),Image.Resampling.NEAREST)
            for use_second in (False,True):
                self.assertImage(t.join_horizontal_strip(scale(normal_h),(64,16),(32,16),use_second),Image.new('RGBA',(32,16),red))
                self.assertImage(t.join_vertical_strip(scale(normal_v),(16,64),(16,32),use_second),Image.new('RGBA',(16,32),red))
                color=blue if use_second else red
                self.assertImage(t.join_horizontal_strip(scale(expanded_h),(64,16),(32,16),use_second),Image.new('RGBA',(32,16),color))
                self.assertImage(t.join_vertical_strip(scale(expanded_v),(16,64),(16,32),use_second),Image.new('RGBA',(16,32),color))
        self.assertFalse(t.is_standard_strip(Image.new('RGBA',(768,513)),(1024,256),(3072,2048)))
        self.assertFalse(t.is_standard_strip(Image.new('RGBA',(513,768)),(256,1024),(2048,3072)))

    def test_roundtrip_and_central_overrides(self):
        t.split_image()
        t.join_image()
        self.assertImage(t.load_source(),self.source)
        (t.FOLDER/'1.png').unlink()
        atlas=Image.new('RGBA',(3*Q,3*Q),(30,50,70,255))
        base=(90,100,110,255)
        atlas.paste(base,(Q,Q,2*Q,2*Q))
        atlas.save(t.FOLDER/'101.png')
        repair=Image.new('RGBA',(3*Q,3*Q),(0,0,0,255))
        patch=(220,200,60,255)
        repair.paste(patch,(Q,Q,2*Q,2*Q))
        repair.save(t.FOLDER/'104.png')
        t.join_image()
        expected=Image.new('RGBA',(2*Q,2*Q),base)
        expected.paste(patch,(0,0,Q,Q))
        self.assertImage(t.open_required('50.png'),expected)
        self.assertImage(t.load_source().crop((Q,3*Q,3*Q,5*Q)),expected)


if __name__=='__main__':
    unittest.main(verbosity=2)
