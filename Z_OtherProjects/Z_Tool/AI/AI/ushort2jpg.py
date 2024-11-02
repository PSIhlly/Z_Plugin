import numpy
import math
import torch
import os
from skimage import io
import torchvision.datasets.mnist as mnist

def softmax(in_mat):
   m,n = numpy.shape(in_mat)
   out_mat=numpy.zeros((m,n))
   tot=0
   for i in range(0,n):
       out_mat[0,i]=math.exp(in_mat[0,i])
       tot+=out_mat[0,i]
   for i in range(0,n):
       out_mat[0,i]=out_mat[0,i]/tot
   return out_mat

if __name__=="__main__":

    root="./dataset/mnist"
    train_set = (
        mnist.read_image_file(os.path.join(root, 'train-images.idx3-ubyte')),
        mnist.read_label_file(os.path.join(root, 'train-labels.idx1-ubyte'))
            )
    test_set = (
        mnist.read_image_file(os.path.join(root, 't10k-images.idx3-ubyte')),
        mnist.read_label_file(os.path.join(root, 't10k-labels.idx1-ubyte'))
            )
    print("training set :",train_set[0].size())
    print("test set :",test_set[0].size())

    def convert_to_img(train=True):
        if(train):
            f=open(root+'train.txt','w')
            data_path=root+'/train/'
            if(not os.path.exists(data_path)):
                os.makedirs(data_path)
            for i, (img,label) in enumerate(zip(train_set[0],train_set[1])):
                img_path=data_path+str(i)+'.jpg'
                io.imsave(img_path,img.numpy())
                f.write(img_path+' '+str(label)+'\n')
            f.close()
        else:
            f = open(root + 'test.txt', 'w')
            data_path = root + '/test/'
            if (not os.path.exists(data_path)):
                os.makedirs(data_path)
            for i, (img,label) in enumerate(zip(test_set[0],test_set[1])):
                img_path = data_path+ str(i) + '.jpg'
                io.imsave(img_path, img.numpy())
                f.write(img_path + ' ' + str(label) + '\n')
            f.close()

    convert_to_img(True)#转换训练集
    convert_to_img(False)#转换测试集

#x_train = numpy.load(./)