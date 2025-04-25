using Form;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdRes
{
    public List<VarForm.Data> v;
    public bool breakFlag;

}

public abstract class CmdBase 
{
    public abstract CmdRes Execute(VarForm.Data[] prs);

}
