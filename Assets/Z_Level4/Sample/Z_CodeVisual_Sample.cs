using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Z_Code;
using Z_CodeVisual;
using Z_Ui;
using Ui.ZCodeEntry;

public class Z_CodeVisual_Sample : MonoBehaviour
{

    void Awake()
    {
        UiManager.instance.ShowUi<UiZCodeEntryCtrl>();
    }

}
