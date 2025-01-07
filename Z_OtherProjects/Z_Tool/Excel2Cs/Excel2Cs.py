import pandas as pd
import sys
import os
from collections import defaultdict
file_namespace = "Form"
file_using=""
class FormInfo:
    df = None
    base_info = None
    def get_namespace_str(self,namespace):
       return f"""using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_Form;
{file_using}
namespace {file_namespace}
{{
"""
    declare_str = f""""""
    dic_str = f""""""

    serialize_str = f""""""
    deserialize_str = f""""""

    add_str = f""""""
    remove_str = f""""""

    content_str = f""""""

    extend_data_str = ':Form.Data'
    extend_str=':Form'


    id_str = 'id'

    def __init__(self):
        self.var_list = []
        self.var_config_dic = {}
        self.var_annotation_dic = {}
        self.var_type_dic = {}
        self.data_list = []
        self.name = 'default'


    def get_result(self):
        namespace_str = self.get_namespace_str(self.name)

        add_op_base_str = ""
        remove_op_base_str = ""
        add_remove_op_str = ""
        children_str=""
        for data in form_info_list:
            if data.extend_str == self.name:
                children_str+=data.name+"Form,"

        if 'write' in self.var_config_dic[self.id_str]:
            visit
            if  children_str == "":
                add_remove_op_str = f'''
        public int AddData(Data data)
        {{
            {add_op_base_str}
            if(free{self.id_str.capitalize()}Queue.Count==0)
            return -1;
            int {self.id_str}=free{self.id_str.capitalize()}Queue.Dequeue();
            data.{self.id_str}={self.id_str};  
            
            return {self.id_str};
        }}
        public void RemoveData(int {self.id_str})
        {{
            {remove_op_base_str}
            if(!_DataBy{self.id_str.capitalize()}.ContainsKey({self.id_str}))
                return;
            var data = _DataBy{self.id_str.capitalize()}[{self.id_str}];
        }}
'''

        

        return f"""{namespace_str}
    public partial class {self.name}Form{extend_str}
    {{
        private {self.name}Form[] childrenForm = new {self.name}Form[]{{{children_str}}};
        private static _ins;
        public static ins
        {{
            get{{
            if(_ins!=null)
             return ins;
            _ins=new {self.name}Form();
            _ins.Init();
            }}
        }}
        private bool inited;
        private Queue<int> free{self.id_str.capitalize()}Queue;

        public partial class Data{self.extend_data_str}
        {{
{self.declare_str}
        }}
{self.dic_str}

        public void Init()
        {{
            if(inited)
                return;
             for(int i=0;i<childrenForm.Length;i++)
             {{
                childrenForm[i].Init();
             }}

            free{self.id_str.capitalize()}Queue=new  Queue<int> (Enumerable.Range(0, 100));
{self.content_str}
            
            foreach(var k in _DataBy{self.id_str.capitalize()}.Keys)
            {{
                free{self.id_str.capitalize()}Queue.Enqueue(k);
            }}
            
            inited=true;
            base.Init();
        }}

        public List<Data> GetDatasByJa(JArray ja)
        {{
            List<Data> lst=new List<Data>();
            foreach(JObject jo in ja)
            {{
                lst.Add(GetDataByJo(jo));
            }}
            return lst;
        }}

        public JArray GetJaByDatas()
        {{
            JArray ja=new JArray();
            foreach(Data data in _DataBy{self.id_str.capitalize()}.Values)
            {{

                ja.Add(GetJoByData(data));
            }}
            return ja;
        }}

        public Data GetDataByJo(JObject jo)
        {{
{self.deserialize_str}
            return data;
        }}

        public JObject GetJoByData(Data data)
        {{
{self.serialize_str}
            return jo;
        }}

{add_remove_op_str}
    }}
}}
        """

files_root_excels = "Excels/"
files_root_cs = "ExcelCs/"

form_info_list = []

def get_pos(content,left):
    left_pos = content.find(left)
    if left_pos == -1:
        left_pos = len(content)
    return left_pos

def get_naked(content):
    #拆外层括号
    if content[0] == '(' and content[len(content) - 1] == ')':
        deepth = 0
        for i in range(len(content)):
         if content[i] == '(':
             deepth+=1
         if content[i] == ')':
             deepth-=1
         if deepth < 0:
             return content
        return content[1:-1]
    return content

