using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Z_Code;
using Z_CodeVisual;

public class Z_CodeVisual_Sample : MonoBehaviour
{
    public Button runBtn;
    public Button rebuildBtn;
    public Button buildBtn;

    public EntryItem item;
    public EntryUnit unit;
    public Transform unitRoot;
    public Transform itemRoot;
    void Awake()
    {
        var cpr = new Compiler();
        var dcpr = new Decompiler();
        var entry = new Entry(unit, item, unitRoot, itemRoot);

        var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
        var res = cpr.Compile(code, out var syntaxs);

        var itp = new Interpreter(res);
        itp.Interpret();

        runBtn.onClick.AddListener(() =>
        {
            var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
            var res = cpr.Compile(code, out var syntaxs);
            var itp = new Interpreter(res);
            itp.Interpret();
        });
        rebuildBtn.onClick.AddListener(() =>
        {
            var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
            var res = cpr.Compile(code, out var syntaxs);
            gameObject.GetComponentInChildren<TMP_InputField>().text = dcpr.Decompile(syntaxs);
        });
        buildBtn.onClick.AddListener(() =>
        {
            var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
            var res = cpr.Compile(code, out var syntaxs);
            foreach(var o in item.transform.parent)
            {
                if(o is Transform trs&&trs!=item.transform)
                {
                    Destroy(trs.gameObject);
                }
            }

            foreach(var o in syntaxs)
            {
                var it = entry.CreateItem();
                it.Draw(0,o);
            }
        });
        
    }
}
