using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Z_Gal.GalUI
{
    public class ProfilePictureController : MonoBehaviour
    {
        public GalUIManagerBaseType galUIBaseType;
        public Image profilePictureImage;


        public void Display(Sprite profilePicture)
        {
            if (profilePicture == null)
                return;
            profilePictureImage.sprite = profilePicture;
        }
    }
}