using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Language;

namespace Z_Text
{
    public class TextManager : Z_Manager<TextManager>
    {
        public string GetTxt(string key)
        {
           return LanguageManager.instance.GetTxt(key);
        }

        public override void Init()
        {
        }
    }
}