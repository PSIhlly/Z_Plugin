using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Z_Ui.Base
{
    public class Ipt : TMP_InputField
    {
        public Action<string> onInput;
        private bool invokeAction;
        public Ipt()
        {
            onValueChanged.AddListener((v) =>
            {
                if (invokeAction)
                {
                    onInput?.Invoke(v);
                }
            });
        }
        public void Set(string content, bool onlyNotFocus = true,bool invokeAction = false)
        {
            if (isFocused && onlyNotFocus)
                return;
            this.invokeAction = invokeAction;
            text = content;
            this.invokeAction = true;
        }
        
    }
}
