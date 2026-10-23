using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ui.Loading;
using UnityEngine;
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
        readonly Dictionary<string, float> itemProgress = new Dictionary<string, float>();
        public override void Init()
        {
            loadingItems = new HashSet<string>();
        }
        public void AddLoadItem(string loadItem)
        {
            loadingItems.Add(loadItem);
            itemProgress[loadItem] = 0f;
            UiManager.instance.ShowUi<UiLoadingCtrl>();
        }
        public void RemoveLoadItem(string loadItem)
        {
            loadingItems.Remove(loadItem);
            itemProgress.Remove(loadItem);
            if (loadingItems.Count == 0)
            {
                UiManager.instance.CloseUi<UiLoadingCtrl>();
            }
        }

        /// <summary>
        /// 报告某个加载项的进度(0~1)。加载界面显示所有加载项的平均值；
        /// 没有报告进度的项按 0 计（例如正等待一个异步任务完成）。
        /// </summary>
        public void SetProgress(string loadItem, float progress)
        {
            if (!loadingItems.Contains(loadItem))
                return;
            itemProgress[loadItem] = Mathf.Clamp01(progress);
        }

        /// <summary>所有加载项的总进度(0~1)；没有加载项时为 1。</summary>
        public float progress
        {
            get
            {
                if (loadingItems.Count == 0)
                    return 1f;
                float sum = 0f;
                foreach (var item in loadingItems)
                    sum += itemProgress.TryGetValue(item, out float value) ? value : 0f;
                return Mathf.Clamp01(sum / loadingItems.Count);
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
                SetProgress(loadItem, 1f);
                RemoveLoadItem(loadItem);
            }
        }

    }
}
