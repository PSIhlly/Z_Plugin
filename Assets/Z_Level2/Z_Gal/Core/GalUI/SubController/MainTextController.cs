using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Z_Gal.GalUI
{
    public class MainTextController : MonoBehaviour
    {

        public GalUIManagerBaseType galUIBaseType;


        public Text mainText;
        public bool isDisplaying
        {
            get;
            private set;
        }
        public bool skipCurrent;
        public void Display(string text)
        {
            skipCurrent = false;
            mainText.text = "";
            galUIBaseType.coroutineWork.StartCoroutine(Displaying(text));
        }

        private IEnumerator Displaying(string text)
        {

            isDisplaying = true;
            int now = 0;
            int target = text.Length;
            while(now<target)
            {
                mainText.text += text[now];
                if (!galUIBaseType.settings.skip && !skipCurrent)
                { 
                    yield return new WaitForSeconds(0.3f / galUIBaseType.settings.textDisplaySpeed);
                }
                now++;
            }
            isDisplaying = false;
        }

    }
}
