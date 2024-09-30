import pandas as pd
import os

class FormInfo:
    name='default'
    def get_title(self):
        return f"""
        public class {self.name}{{

        }}
        """

files_root_excels="Excels/"
files_root_cs="ExcelCs/"

form_info_list=[]




for root, dirs, files in os.walk(files_root_excels):
    for file in files:
        if file.endswith('.xlsx') or file.endswith('.xls'):
            formInfo=FormInfo()

            file_name_without_extension = os.path.splitext(file)[0]
            formInfo.name=f"{file_name_without_extension}.cs"
            form_info_list.append(formInfo);

# 打印找到的 Excel 文件
for file in form_info_list:
    print('处理中：'+file.name)
    with open(files_root_cs+file.name, 'w') as f:
        f.write(file.get_title())
        


