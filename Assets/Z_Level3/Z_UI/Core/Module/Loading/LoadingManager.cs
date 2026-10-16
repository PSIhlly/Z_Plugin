using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ui.Loading;
using Z_DesignStyle;
using Z_Time;
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

        // Call from the Unity main thread. Each operation owns its item, so a
        // canceled old request cannot close a newer request's loading screen.
        public async Task RunAsync(string name, Func<Task> action, CancellationToken cancellationToken = default)
        {
            var loadItem = name + ":" + Guid.NewGuid().ToString("N");
            AddLoadItem(loadItem);
            try
            {
                var rendered = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(() => rendered.TrySetCanceled()))
                {
                    TimeManager.instance.AddCurFrameEndWithoutCheckAction(() =>
                        TimeManager.instance.AddNextUpdateWithoutCheckAction(() => rendered.TrySetResult(true)));
                    await rendered.Task;
                }
                cancellationToken.ThrowIfCancellationRequested();
                await action();
            }
            finally
            {
                RemoveLoadItem(loadItem);
            }
        }

    }
}
