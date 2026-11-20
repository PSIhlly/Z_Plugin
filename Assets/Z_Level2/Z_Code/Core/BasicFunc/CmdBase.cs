using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using UnityEngine;
using Z_Code.Form;
namespace Z_Code
{

    public abstract class CmdBase
    {
#if ENABLE_PROFILER
        private static readonly Dictionary<string, Unity.Profiling.ProfilerMarker> profilerMarkers =
            new Dictionary<string, Unity.Profiling.ProfilerMarker>(StringComparer.Ordinal);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetProfilerMarkers() => profilerMarkers.Clear();
        internal Unity.Profiling.ProfilerMarker GetProfilerMarker()
        {
            string name = GetName();
            if (!profilerMarkers.TryGetValue(name, out var marker))
            {
                marker = new Unity.Profiling.ProfilerMarker("ZCode.Cmd " + name);
                profilerMarkers.Add(name, marker);
            }
            return marker;
        }
#endif
        public static bool TryGetForm(string name, out CmdDataForm.Data form)
        {
            form = null;
            if (string.IsNullOrEmpty(name)) return false;
            if (CmdDataForm.DataByName.TryGetValue(name, out form)) return true;
            return BaseData.cmdDic.TryGetValue(name, out var command)
                && CmdDataForm.DataByName.TryGetValue(command.GetName(), out form);
        }

        protected static void Register(CmdBase cmd)
        {
            BaseData.cmdDic[cmd.GetName()] = cmd;
        }
        protected static void RegisterAlias(string alias, CmdBase cmd)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("命令别名不能为空", nameof(alias));
            }
            if (cmd == null)
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            BaseData.cmdDic[alias] = cmd;
        }
        public virtual void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            act?.Invoke(CmdDataForm.DataByName[GetName()].defaultCode);
        }

        public abstract string GetName();

        public abstract CmdBase GetNew();
        protected abstract bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask);
        internal static BoxDataForm.Data ResolveParameter(BoxDataForm.Data parameter, int index,
            Dictionary<string, BoxDataForm.Data> heap)
        {
            if (parameter == null)
                throw new InvalidOperationException($"第 {index + 1} 个命令参数为空");
            if (string.IsNullOrEmpty(parameter.valName)) return parameter;
            if (!heap.TryGetValue(parameter.valName, out var value) || value == null)
                throw new KeyNotFoundException($"找不到命令参数变量 {parameter.valName}");
            return value;
        }

        public void Execute(BoxDataForm.Data[] prm, Dictionary<string, BoxDataForm.Data> heap, InterpretAsyncTask asyncTask)
        {
#if ENABLE_PROFILER
            Unity.Profiling.ProfilerMarker.AutoScope profile = default;
            bool hasProfile = false;
#endif
            try
            {
                asyncTask.Run();
#if ENABLE_PROFILER
                profile = GetProfilerMarker().Auto();
                hasProfile = true;
#endif
                if (prm == null)
                {
                    throw new ArgumentNullException(nameof(prm));
                }
                if (heap == null)
                {
                    throw new ArgumentNullException(nameof(heap));
                }

                for (int i = 0; i < prm.Length; i++)
                {
                    prm[i] = ResolveParameter(prm[i], i, heap);
                }

                Array.Reverse(prm);
                if (ExecuteInternal(prm, asyncTask))
                {
                    asyncTask.Complete();
                }
            }
            catch (Exception e)
            {
                asyncTask.error = GetName() + " 执行失败: " + e.Message;
                asyncTask.Complete();
            }
            finally
            {
#if ENABLE_PROFILER
                if (hasProfile) profile.Dispose();
#endif
            }
        }
    }

    // Stateless synchronous queries borrow their inputs only during Query.
    // Return an independently owned Box, including any dictionary children.
    public abstract class SynchronousQueryCmd : CmdBase
    {
        public sealed override CmdBase GetNew() => this;
        protected abstract BoxDataForm.Data Query(ReadOnlySpan<BoxDataForm.Data> parameters);

        protected sealed override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new[] { Query(prm) };
            return true;
        }

        internal BoxDataForm.Data ExecuteQuery(Span<BoxDataForm.Data> parameters, Dictionary<string, BoxDataForm.Data> heap)
        {
#if ENABLE_PROFILER
            using var scope = GetProfilerMarker().Auto();
#endif
            for (int i = parameters.Length - 1; i >= 0; i--)
                parameters[i] = ResolveParameter(parameters[i], parameters.Length - i - 1, heap);
            return Query(parameters);
        }
    }
}