def get_naked_subs(content):
     if content == 'null':
        return []
     #分外层逗号
     values = []
     deepth = 0
     startId = 0
     for i in range(len(content)):
         if content[i] == ',' and deepth == 0:
             values.append(content[startId:i])
             startId = i + 1
         if content[i] == '(':
             deepth+=1
         if content[i] == ')':
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
    value = get_naked(value)
    #转nan
    if value == 'nan':
        value = get_default_form(type)

    if type[0] == '(':
         #()
         types = type[1:-1]
         sub_types = types.split(',')
         sub_values = get_naked_subs(value)
         content = "("
         for i in range(len(sub_types)):
            if len(sub_values) - 1 <= i:
                sub_values.append(get_default_form(sub_types[i]))
            content+=get_value(sub_types[i],sub_values[i]) + ','
         content = content[0:-1] + ')'
         return content
    elif type[0] == '<':
         #<>
         types = type[1:-1]
         sub_types = types.split(',')
         sub_values = get_naked_subs(value)
         content = ""
         for i in range(len(sub_types)):
            if len(sub_values) - 1 <= i:
                sub_values.append(get_default_form(sub_types[i]))
            content+=get_value(sub_types[i],sub_values[i]) + ','
         content = content[0:-1]
         return content
    elif type[0:4] == 'List':
         if value == 'null':
            return 'null'
         values = get_naked_subs(value)
         content = ''
         for v in values:
             content+=get_value(type[4:],v) + ','
         return 'new ' + type + '(){' + content + '}'
    elif type[0:10] == 'Dictionary':
         values = get_naked_subs(value)
         content = ''
         for v in values:
             content+='{' + get_value(type[10:],v) + '},'
         return 'new ' + type + '(){' + content + '}'
    #basic type
    elif type == 'float':
        return value + 'f'
    elif type == 'bool':
        if value == '1':
            return 'true'
        else:
            return 'false'
    elif type == 'string':
        return '"' + str(translate_string_to_cs(value)) + '"'
    else:
        return value
    return 'null'




for i, arg in enumerate(sys.argv):
    if i == 1:
        files_root_excels = arg + files_root_excels
    if i == 2:
        files_root_cs = arg + files_root_cs
    if i == 3:
        file_namespace = arg + "." + file_namespace
    if i == 4:
        file_using = "using "+arg+";";
        
print(f'''start\n''')
for root, dirs, files in os.walk(files_root_excels):
    for file in files:
        if file.endswith('.xlsx') or file.endswith('.xls'):
            print('manage:' + file + "\n")
            formInfo = FormInfo()
            file_name_without_extension = os.path.splitext(file)[0]
            temp_names = file_name_without_extension.split("_")
            formInfo.name = temp_names[0]
            if len(temp_names) > 1:
                formInfo.base_info = temp_names[1]
                formInfo.extend_str = " : " + temp_names[1] +'Form'
                formInfo.extend_data_str = " : " + temp_names[1] + "Data"
            formInfo.df = pd.read_excel(root + "/" + file)

            #第一行，获取名称和配置
            cur_row = formInfo.df.iloc[0]
            for title,content in cur_row.items():
                if len(formInfo.var_list)==0:
                    formInfo.id_str=title
                formInfo.var_list.append(title)
                if pd.isna(content):
                    formInfo.var_config_dic[title] = [None]
                else:
                    formInfo.var_config_dic[title] = content.split(';')
                    
            #第二行，获取注释
            cur_row = formInfo.df.iloc[1]
            for title,content in cur_row.items():
                if not pd.isna(content):
                    formInfo.var_annotation_dic[title] = content

            #第三行，获取类型
            cur_row = formInfo.df.iloc[2]
            for title,content in cur_row.items():
                formInfo.var_type_dic[title] = content
            
            form_info_list.append(formInfo)

