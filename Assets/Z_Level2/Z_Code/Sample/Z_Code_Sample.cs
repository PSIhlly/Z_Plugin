using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Z_Code;

public class Z_Code_Sample : MonoBehaviour
{
    void Awake()
    {
        var cpr = new Compiler();

        var itp = new Interpreter();

        var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
        var res=cpr.Compile(code);
        itp.Interpret(res);

        gameObject.GetComponentInChildren<Button>().onClick.AddListener(()=>
        {
            var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
            var res = cpr.Compile(code);
            itp.Interpret(res);
        });

    }
}
