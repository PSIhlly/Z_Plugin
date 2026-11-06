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
        private sealed class Work
        {
            internal long total, completed;
            internal int phaseVersion;
            internal readonly Dictionary<string, (long completed, long total)> details = new Dictionary<string, (long, long)>();
            internal double partial;
            internal string titleKey;
            internal long titleOrder;
        }
        private readonly object progressLock = new object();
        private readonly Dictionary<string, Work> workItems = new Dictionary<string, Work>();
        private long plannedWork, finishedWork;
        private long titleOrder;
        public int sessionId { get; private set; }
        public bool hasProgress { get { lock (progressLock) return plannedWork > 0; } }
        public string currentPhase
        {
            get
            {
                lock (progressLock)
                {
                    Work current = null;
                    foreach (var item in loadingItems)
                    {
                        var work = workItems[item];
                        if ((work.total > 0 && work.completed >= work.total) || string.IsNullOrEmpty(work.titleKey)) continue;
                        if (current == null || work.titleOrder > current.titleOrder) current = work;
                    }
                    return current?.titleKey ?? "loading";
                }
            }
        }
        public override void Init()
        {
            loadingItems = new HashSet<string>();
            workItems.Clear();
            plannedWork = finishedWork = 0;
        }
        public void AddLoadItem(string loadItem, long totalWork = 0)
        {
            if (totalWork < 0) throw new ArgumentOutOfRangeException(nameof(totalWork));
            lock (progressLock)
            {
                if (loadingItems.Count == 0)
                {
                    sessionId++;
                    plannedWork = finishedWork = 0;
                    workItems.Clear();
                }
                if (loadingItems.Add(loadItem))
                {
                    workItems[loadItem] = new Work { total = totalWork };
                    plannedWork += totalWork;
                }
            }
            UiManager.instance.ShowUi<UiLoadingCtrl>();
        }
        public void RemoveLoadItem(string loadItem)
        {
            bool close;
            lock (progressLock)
            {
                if (!loadingItems.Remove(loadItem)) return;
                // Completed work stays in this session's denominator/numerator.
                // Removing a child must not reset the parent to zero.
                close = loadingItems.Count == 0;
            }
            if (close) UiManager.instance.CloseUi<UiLoadingCtrl>();
        }

        /// <summary>Declare newly discovered work before executing it; never add a percentage.</summary>
        public void AddWork(string loadItem, long count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            lock (progressLock)
            {
                if (!loadingItems.Contains(loadItem)) return;
                workItems[loadItem].total += count;
                plannedWork += count;
            }
        }

        public void SetPhase(string loadItem, string phase, string titleKey = null)
        {
            lock (progressLock)
            {
                if (!loadingItems.Contains(loadItem)) return;
                var work = workItems[loadItem];
                work.titleKey = titleKey ?? phase;
                work.titleOrder = ++titleOrder;
                work.phaseVersion++;
                work.details.Clear();
                work.partial = 0;
            }
        }

        /// <summary>Report completed units, including from workers. UI remains on Unity's thread.</summary>
        public void Advance(string loadItem, long count = 1)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            lock (progressLock)
            {
                if (!loadingItems.Contains(loadItem)) return;
                var work = workItems[loadItem];
                long accepted = Math.Min(count, work.total - work.completed);
                work.completed += accepted;
                finishedWork += accepted;
            }
        }

        public void RunStep(string loadItem, string phase, Action action)
        {
            SetPhase(loadItem, phase);
            action();
            Advance(loadItem);
        }

        public async Task<T> RunStepAsync<T>(string loadItem, string phase, Func<Task<T>> action)
        {
            SetPhase(loadItem, phase);
            T result = await action();
            Advance(loadItem);
            return result;
        }

        // Each detailed operation fills one declared work unit. Declare its
        // sub-stages first, then report real rows/bytes/files within each stage.
        public async Task<T> RunStepAsync<T>(string loadItem, string phase,
            Func<Action<string, long, long>, Task<T>> action)
        {
            SetPhase(loadItem, phase);
            int version;
            Work work;
            lock (progressLock)
            {
                if (!workItems.TryGetValue(loadItem, out work)) throw new InvalidOperationException("Inactive loading item: " + loadItem);
                version = work.phaseVersion;
            }
            void Report(string detail, long completed, long total)
            {
                if (completed < 0 || total < completed) throw new ArgumentOutOfRangeException(nameof(completed));
                lock (progressLock)
                {
                    if (!loadingItems.Contains(loadItem) || !ReferenceEquals(workItems[loadItem], work)
                        || work.phaseVersion != version) return;
                    work.details[detail] = (completed, total);
                    // 0/0 only declares future work; it must not become the
                    // visible title before that substage actually starts.
                    if (total > 0)
                    {
                        work.titleKey = detail;
                        work.titleOrder = ++titleOrder;
                    }
                    double sum = 0;
                    foreach (var value in work.details.Values)
                        if (value.total > 0) sum += (double)value.completed / value.total;
                    work.partial = sum / work.details.Count;
                }
            }
            try
            {
                T result = await action(Report);
                lock (progressLock)
                {
                    if (!loadingItems.Contains(loadItem) || !ReferenceEquals(workItems[loadItem], work)
                        || work.phaseVersion != version) return result;
                    work.partial = 0;
                    work.phaseVersion++;
                    Advance(loadItem);
                }
                return result;
            }
            catch
            {
                lock (progressLock)
                    if (work.phaseVersion == version) { work.partial = 0; work.phaseVersion++; }
                throw;
            }
        }

        public async Task RunStepsAsync(string loadItem, string phase, IEnumerable<Action> actions)
        {
            await RunStepAsync(loadItem, phase, async report =>
            {
                var steps = new List<Action>(actions);
                report(phase, 0, steps.Count);
                await WaitForRenderAsync();
                var budget = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < steps.Count; i++)
                {
                    steps[i]();
                    report(phase, i + 1, steps.Count);
                    if (i + 1 < steps.Count && budget.ElapsedMilliseconds >= 8)
                    {
                        await WaitForRenderAsync();
                        budget.Restart();
                    }
                }
                return true;
            });
        }

        private double CompletedWork()
        {
            double completed = finishedWork;
            foreach (var item in loadingItems)
            {
                var work = workItems[item];
                if (work.completed < work.total) completed += work.partial;
            }
            return completed;
        }

        /// <summary>Actual completed/declared work, without averaging visibility guards.</summary>
        public float progress
        {
            get
            {
                lock (progressLock)
                    return loadingItems.Count == 0 ? 1f : plannedWork == 0 ? 0f : (float)(CompletedWork() / plannedWork);
            }
        }

        public async Task WaitForRenderAsync(CancellationToken cancellationToken = default)
        {
            var rendered = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => rendered.TrySetCanceled()))
            {
                TimeManager.instance.AddCurFrameEndWithoutCheckAction(() =>
                    TimeManager.instance.AddNextUpdateWithoutCheckAction(() => rendered.TrySetResult(true)));
                await rendered.Task;
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        // Call from the Unity main thread. Each operation owns its item, so a
        // canceled old request cannot close a newer request's loading screen.
        public async Task RunAsync(string name, Func<Task> action, CancellationToken cancellationToken = default)
        {
            var loadItem = name + ":" + Guid.NewGuid().ToString("N");
            AddLoadItem(loadItem);
            try
            {
                SetPhase(loadItem, "Wait for Loading render");
                await WaitForRenderAsync(cancellationToken);
                SetPhase(loadItem, "Run action");
                await action();
            }
            finally
            {
                RemoveLoadItem(loadItem);
            }
        }

    }
}
