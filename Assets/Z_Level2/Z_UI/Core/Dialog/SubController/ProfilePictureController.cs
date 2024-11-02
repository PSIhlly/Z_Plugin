using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;

namespace Z_Ui.Dialog
{
    public class ProfilePictureController : Z_MonoController<DialogUiBaseManager>
    {
        public Image profilePictureImg;


        public void Display(Sprite profilePicture)
        {
            if (profilePicture == null)
                return;
            profilePictureImg.sprite = profilePicture;
        }
    }
}