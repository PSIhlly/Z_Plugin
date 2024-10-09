import zmail

MAIL ={
    "from": 'hlzygame@163.com',
    "pwd":'EMaYJ3Afk7TkdchV'    
    }

receiver_list=[]

def get_code_content(code):
    content={
    'subject':"HlZy"
    }
    content['content_text']='验证码：'+code;
    return content

def send_code(email,code):
 try:
     server=zmail.server(MAIL['from'],MAIL['pwd'])
     server.send_mail(email,get_code_content(code))
     print("ok")
 except Exception as e:
     print(e);
