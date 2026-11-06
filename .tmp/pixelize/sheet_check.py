# -*- coding: utf-8 -*-
import os
from PIL import Image

p = r"D:\Works\Unity's\Personal\Z_Plugin\Z_Plugin\.tmp\pixelize\preview"
for f in sorted(os.listdir(p)):
    if not f.endswith(".png"):
        continue
    im = Image.open(os.path.join(p, f)).convert("RGB")
    print(f, im.size, "unique_colors=", len(im.getcolors(maxcolors=1 << 24) or []),
          "bytes=", os.path.getsize(os.path.join(p, f)))
