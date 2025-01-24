using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Debug.Z_Cmd
{
    public class Z_Cmd
    {
        public Type cmdType;
        public Z_Cmd(Type cmdType)
        {
            this.cmdType = cmdType;
        }
        public void Excute(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return;
            var cmds = cmd.Split(" ");
            
            // 查找对应的方法
            MethodInfo method = cmdType.GetMethod(cmds[0]);

            if (method != null)
            {
                // 解析参数
                ParameterInfo[] parameterInfos = method.GetParameters();
                object[] args = null;

                if (cmds.Length > 1)
                {
                    // 处理参数字符串，假设参数是以空格分隔
                    
                    // 根据参数类型构造正确的参数
                    args = new object[parameterInfos.Length];
                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        // 将参数值转换为正确的类型
                        args[i] = Convert.ChangeType(cmds[i+1], parameterInfos[i].ParameterType);
                    }
                }
                // 调用方法
                method.Invoke(this, args);
                Debug.Log($"{cmds[0]} invoke！");
            }
            else
            {
                Debug.LogError($"{cmds[0]} not exist！");
            }
        }

        
    }
}

