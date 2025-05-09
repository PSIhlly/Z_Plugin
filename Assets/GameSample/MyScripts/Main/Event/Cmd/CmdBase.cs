using Form;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdRes
{
    public List<VarForm.Data> v;
    public string ignoreUntilCmd;
    public string ignoreTimesCmd;
    public float progress=1;

}

public abstract class CmdBase 
{
    public abstract CmdRes Execute(CmdForm.Data self,VarForm.Data[] prs, float progress=0);
}
