using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Ui;
using Ui.DialogBg;
using Ui.DialogHistory;
using Ui.DialogMain;
using Ui.Loading;
using Unity.VisualScripting;
using UnityEngine;
using Z_DesignStyle;
using Z_Time;
using Z_Ui.Form;
using static UnityEditor.PlayerSettings;
namespace Z_Ui.Loading
{
    public enum LoadingState
    {
        Loading,
        Done
    }
    public class LoadingEvent : Z_Event
    {
        public LoadingState state;
    }
    public class LoadingManager : Z_Manager<LoadingManager>
    {
        HashSet<string> loadingItems;
        public override void Init()
        {
            loadingItems = new HashSet<string>();
        }
        public void AddLoadItem(string loadItem)
        {
            loadingItems.Add(loadItem);
            UiManager.instance.ShowUi<UiLoadingCtrl>();
        }
        public void RemoveLoadItem(string loadItem)
        {
            loadingItems.Remove(loadItem);
            if(loadingItems.Count==0)
            {
                UiManager.instance.CloseUi<UiLoadingCtrl>();
            }
        }

    }
}
