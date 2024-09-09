using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Z_Gal.GalUI
{
    public class MainPictureController : MonoBehaviour
    {
        public GalUIManagerBaseType galUIBaseType;
        public Image mainPictureImage;


        public void Display(Sprite mainPicture)
        {
            if (mainPicture == null)
                return;
            mainPictureImage.sprite = mainPicture;
        }
    }
}