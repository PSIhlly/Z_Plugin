"""Emit owned copies of Form rows and nested List/Dictionary values."""


def split_types(text):
    parts, start, depth = [], 0, 0
    for index, char in enumerate(text):
        if char in '<([':
            depth += 1
        elif char in '>)]':
            depth -= 1
        elif char == ',' and depth == 0:
            parts.append(text[start:index].strip())
            start = index + 1
    parts.append(text[start:].strip())
    return parts


class CopyEmitter:
    def __init__(self):
        self.helpers = {}

    @staticmethod
    def collection(type_name):
        if '<' not in type_name or not type_name.endswith('>'):
            return None, []
        outer, inner = type_name.split('<', 1)
        return outer.strip(), split_types(inner[:-1])

    def needs_copy(self, type_name):
        type_name = type_name.strip()
        if type_name.endswith('Form.Data'):
            return True
        outer, arguments = self.collection(type_name)
        return outer in ('List', 'Dictionary') or type_name.startswith('(') and any(
            self.needs_copy(part) for part in split_types(type_name[1:-1]))

    def expression(self, type_name, value, field=False):
        type_name = type_name.strip()
        if type_name.endswith('Form.Data'):
            return f'{value}?.Copy()'
        outer, arguments = self.collection(type_name)
        if outer in ('List', 'Dictionary'):
            empty = f'new {type_name}()' if field else 'null'
            if any(self.needs_copy(arg) for arg in arguments):
                helper = self.helper(type_name, outer, arguments)
                return f'{value}==null?{empty}:{helper}({value})'
            return f'{value}==null?{empty}:new {type_name}({value})'
        if type_name.startswith('(') and type_name.endswith(')'):
            parts = split_types(type_name[1:-1])
            if any(self.needs_copy(part) for part in parts):
                return '(' + ','.join(self.expression(part, f'{value}.Item{index + 1}')
                                      for index, part in enumerate(parts)) + ')'
        return value

    def helper(self, type_name, outer, arguments):
        if type_name in self.helpers:
            return self.helpers[type_name][0]
        name = f'CopyCollection{len(self.helpers)}'
        self.helpers[type_name] = (name, '')
        if outer == 'List':
            entry = self.expression(arguments[0], 'item')
            body = f'foreach (var item in source) result.Add({entry});'
            constructor = f'new {type_name}(source.Count)'
        else:
            key = self.expression(arguments[0], 'pair.Key')
            value = self.expression(arguments[1], 'pair.Value')
            body = f'foreach (var pair in source) result.Add({key}, {value});'
            constructor = f'new {type_name}(source.Count, source.Comparer)'
        code = f'''
            private static {type_name} {name}({type_name} source)
            {{
                var result = {constructor};
                {body}
                return result;
            }}
'''
        self.helpers[type_name] = (name, code)
        return name

    def methods(self):
        return ''.join(code for _, code in self.helpers.values())
