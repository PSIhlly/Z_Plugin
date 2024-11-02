import re

def get_bracket_content(string):
    # 使用正则表达式匹配括号内的内容
    matches = re.findall(r'\((.*?)\)', string)
    return matches