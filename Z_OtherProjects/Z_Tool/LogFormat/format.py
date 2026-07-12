import os
import re
import sys

def main():
    try:
        folder_path = os.path.dirname(os.path.abspath(__file__))
        
        # 终极兼容正则：
        # re.IGNORECASE: 忽略大小写
        # re.DOTALL: 让 . 可以匹配换行符
        # (\r?\n){2}: 兼容 Windows(\r\n) 和 Linux/Mac(\n) 的连续两个换行符
        pattern = re.compile(r'UnityEngine\.Debug:Log\s*\(object\).*?(\r?\n){2}', re.DOTALL | re.IGNORECASE)
        
        processed_count = 0
        for filename in os.listdir(folder_path):
            if filename.lower().endswith(".txt"):
                file_path = os.path.join(folder_path, filename)
                try:
                    with open(file_path, 'r', encoding='utf-8') as f:
                        content = f.read()
                    
                    new_content = pattern.sub('', content)
                    
                    if content != new_content:
                        with open(file_path, 'w', encoding='utf-8') as f:
                            f.write(new_content)
                        print(f"[成功] 已清理: {filename}")
                        processed_count += 1
                    else:
                        print(f"[跳过] 未发现目标内容: {filename}")
                        # 【核心排查功能】如果没匹配到，打印出文件开头的内容，帮你看看真实格式
                        preview = content[:200].replace('\n', '\\n').replace('\r', '\\r')
                        print(f"       -> 文件前200个字符预览: [{preview}]")
                        
                except Exception as e:
                    print(f"[失败] 处理 {filename} 时发生错误: {e}")
                    
        print(f"\n处理完毕！共成功清理了 {processed_count} 个文件。")
        
    except Exception as e:
        print(f"\n发生致命错误: {e}")
        
    input("\n操作已结束，请按回车键退出窗口...")

if __name__ == "__main__":
    main()