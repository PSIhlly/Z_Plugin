using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Text.Form;

namespace Z_Language
{
    public enum Language
    {
        En,
        Cn,
    }
    public class LanguageManager : Z_Manager<LanguageManager>
    {
        public Language language
        {
            private set;
            get;
        } = Language.En;
        public void SetLanguage(Language language)
        {
            this.language = language;
        }

        public string GetTxt(string key)
        {
            if(!TextBaseForm.DataByKey.ContainsKey(key))
            {
                Debug.LogError("Language text key: "+ key + " not exist!");
                return key;
            }
            switch (language)
            {
                case Language.Cn:
                    return TextBaseForm.DataByKey[key].contentCn;
                case Language.En:
                default:
                    return TextBaseForm.DataByKey[key].contentEn;
            }
        }

        public override void Init()
        {
        }
    }

}
