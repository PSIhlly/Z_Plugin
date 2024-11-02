import os
import numpy as np
from PIL import Image
def get_images_as_npy(directory):
    images = []
    files=os.listdir(directory)
    files = [f for f in files if f.endswith('.jpg')]
    files.sort(key=lambda x: int(x[:-4]))
    for filename in files:
            # 检查文件是否是.jpg或.jpeg格式
            file_path = os.path.join(directory, filename)
            print(filename)
            try:
                # 打开图像并转换为数组
                img = Image.open(file_path)
                img_array = np.array(img)
                images.append(img_array)
            except Exception as e:
                print(f"无法加载图像 {file_path}: {e}")
    return images

if __name__=="__main__":
    root="./dataset/mnist/train"
    images = get_images_from_directory(root)
    np.save('images.npy', images)
