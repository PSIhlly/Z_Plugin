using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;

namespace Z_Ui.Dialog
{
    public class MainTextController : Z_MonoController<DialogUiBaseManager>
    {
        public Text mainText;
        public Button skipCurrentBtn;
        public bool isDisplaying
        {
            get;
            private set;
        }
        public bool skipCurrent;

        public void Awake()
        {
            skipCurrentBtn.onClick.AddListener(() =>
            {
                skipCurrent = true;
            });
        }

        public void Display(string text)
        {
            skipCurrent = false;
            mainText.text = "";
            _manager.coroutineWork.StartCoroutine(Displaying(text));
        }

        private IEnumerator Displaying(string text)
        {
            isDisplaying = true;
            int now = 0;
            int target = text.Length;
            while(now<target)
            {
                mainText.text += text[now];
                if (!skipCurrent)
                { 
                    yield return new WaitForSeconds(0.3f / _manager.settings.textDisplaySpeed);
                }
                now++;
            }
            isDisplaying = false;
            Z_EventHelper.Invoke(new ClipPlayEvent() { 
                isOver=true
            });
        }

    }
}
