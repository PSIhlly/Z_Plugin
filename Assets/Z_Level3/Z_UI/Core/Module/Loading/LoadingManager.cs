using System.Collections.Generic;
using Ui.Loading;
using Z_DesignStyle;
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
            if (loadingItems.Count == 0)
            {
                UiManager.instance.CloseUi<UiLoadingCtrl>();
            }
        }

    }
}
