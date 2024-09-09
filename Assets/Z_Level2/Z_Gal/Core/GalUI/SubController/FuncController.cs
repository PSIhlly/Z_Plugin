using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Z_Gal.GalUI
{
    public class FuncController : MonoBehaviour
    {
        public GalUIManagerBaseType galUIBaseType;


        public Text autoPlayText;
        public Text skipText;
        public Text hideUIText;
        public Text historyText;

        public Button autoPlayBtn;
        public Button skipBtn;
        public Button hideBtn;
        public Button historyBtn;

        public Image autoPlayImg;
        public Image skipImg;
        public Image hideImg;
        public Image historyImg;

        public Sprite selectedIcon;
        public Sprite unselectIcon;

        public void Awake()
        {
            autoPlayBtn.onClick.AddListener(()=> 
            {
                ClickAutoPlay(); 
            });
            skipBtn.onClick.AddListener(() =>
            {
                ClickSkip();
            });
            hideBtn.onClick.AddListener(() =>
            {
                ClickHide();
            });
            historyBtn.onClick.AddListener(() =>
            {
                ClickHistory();
            });
        }



        public void ClickAutoPlay()
        {
            CalcAutoPlaySpeed();


            if(galUIBaseType.settings.autoPlaySpeed==0)
            {
                autoPlayText.text = "自动播放";
                autoPlayImg.sprite = unselectIcon;
            }
            else
            {
             autoPlayText.text = galUIBaseType.settings.autoPlaySpeed+"倍速";
             autoPlayImg.sprite = selectedIcon;
            }
            
        }
       
        public void ClickSkip()
        {
            galUIBaseType.settings.skip = !galUIBaseType.settings.skip;
        }
        public void ClickHide()
        {
            galUIBaseType.ChangeMainUIActive(false);
        }
        public void ClickHistory()
        {
            galUIBaseType.ChangeHistoryUIActive(true);
        }
 
        //

        private void CalcAutoPlaySpeed()
        {
            if (galUIBaseType.settings.autoPlaySpeed == 0)
            {
                galUIBaseType.settings.autoPlaySpeed = 1;
            }
            else
            {
                galUIBaseType.settings.autoPlaySpeed *= 2;
            }


            if (galUIBaseType.settings.autoPlaySpeed > 10)
            {
                galUIBaseType.settings.autoPlaySpeed = 0;
            }
        }

    }
}