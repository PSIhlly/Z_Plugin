using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Z_Code;

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

        var itp = new Interpreter(res);
        itp.Interpret();

        runBtn.onClick.AddListener(()=>
        {
            var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
            var res = cpr.Compile(code,out var syntaxs);
            var itp = new Interpreter(res);
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
