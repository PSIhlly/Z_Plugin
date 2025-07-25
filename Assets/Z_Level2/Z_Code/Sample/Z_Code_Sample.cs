using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Z_Code;
using Z_Code.Form;

public class Z_Code_Sample : MonoBehaviour
{
    public Button runBtn;
    public Button rebuildBtn;
    void Awake()
    {
        var cpr = new Compiler();
        var dcpr = new Decompiler();

        var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
        var res=cpr.Compile(code, out var syntaxs);
        var program = new ProgramDataForm.Data(-1,"",code,res);
        var itp = new InterpretDataForm.Data(-1,new List<BoxDataForm.Data>(),new Dictionary<string, BoxDataForm.Data>(), program,0,-1);
        itp.Interpret();

        runBtn.onClick.AddListener(()=>
        {
            itp.program.code= gameObject.GetComponentInChildren<TMP_InputField>().text;
            itp.program.zCode = cpr.Compile(code,out var syntaxs);
            itp.Interpret();
        });
        rebuildBtn.onClick.AddListener(() =>
        {
            var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
            var res = cpr.Compile(code, out var syntaxs);
            gameObject.GetComponentInChildren<TMP_InputField>().text = dcpr.Decompile(syntaxs);
        });
    }
}
