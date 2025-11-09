using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Z_DesignStyle
{
    public class MultiTaskWorker : Z_MonoSingleton<MultiTaskWorker>
    {
        private string lockKey = "task";
        public Action onLast;
        public void Add(Action act)
        {
            lock (lockKey)
            {
                onLast += act;
            }
        }
        private void LateUpdate()
        {
            lock (lockKey)
            {
                onLast?.Invoke();
            }
        }
    }

    public class Z_MultiTask<Res>
    {
        Action<Res> onComplete;
        string lockKey = "task";
        Task task;
        public void Run(Res cur, Func<Res> taskRun, Action<Res> taskComplete)
        {

            lock (lockKey)
            {
                onComplete += taskComplete;
                if (cur == null)
                {
                    if (task == null)
                    {
                        task = Task.Run(() =>
                        {
                            var res = taskRun();
                            lock (lockKey)
                            {
                                MultiTaskWorker.instance.Add(() =>
                                {
                                    onComplete?.Invoke(res);
                                    onComplete -= onComplete;
                                });
                            }
                        });
                    }
                }
                else
                {
                    onComplete?.Invoke(cur);
                    onComplete -= onComplete;
                }
            }
        }

    }

}
