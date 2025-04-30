import pandas as pd
import sys
import os
from formBaseInfo import FormInfo
from dataAnalysis import get_value
from collections import defaultdict
file_namespace = "Form"
file_using=""
files_root_excels = "Excels/"
files_root_cs = "ExcelCs/"


form_info_list = []

def create_forms():
    for root, dirs, files in os.walk(files_root_excels):
        for file in files:
            if file.endswith('.xlsx') or file.endswith('.xls'):
                print('manage:' + file + "\n")
                formInfo = FormInfo(file_using,file_namespace)

                file_name_without_extension = os.path.splitext(file)[0]
                temp_names = file_name_without_extension.split("_")
                formInfo.name = temp_names[0]
                if len(temp_names) > 1:
                    formInfo.extend_data_str =temp_names[1]

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
                    else:
                        formInfo.var_annotation_dic[title] = ''
                #第三行，获取类型
                cur_row = formInfo.df.iloc[2]
                for title,content in cur_row.items():
                    formInfo.var_type_dic[title] = content

                #id特殊处理
                if 'mass' in formInfo.var_config_dic[formInfo.id_str]:
                    formInfo.id_cnt = 1000000
                if 'medium' in formInfo.var_config_dic[formInfo.id_str]:
                    formInfo.id_cnt = 10000
            
                form_info_list.append(formInfo)

def get_dic_info(info,vs):
    type=""
    name=""
    prm=""
    for v in vs:
        type+=info.var_type_dic[v]+","
        name+=v.capitalize()
        prm+=f"data.{v},"
    type=type[0:-1]
    prm=prm[0:-1]
    if len(vs) > 1:
        type=f"({type})"
        prm=f"({prm})"
    return type,name,prm

def args_handle():
    global files_root_excels,files_root_cs,file_namespace,file_using
    for i, arg in enumerate(sys.argv):
        if i == 1:
            files_root_excels = arg + files_root_excels
        if i == 2:
            files_root_cs = arg + files_root_cs
        if i >= 3:
            content = arg.split(':')
            if content[0] == 'namespace':
                file_namespace = content[1] + "." + file_namespace
            if content[0] == 'using':
                file_using = file_using+"using "+content[1]+";\n"
        
def var_sub_handle():
    for formInfo in form_info_list:
     #构造声明
        #外挂式变量并移除
        for name in formInfo.var_list[::-1]:
            if 'sub' in formInfo.var_config_dic[name]:
                if 'override' not in formInfo.var_config_dic[name]:
                    formInfo.declare_str+=f'''
                protected {formInfo.var_type_dic[name]} _{name};
'''
                formInfo.declare_str+=f'''
                /// <summary>
                ///{formInfo.var_annotation_dic[name]}
                ///</summary>
                public {formInfo.var_type_dic[name]} {name}
                {{
                    get
                    {{
                        return ({formInfo.var_type_dic[name]}) _{name};
                    }}
                }}
'''  
                formInfo.declare_sub_str+=f'''
                    _{name}=new {formInfo.var_type_dic[name]}(this);
'''
                formInfo.var_list.remove(name)
                formInfo.var_type_dic.pop(name, 'Not Found')
def var_normal_handle():
    for formInfo in form_info_list:
    #常规声明
        for name in formInfo.var_list:
            if 'override' not in formInfo.var_config_dic[name]:
                name_string=f'''"{name}" '''
                type = formInfo.var_type_dic[name] + ' '

                if 'write' in formInfo.var_config_dic[name]:
                    set_visit=''
                else:
                    set_visit='private'

                if 'write' in formInfo.var_config_dic[name]:
                    set_event=f'''
                    if(_DataBy{formInfo.id_str.capitalize()}!=null&&_DataBy{formInfo.id_str.capitalize()}.ContainsValue(this))
                    {{
                       Change{name.capitalize()}(this,_{name},value); 
                    }}
'''
                else:
                    set_event=''

                set_str=f'''{set_visit} set{{
{set_event}        
                _{name} = value;
                }}
                '''

                #类型描述
                formInfo.declare_str+=f'''
                    private {type} _{name};
                    /// <summary>
                    ///{formInfo.var_annotation_dic[name]}
                    ///</summary>
                    public {type} {name}{{
                                get{{return _{name};}}
{set_str} 
                     }}
                    '''

