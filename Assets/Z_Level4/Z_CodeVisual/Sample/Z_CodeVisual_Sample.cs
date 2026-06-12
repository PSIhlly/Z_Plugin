using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Ui.ZCodeEntry;
using UnityEngine;
using UnityEngine.UI;
using Z_Code;
using Z_Code.Form;
using Z_CodeVisual;
using Z_Ui;

public class Z_CodeVisual_Sample : MonoBehaviour
{

    void Awake()
    {
        var testCode = "Print(param1);";
        var cpr = new Compiler();
        var lst = cpr.Compile(testCode, out _, out var count, out var ret,out _,out var errors);
        ProgramDataForm.AddData(new ProgramDataForm.Data(-1, "Test", testCode, lst, null, count, ret));

        UiManager.instance.ShowUi<UiZCodeEntryCtrl>();
    }

}
