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
        public Action<string> onFinishInput;
        private bool invokeAction=true;

        private string oldStr;
        public Ipt()
        {
            onValueChanged.AddListener((v) =>
            {
                if (invokeAction)
                {
                    onInput?.Invoke(v);
                }
            });
            onSelect.AddListener((v)=>
            {
                oldStr = v;
            });
            onDeselect.AddListener((v) =>
            {
                if (invokeAction&& oldStr != text)
                {
                    oldStr = text;
                    onFinishInput?.Invoke(v);
                }
            });
            onSubmit.AddListener((v) =>
            {
                if (invokeAction && oldStr != text)
                {
                    oldStr = text;
                    onFinishInput?.Invoke(v);
                }
            });
        }
        public void Set(string content, bool onlyNotFocus = true,bool invokeAction = false)
        {
            if (isFocused && onlyNotFocus)
                return;
            this.invokeAction = invokeAction;
            text = content;
            oldStr = text;
            this.invokeAction = true;
        }
        
    }
}