for formInfo in form_info_list:
            #构造声明
            for name in formInfo.var_list:
                if 'override' not in formInfo.var_config_dic[name]:
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

            #添加索引
            
            formInfo.dic_str+=f"""
        Dictionary<int, Data> _DataBy{formInfo.id_str.capitalize()} = null;
        public Dictionary<int, Data> DataBy{formInfo.id_str.capitalize()}
        {{
            get
            {{
                Init();
                return _DataBy{formInfo.id_str.capitalize()};
            }}
        }}
"""
            for name in formInfo.var_list:
                if 'uniqueIndex' in formInfo.var_config_dic[name]:
                    formInfo.dic_str+=f"""
        Dictionary<{formInfo.var_type_dic[name]}, Data> _DataBy{name.capitalize()} = null;
        public Dictionary<{formInfo.var_type_dic[name]}, Data> DataBy{name.capitalize()}
        {{
            get
            {{
                Init();
                return _DataBy{name.capitalize()};
            }}
        }}
"""
                if 'index' in formInfo.var_config_dic[name]:
                    formInfo.dic_str+=f"""
        static Dictionary<{formInfo.var_type_dic[name]}, List<Data>> _DatasBy{name.capitalize()} = null;
        public static Dictionary<{formInfo.var_type_dic[name]}, List<Data>> DatasBy{name.capitalize()}
        {{
            get
            {{
                Init();
                return _DatasBy{name.capitalize()};
            }}
        }}
"""

            #类型构造方法设置
            con_extend_str = ':base('
            con_arg_str = ''
            con_set_str = ''
            for key in formInfo.var_type_dic:
                print(key+' '+(str)(formInfo.var_config_dic[key]))
                if 'override' in formInfo.var_config_dic[key]:
                    con_extend_str+=key+','
                con_arg_str+=formInfo.var_type_dic[key] + ' ' + key + ','
                con_set_str+=f'''
                this.{key} = {key};'''

            con_extend_str=con_extend_str[0:-1]+')'
            if formInfo.base_info == None:
                con_extend_str=''
            formInfo.declare_str+=f'''
            public Data({con_arg_str[0:-1]}){con_extend_str}
            {{
{con_set_str}
            }}
            '''
            
            #第四行起，读取并构造内容
            total_rows = formInfo.df.shape[0]
            last_row = None
            for i in range(3,total_rows):
                dic = {}
                cur_row = formInfo.df.iloc[i]
                for title in cur_row.index:
                    #处理auto
                    if ('auto' in formInfo.var_config_dic[title]) and pd.isna(cur_row[title]) and last_row is not None:
                        cur_row[title] = last_row[title]
                    content = cur_row[title]
                    dic[title] = get_value(formInfo.var_type_dic[title],str(content))

                formInfo.data_list.append(dic)
                last_row = cur_row

            #处理初始化
            #id
            formInfo.content_str+=f'''
                _DataBy{formInfo.id_str.capitalize()} = new Dictionary<int, Data>() {{
'''
            formInfo.add_str+=f'''
                _DataBy{formInfo.id_str.capitalize()}[data.{formInfo.id_str}]=data;
'''         
            formInfo.remove_str+=f'''
                _DataBy{formInfo.id_str.capitalize()}.Remove(data.{formInfo.id_str});
'''
            for data in formInfo.data_list:
                args = ''
                for key in data:
                    args+=data[key] + ','
                formInfo.content_str+=f'''
                {{{data[formInfo.id_str]},new Data({args[0:-1]})}},
'''
            formInfo.content_str+=f'''
                }};
'''

            #index
            for name in formInfo.var_list:
                if 'uniqueIndex' in formInfo.var_config_dic[name]:
                    formInfo.content_str+=f'''
                _DataBy{name.capitalize()} = new Dictionary<{formInfo.var_type_dic[name]}, Data>() {{
'''
                    formInfo.add_str+=f'''
                _DataBy{name.capitalize()}[data.{name}]=data;
'''         
                    formInfo.remove_str+=f'''
                _DataBy{name.capitalize()}.Remove(data.{name});
'''
                    for data in formInfo.data_list:
                        formInfo.content_str+=f'''
                    {{{data[name]},_DataBy{formInfo.id_str.capitalize()}[{data['id']}]}},
'''
                    formInfo.content_str+=f'''
                }};
'''
                if 'index' in formInfo.var_config_dic[name]:
                    formInfo.content_str+=f'''
                _DatasBy{name.capitalize()} = new Dictionary<{formInfo.var_type_dic[name]}, List<Data>>() {{
'''
                    formInfo.add_str+=f'''
                _DataBy{name.capitalize()}[data.{name}].Add(data);
'''         
                    formInfo.remove_str+=f'''
                _DataBy{name.capitalize()}[data.{name}].Remove(data);
'''

                    #登记list
                    exist_list = []
                    for data in formInfo.data_list:
                        if data[name] in exist_list:
                            continue
                        exist_list.append(data[name])
                        formInfo.content_str+=f'''
                    {{{data[name]},new List<Data>()}},
'''                 
                    formInfo.content_str+=f'''
                }};
'''
                    #注册
                    for data in formInfo.data_list:
                        formInfo.content_str+=f'''
                    _DatasBy{name.capitalize()}[{data[name]}].Add(_DataBy{formInfo.id_str.capitalize()}[{data[formInfo.id_str]}]);
'''
            #构造序列化
            formInfo.serialize_str = f'''
                    JObject jo=new JObject();
'''               
            for name in formInfo.var_list:
                if name == formInfo.id_str or 'write' in formInfo.var_config_dic[name]:
                    formInfo.serialize_str+=f'''
                    jo.Set<{formInfo.var_type_dic[name]}>("{name}",data.{name});
'''               


            formInfo.deserialize_str = f'''
                    Data data=new Data(
'''               
            for name in formInfo.var_list: 
                if name == formInfo.id_str or 'write' in formInfo.var_config_dic[name]:
                    formInfo.deserialize_str+=f'''
                        jo.Get<{formInfo.var_type_dic[name]}>("{name}"),
'''             
                else:#不可用的取0
                    formInfo.deserialize_str+=f'''
                    _DataBy{formInfo.id_str.capitalize()}[0].{name},
'''               

            #去结尾,
            formInfo.deserialize_str = formInfo.deserialize_str[0:-2]
            formInfo.deserialize_str+=f'''
                    );
'''           

# 打印找到的 Excel 文件
for file in form_info_list:
    print('处理中：' + file.name)
    with open(files_root_cs + file.name + '.cs', 'w') as f:
        f.write(file.get_result())



