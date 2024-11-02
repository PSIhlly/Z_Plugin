import pandas as pd
import os
class FormInfo:
    
    def get_namespace_str(self,namespace):
       return f"""using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace Form.{namespace}
{{
"""
    declare_str = f""""""
    content_str = f""""""
    def __init__(self):
        self.var_list = []
        self.var_config_dic = {}
        self.var_annotation_dic = {}
        self.var_type_dic = {}
        self.data_list = []
        self.name = 'default'

    def get_result(self):
        namespace_str=self.get_namespace_str(self.name)
        return f"""{namespace_str}
    public static partial class {self.name}
    {{
        public class Data
        {{
{self.declare_str}
        }}
        static IReadOnlyDictionary<int, Data> _Datas = null;
        public static IReadOnlyDictionary<int, Data> Datas
        {{
            get
            {{
                Init();
                return _Datas;
            }}
        }}

        static void Init()
        {{
            _Datas = new Dictionary<int, Data>() {{
{self.content_str}
            }};
        }}
    }}
}}
        """

files_root_excels = "Excels/"
files_root_cs = "ExcelCs/"

form_info_list = []

def get_pos(content,left):
    left_pos=content.find(left)
    if left_pos == -1:
        left_pos=len(content)
    return left_pos

def get_naked(content):
    #拆外层括号
    if content[0]=='(' and content[len(content)-1]==')':
        deepth=0
        for i in range(len(content)):
         if content[i]=='(':
             deepth+=1
         if content[i]==')':
             deepth-=1
         if deepth<0:
             return content
        return content[1:-1]
    return content

def get_naked_subs(content):
     if content=='null':
        return []
     #分外层逗号
     values=[]
     deepth=0
     startId=0
     for i in range(len(content)):
         if content[i]==',' and deepth == 0:
             values.append(content[startId:i])
             startId=i+1
         if content[i]=='(':
             deepth+=1
         if content[i]==')':
             deepth-=1

     if startId != len(content):
         values.append(content[startId:len(content)])

     return values

def get_default_form(type):
    if type == 'int' or type == 'float' or type == 'bool':
        return '0'
    if type == 'string':
        return ''
    return 'null'
def translate_string_to_cs(content):
    return content.replace('\\','\\\\')
def get_value(type,value):
    #拆外壳
    value=get_naked(value)
    #转nan
    if value=='nan':
        value=get_default_form(type)

    print(type+"  after "+value)
    if type[0]=='(':
         #()
         types=type[1:-1]
         sub_types=types.split(',')
         sub_values=get_naked_subs(value)
         content="("
         for i in range(len(sub_types)):
            if len(sub_values)-1 <=i:
                sub_values.append(get_default_form(sub_types[i]))
            content+=get_value(sub_types[i],sub_values[i])+','
         content=content[0:-1]+')'
         return content
    elif type[0]=='<':
         #<>
         types=type[1:-1]
         sub_types=types.split(',')
         sub_values=get_naked_subs(value)
         content=""
         for i in range(len(sub_types)):
            if len(sub_values)-1 <=i:
                sub_values.append(get_default_form(sub_types[i]))
            content+=get_value(sub_types[i],sub_values[i])+','
         content=content[0:-1]
         return content
    elif type[0:4]=='List':
         if value == 'null':
            return 'null'
         values=get_naked_subs(value)
         content=''
         for v in values:
             content+=get_value(type[4:],v)+','
         return 'new '+type+'(){'+content+'}'
    elif type[0:10]=='Dictionary':
         values=get_naked_subs(value)
         content=''
         for v in values:
             content+='{'+get_value(type[10:],v)+'},'
         return 'new '+type+'(){'+content+'}'
    #basic type
    elif type == 'float':
        return value+'f'
    elif type == 'bool':
        if value == '1':
            return 'true'
        else:
            return 'false'
    elif type == 'string':
        return '"'+str(translate_string_to_cs(value))+'"'
    else:
        return value
    return 'null'


for root, dirs, files in os.walk(files_root_excels):
    for file in files:
        if file.endswith('.xlsx') or file.endswith('.xls'):
            formInfo = FormInfo()
            file_name_without_extension = os.path.splitext(file)[0]
            formInfo.name = f"{file_name_without_extension}"

            df = pd.read_excel(root + "/" + file)

            #第一行，获取名称和配置
            cur_row = df.iloc[0]
            for title,content in cur_row.items():
                formInfo.var_list.append(title)
                if pd.isna(content):
                    formInfo.var_config_dic[title] = [None]
                else:
                    formInfo.var_config_dic[title] = content.split(';')
                    
            #第二行，获取注释
            cur_row = df.iloc[1]
            for title,content in cur_row.items():
                if not pd.isna(content):
                    formInfo.var_annotation_dic[title] = content

            #第三行，获取类型
            cur_row = df.iloc[2]
            for title,content in cur_row.items():
                formInfo.var_type_dic[title] = content

            #构造声明
            for name in formInfo.var_list:
                declare = ''
                if name in formInfo.var_annotation_dic:
                    declare+=f'''
            /// <summary>
            ///{formInfo.var_annotation_dic[name]}
            ///</summary>
            '''
                else:
                    declare+=f'''
            '''
                declare+='public '
                if 'write' not in formInfo.var_config_dic[name]:
                    declare+='readonly '
                #类型描述
                if 'custom' in formInfo.var_config_dic[name]:
                    declare+=formInfo.var_type_dic[name] + ' '#暂时一样
                else:
                    declare+=formInfo.var_type_dic[name] + ' '
                declare+=name
                formInfo.declare_str+=declare + ';\n'

            #类型构造方法设置
            con_arg_str=''
            con_set_str=''
            for key in formInfo.var_type_dic:
                con_arg_str+=formInfo.var_type_dic[key]+' '+key+','
                con_set_str+=f'''
                this.{key} = {key};'''
            formInfo.declare_str+=f'''
            public Data({con_arg_str[0:-1]})
            {{
{con_set_str}
            }}
            '''
            
            #第四行起，读取并构造内容
            total_rows = df.shape[0]
            last_row=None
            for i in range(3,total_rows):
                dic = {}
                cur_row = df.iloc[i]
                for title in cur_row.index:
                    #处理auto
                    if ('auto' in formInfo.var_config_dic[title]) and pd.isna(cur_row[title]) and last_row is not None:
                        cur_row[title]=last_row[title]
                    content=cur_row[title]
                    dic[title] = get_value(formInfo.var_type_dic[title],str(content))

                formInfo.data_list.append(dic)
                last_row=cur_row

            for data in formInfo.data_list:
                args=''
                for key in data:
                    args+=data[key]+','
                formInfo.content_str+=f'''
                {{{data['id']},new Data({args[0:-1]})}},
'''
            form_info_list.append(formInfo)

# 打印找到的 Excel 文件
for file in form_info_list:
    print('处理中：' + file.name)
    with open(files_root_cs + file.name + '.cs', 'w') as f:
        f.write(file.get_result())



