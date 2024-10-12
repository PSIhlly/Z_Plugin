import numpy
import math
import torch

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
    print(__name__)

#x_train = numpy.load(./)