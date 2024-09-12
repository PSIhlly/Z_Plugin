using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;
namespace Z_Ui.Dialog
{
    public class MainPictureController : Z_MonoController<MainPictureController>
    {
        public Image mainPictureImg;

        public void Display(Sprite mainPicture)
        {
            if (mainPicture == null)
                return;
            mainPictureImg.sprite = mainPicture;
        }
    }
}