def dic_handle():
    for formInfo in form_info_list:
    #添加索引
        formInfo.dic_str+=f"""
            static Dictionary<int, Data> _DataBy{formInfo.id_str.capitalize()};
            public static Dictionary<int, Data> DataBy{formInfo.id_str.capitalize()}
            {{
                get
                {{
                    Init();
                    return _DataBy{formInfo.id_str.capitalize()};
                }}
            }}
    """
        combine_dic=defaultdict(list)
        combine_uni_dic=defaultdict(list)
        for name in formInfo.var_list:
            for i in range(0,10):
                if 'uniqueIndex'+str(i) in formInfo.var_config_dic[name]:
                    combine_uni_dic[i].append(name)
            for i in range(0,10):
                if 'index'+str(i) in formInfo.var_config_dic[name]:
                    combine_dic[i].append(name)
            if 'uniqueIndex' in formInfo.var_config_dic[name]:
                combine_uni_dic[name].append(name)
            if 'index' in formInfo.var_config_dic[name]:
                combine_dic[name].append(name)
        for group,vs in combine_dic.items():
            type,name,prm=get_dic_info(formInfo,vs)

            formInfo.dic_str+=f"""
            static Dictionary<{type}, List<Data>> _DatasBy{name};
            public static Dictionary<{type}, List<Data>> DatasBy{name}
            {{
                get
                {{
                    Init();
                    return _DatasBy{name};
                }}
            }}
    """
        for group,vs in combine_uni_dic.items():
            type,name,prm=get_dic_info(formInfo,vs)

            formInfo.dic_str+=f"""
            static Dictionary<{type}, Data> _DataBy{name};
            public static Dictionary<{type}, Data> DataBy{name}
            {{
                get
                {{
                    Init();
                    return _DataBy{name};
                }}
            }}
    """



def create_data_handle():
    for formInfo in form_info_list:
         #类型构造方法设置
        con_extend_str = ':base('
        con_arg_str = ''
        con_copy_str= f'''
                public Data Copy()
                {{
        return new Data(-1,'''
        con_set_str = ''
        for key in formInfo.var_type_dic:
            if key is not formInfo.id_str:
                con_copy_str+=key+','
            if 'override' in formInfo.var_config_dic[key]:
                con_extend_str+=key+','
            con_arg_str+=formInfo.var_type_dic[key] + ' ' + key + ','
            con_set_str+=f'''
             this.{key} = {key};'''
        con_copy_str=con_copy_str[0:-1]+f''');
                }}'''
        con_extend_str=con_extend_str[0:-1]+')'
        if formInfo.extend_data_str == '':
            con_extend_str=''
        formInfo.declare_str+=f'''
            public Data({con_arg_str[0:-1]}){con_extend_str}
            {{
{con_set_str}
{formInfo.declare_sub_str}
            }}
{con_copy_str}
            '''

            
        

def assign_data_handle():
    for formInfo in form_info_list:
    #第四行起，读取并构造内容
        total_rows = formInfo.df.shape[0]
        last_row = None
        for i in range(3,total_rows):
            dic = {}
            cur_row = formInfo.df.iloc[i]
            for title in cur_row.index:
                if 'sub' in formInfo.var_config_dic[title]:
                    continue
                    #处理auto
                if ('auto' in formInfo.var_config_dic[title]) and pd.isna(cur_row[title]) and last_row is not None:
                    cur_row[title] = last_row[title]
                content = cur_row[title]
                if 'custom' in formInfo.var_config_dic[title]:
                    if str(content) == 'nan':
                        content="null"
                    dic[title] = str(content)
                else:
                    dic[title] = get_value(formInfo.var_type_dic[title],str(content))
                    
            formInfo.data_list.append(dic)
            last_row = cur_row

        var_assign=''
        for data in formInfo.data_list:
            args = ''
            for key in data:
                args+=data[key] + ','
            if data[formInfo.id_str]=='0':
                formInfo.default_content_str=f'''
                   public static Data defaultData=new Data({args[0:-1]});
'''
            else:
                var_assign+=f'''
                {{{data[formInfo.id_str]},new Data({args[0:-1]})}},
'''

        formInfo.content_str+=f'''
                _DataBy{formInfo.id_str.capitalize()} = new Dictionary<int, Data>() {{
{var_assign}
                }};'''

def add_remove_handle():
    for formInfo in form_info_list:
        #处理初始化
        formInfo.add_str+=f'''
        DataBy{formInfo.id_str.capitalize()}[data.{formInfo.id_str}]=data;
    '''         
        formInfo.remove_str+=f'''
                    DataBy{formInfo.id_str.capitalize()}.Remove(data.{formInfo.id_str});
    '''
        formInfo.clear_str+=f'''
                    DataBy{formInfo.id_str.capitalize()}.Clear();
    '''
           

