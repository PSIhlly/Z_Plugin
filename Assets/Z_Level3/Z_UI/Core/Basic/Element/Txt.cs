using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Z_Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Z_Ui.Base
{
    public class Txt : TextMeshProUGUI, ITextPreprocessor
    {
        [Tooltip("use language manage")]
        public bool languageTranslatable=true;
        private string oriTxt;
        public string PreprocessText(string text)
        {
            return text;
        }

        protected override void Awake()
        {
            oriTxt = text;
            base.Awake();
        }
        protected override void OnEnable()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Refresh();
            }
#else
            Refresh();
#endif

            base.OnEnable();
        }

        private void Refresh()
        {
            text = TextManager.instance.GetTxt(oriTxt);

        }
    }
}
