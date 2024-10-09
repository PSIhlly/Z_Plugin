import time
import random
import mail
import db
import threading
import ctypes
from ctypes import c_int, CFUNCTYPE,c_ulong,c_ushort,c_char_p
from enum import Enum

#初始变量
class ProtoType(Enum):
    UDP = 0
    TCP = 1

code_map={}
port_login=44441
cmdList=[]
#

# 初始化
dll_core = ctypes.WinDLL('./Z_Server.dll')
db.connect_users()
dll_core.init()
#

def sendByString(port_login,id_ip,id_port,content):
    chars=content.encode('utf-8');
    dll_core.sendMassage(port_login,id_ip,id_port,chars,len(chars));

def generate_random_number():
    # 生成六位随机数字
    random_number = ''.join(random.choices('0123456789', k=6))
    return random_number


def thread_function(type,port,on_receive,on_close):
    dll_core.run(type.value,port,on_receive,on_close)
   

'''
1$account 获取验证码
2$account$code 验证验证码
'''
def on_receive_callback(id_ip,id_port,mes,mes_length):
    truncated_bytes = mes[:mes_length]
    string_result = truncated_bytes.decode('utf-8')
    print(string_result)
    res = string_result.split('$')
   

    if(res[0]=='1'):
        code=generate_random_number()
        code_map[res[1]]=code
        #mail.send_code(res[1],code)
        print(code)
        sendByString(port_login,id_ip,id_port,"1$请输入验证码");
        
    elif(res[0]=='2'):
        if(code_map[res[1]]==res[2]):
            if(db.find_user(res[1])==False):
                db.insert_user(res[1],"tmp")
            sendByString(port_login,id_ip,id_port,"2$1$登陆成功");
        else:
            sendByString(port_login,id_ip,id_port,"2$0$验证码不正确");
            
def on_close_callback(id_ip,id_port):
    print("Callback called after 1 seconds!")

ONRECEIVE_CALLBACK_FUNC_TYPE = CFUNCTYPE(None, c_ulong,c_ushort,c_char_p,c_int)
ONCLOSE_CALLBACK_FUNC_TYPE = CFUNCTYPE(None, c_ulong,c_ushort)

on_receive_callback_instance = ONRECEIVE_CALLBACK_FUNC_TYPE(on_receive_callback)
on_close_callback_instance = ONCLOSE_CALLBACK_FUNC_TYPE(on_close_callback)

thread1 = threading.Thread(target=thread_function, args=(ProtoType.TCP,port_login,on_receive_callback_instance,on_close_callback_instance))
thread1.start()
thread1.join()

    