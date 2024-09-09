using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Gal.GalUI
{
    public class Settings
    {
            public float autoPlaySpeed;//0~10
            public bool skip;
            public float textDisplaySpeed=5;//1~10
    }
    public class ClipData
    {
        public string mainText;
        public Sprite mainPicture;
        public Sprite profilePicture;
        public string title;
        public ClipData(string text)
        {
            this.mainText = text;
        }
    }
    public class GalUIManagerBaseType : MonoBehaviour
    {
       
        //sub
        public Settings settings;
        public MainTextController mainTextController;
        public ProfilePictureController profilePictureController;
        public TitleController titleController;
        public MainPictureController mainPictureController;
        public CoroutineWork coroutineWork;

        public GameObject mainUIPanel;
        public GameObject historyUIPanel;

        public void ChangeMainUIActive(bool active)
        {
            mainUIPanel.SetActive(active);
        }
        public void ChangeMainUIActive()
        {
            mainUIPanel.SetActive(!mainUIPanel.activeSelf);
        }
        public bool GetMainUIActive()
        {
            return mainUIPanel.activeSelf;
        }
        public void ChangeHistoryUIActive(bool active)
        {
            historyUIPanel.SetActive(active);
        }
        public void ChangeHistoryUIActive()
        {
            historyUIPanel.SetActive(!mainUIPanel.activeSelf);
        }
        public bool GetHistoryUIActive()
        {
            return historyUIPanel.activeSelf;
        }
        // Start is called before the first frame update
        public void StartClip(ClipData data)
        {
            
            titleController.Display(data.title);
            mainPictureController.Display(data.mainPicture);
            profilePictureController.Display(data.profilePicture);
            mainTextController.Display(data.mainText);
        }

    }
}
