using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;

namespace Z_Ui.Dialog
{
    public class TitleController : Z_MonoController<DialogUiBaseManager>
    {
        public Text titleText; 

        public void Display(string title)
        {
            if (string.IsNullOrEmpty(title))
                return;
            titleText.text = title;
        }
    }
}