import pandas as pd
import os

files_root="Excels/"
excel_files=[]

for root, dirs, files in os.walk(files_root):
    for file in files:
        if file.endswith('.xlsx') or file.endswith('.xls'):
            excel_files.append(os.path.join(root, file))

# 打印找到的 Excel 文件
for file in excel_files:
    print(file)
