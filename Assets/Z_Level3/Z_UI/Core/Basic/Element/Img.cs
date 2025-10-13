using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_Language;

namespace Z_Ui.Base
{
    public class Img : Image
    {
        [SerializeField]
        public Sprite cn;
        public Sprite en;
        protected override void OnEnable()
        {
            switch (LanguageManager.instance.language)
            {
                case Language.Cn:
                    if (cn != null)
                    {
                        sprite = cn;
                    }
                    break;
                default:
                    if (en != null)
                    {
                        sprite = en;
                    }
                    break;
            }
            base.OnEnable();
        }
    }
}
