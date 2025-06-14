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


        var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
        var res=cpr.Compile(code);

        var itp = new Interpreter(res);
        itp.Interpret();

        gameObject.GetComponentInChildren<Button>().onClick.AddListener(()=>
        {
            var code = gameObject.GetComponentInChildren<TMP_InputField>().text;
            var res = cpr.Compile(code);
            itp.Interpret();
        });

    }
}
