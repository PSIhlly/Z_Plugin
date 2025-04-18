using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdRes
{
    public object returnV;
    public bool breakFlag;

}

public abstract class CmdBase 
{
    public abstract CmdRes Execute(object[] prs);

}
