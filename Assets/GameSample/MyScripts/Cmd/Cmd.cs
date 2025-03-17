using Item;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Debug.Z_Cmd;
using Z_DesignStyle;

public class Cmd:Z_Singleton<Cmd>
{
    private Z_Cmd cmd;
    public Cmd()
    {
        cmd=new Z_Cmd(GetType());
    }
    public void Excute(string content)
    {
        cmd.Excute(content);
    }
    public static void AddItem(int id,int count)
    {
        ItemManager.instance.AddItem(id, count);
    }
}
