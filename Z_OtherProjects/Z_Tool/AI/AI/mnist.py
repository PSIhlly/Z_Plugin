
import sys
import os
import numpy as np
import torch
import unet
import matplotlib.pyplot as plt
from tqdm import tqdm
import torch.nn as nn
#引入其他自定义
import txt2npy
import z_string
def get_splited_answer():
    content = txt2npy.get_txt_as_npy("./dataset/mnisttrain.txt")
    mes = []
    for i in range(len(content)):
        if i % 2 == 0:
            continue
        mes.append(int(z_string.get_bracket_content(content[i])[0]))
    return np.array(mes)

#环境设定
os.environ["CUDA_VISIBLE_DEVICES"] = '0'
batch_size = 320
epochs = 1024
device = "cpu"

class NeuralNetwork(torch.nn.Module):
    def __init__(self):
        super(NeuralNetwork,self).__init__()
        self.flatten = torch.nn.Flatten()
        self.linear_relu_stack = torch.nn.Sequential(torch.nn.Linear(28 * 28,312),
            torch.nn.ReLU(),
            torch.nn.Linear(312,256),
            torch.nn.ReLU(),
            torch.nn.Linear(256,10))
    def forward(self,input):
        x = self.flatten(input)
        logits = self.linear_relu_stack(x)
        return logits

model = NeuralNetwork()
model = model.to(device)
'''
torch._dynamo.config.suppress_errors = True
model = torch.compile(model)
'''
loss_fu = torch.nn.CrossEntropyLoss()
optimizer = torch.optim.Adam(model.parameters(),lr=2e-5)
#


x_train = np.load("./images.npy")
y_train_label = get_splited_answer()
train_num = len(x_train) // batch_size

for epoch in range(20):
    train_loss = 0
    for i in range(train_num):
        start = i * batch_size
        end = (i + 1) * batch_size
        train_batch = torch.tensor(x_train[start:end]).float().to(device)
        label_batch = torch.tensor(y_train_label[start:end]).to(device)
        pred = model(train_batch)
        loss = loss_fu(pred,label_batch)
        optimizer.zero_grad()
        loss.backward()
        optimizer.step()
        train_loss+=loss.item()
    train_loss/=train_num
    accuracy = (pred.argmax(1) == label_batch).type(torch.float32).sum().item() / batch_size
    print("train_loss:",round(train_loss,2),"accuracy:",round(accuracy,2))