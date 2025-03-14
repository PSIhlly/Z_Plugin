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
