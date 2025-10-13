using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace Z_Ui.Base
{
    public class Dp : TMP_Dropdown
    {
        public Action<int> onFinishSelect;
        private bool invokeAction = true;

        private string oldStr;
        public Dp()
        {
            onValueChanged.AddListener((v) =>
            {
                if (invokeAction)
                {
                    onFinishSelect?.Invoke(v);
                }
            });
           
           
        }
        public void Set(int op,bool invokeAction = false)
        {
            var old = this.invokeAction;
            this.invokeAction = invokeAction;
            value = op;
            this.invokeAction = old;
        }

    }

}