def dic_index_handle():
    for formInfo in form_info_list:
        combine_dic=defaultdict(list)
        combine_uni_dic=defaultdict(list)
        #index
        for name in formInfo.var_list:
            for i in range(0,10):
                if 'uniqueIndex'+str(i) in formInfo.var_config_dic[name]:
                    combine_uni_dic[i].append(name)
            for i in range(0,10):
                if 'index'+str(i) in formInfo.var_config_dic[name]:
                    combine_dic[i].append(name)
            if 'uniqueIndex' in formInfo.var_config_dic[name]:
                    combine_uni_dic[name].append(name)
            if 'index' in formInfo.var_config_dic[name]:
                    combine_dic[name].append(name)

        for group,vs in combine_uni_dic.items():
            type,name,prm=get_dic_info(formInfo,vs)


            formInfo.content_str+=f'''
                    _DataBy{name} = new Dictionary<{type}, Data>() {{
    '''
            formInfo.add_str+=f'''
                    DataBy{name}[{prm}]=data;
    '''         
            formInfo.remove_str+=f'''
                    DataBy{name}.Remove({prm});
    '''                   
            formInfo.clear_str+=f'''
                    DataBy{name}.Clear();
    '''
            for data in formInfo.data_list:
                    if data[formInfo.id_str]=='0':
                        continue
                    k=""
                    for v in vs:
                        k+=data[v]+","
                    k=k[0:-1]
                    if len(vs) > 1:
                        k=f"({k})"
                    formInfo.content_str+=f'''
                        {{{k},_DataBy{formInfo.id_str.capitalize()}[{data[formInfo.id_str]}]}},
    '''
            formInfo.content_str+=f'''
                    }};
    '''
        for group,vs in combine_dic.items():
            type,name,prm=get_dic_info(formInfo,vs)
           
            formInfo.content_str+=f'''
                    _DatasBy{name} = new Dictionary<{type}, List<Data>>() {{
    '''
            formInfo.add_str+=f'''
                    if(!DatasBy{name}.ContainsKey({prm}))
                        DatasBy{name}[{prm}]=new List<Data>();
                    DatasBy{name}[{prm}].Add(data);
    '''         
            formInfo.remove_str+=f'''
                    DatasBy{name}[{prm}].Remove(data);
                    if(DatasBy{name}[{prm}].Count==0)
                        DatasBy{name}.Remove({prm});
    '''
            formInfo.clear_str+=f'''
                    DatasBy{name}.Clear();
    '''
                #登记list
            exist_list = []
            for data in formInfo.data_list:
                 if data[formInfo.id_str]=='0':
                        continue
                 k=""
                 for v in vs:
                    k+=data[v]+","
                 k=k[0:-1]
                 if len(vs) > 1:
                    k=f"({k})"
                 if k not in exist_list :
                    exist_list.append(k)
                    formInfo.content_str+=f'''
                            {{{k},new List<Data>()}},
        '''                 
            formInfo.content_str+=f'''
                }};
'''
                #注册
            for data in formInfo.data_list:
                if data[formInfo.id_str]=='0':
                        continue
                k=""
                for v in vs:
                   k+=data[v]+","
                k=k[0:-1]
                if len(vs) > 1:
                   k=f"({k})"
                formInfo.content_str+=f'''
                    _DatasBy{name}[{k}].Add(_DataBy{formInfo.id_str.capitalize()}[{data[formInfo.id_str]}]);
''' 




def serialize_handle():
    for formInfo in form_info_list:
    #构造序列化
        formInfo.serialize_str = f'''
            JObject jo=new JObject();
'''               
        for name in formInfo.var_list:

            if name == formInfo.id_str or 'write' in formInfo.var_config_dic[name] and 'unsave' not in formInfo.var_config_dic[name] :
                formInfo.serialize_str+=f'''
            jo.Set<{formInfo.var_type_dic[name]}>("{name}",data.{name});
'''               


        formInfo.deserialize_str = f'''
            Data data=new Data(
'''               
        for name in formInfo.var_list: 

            if name == formInfo.id_str or 'write' in formInfo.var_config_dic[name] and 'unsave' not in formInfo.var_config_dic[name] :
                formInfo.deserialize_str+=f'''
                jo.Get<{formInfo.var_type_dic[name]}>("{name}"),
'''             
            else:#不可用的取default
                formInfo.deserialize_str+=f'''
                    defaultData.{name},
'''               

        #去结尾,
        formInfo.deserialize_str = formInfo.deserialize_str[0:-2]
        formInfo.deserialize_str+=f'''
                    );
'''           


print(f'''start\n''')
args_handle()
create_forms()
var_sub_handle()
var_normal_handle()
dic_handle()
create_data_handle()
assign_data_handle() 
add_remove_handle()
dic_index_handle()               
serialize_handle()
           
# 打印找到的 Excel 文件
for file in form_info_list:
    print('处理中：' + file.name)
    with open(files_root_cs + file.name + 'Form.cs', 'w') as f:
        f.write(file.get_result())



