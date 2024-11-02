import os
import numpy as np
def get_txt_as_npy(path):
    with open(path,'r',encoding='utf-8') as file:
        content=file.read()
        split_content=content.split()
    return np.array(split_content)

if __name__=="__main__":
    root="./dataset/mnisttrain.txt"
    split_content=get_txt_as_npy(root)
    np.save('images.npy', images)
