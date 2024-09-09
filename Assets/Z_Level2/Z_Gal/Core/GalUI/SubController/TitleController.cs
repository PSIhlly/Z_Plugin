using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Z_Gal.GalUI
{
    public class TitleController : MonoBehaviour
    {
        public GalUIManagerBaseType galUIBaseType;
        public Text titleText; 

        public void Display(string title)
        {
            if (string.IsNullOrEmpty(title))
                return;
            titleText.text = title;
        }
    }
}