import sqlite3

lock = threading.Lock()

cursor=None

def connect_users():
    
    lock.acquire();
    #   创建或连接到数据库
    connection = sqlite3.connect('users.db',check_same_thread=False)
    global cursor 
    cursor= connection.cursor()
    cursor.execute('''
    CREATE TABLE IF NOT EXISTS users (
        id INTEGER PRIMARY KEY,
        account TEXT NOT NULL,
        password INTEGER NOT NULL
    )
    ''')
    # 提交事务
    connection.commit()
    lock.release();

def insert_user(account,psw):
    lock.acquire();
    cursor.execute("INSERT INTO users (account, password) VALUES (?, ?)", (account, psw))
    connection.commit()
    lock.release();

def find_user(account):
    
    lock.acquire();
    cursor.execute("SELECT * FROM users")
    rows = cursor.fetchall()
    lock.release();
    if(len(rows)>0):
        return False;
    return True;

def clear():
    lock.acquire();
    cursor.execute("DROP TABLE users")
    connection.commit()
    lock.release();