using UnityEngine;
using System.Collections;
using System.IO;
using System.Reflection;
using System;
using System.Linq;
namespace Z_IlRuntime.Core
{
    public class IlRuntimeManager : MonoBehaviour
    {
        public dynamic appdomain;

        public static void Init()
        {
            var go = new GameObject("IlRuntimeManager");
            DontDestroyOnLoad(go);
            go.AddComponent<IlRuntimeManager>().StartLoad();

        }

        public void StartLoad()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            Assembly targetAssembly = assemblies.FirstOrDefault(a => a.GetName().FullName.Contains("ILRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
            if (targetAssembly == null)
            {
                Debug.LogError("Can't find ilr asm");
                return;
            }

            var type = targetAssembly.GetType("ILRuntime.Runtime.Enviorment.AppDomain");
            if (type == null)
            {
                Debug.LogError("Can't find ilr type");
                return;
            }
            ConstructorInfo ctorWithParam = type.GetConstructor(new Type[] { typeof(int) });
            appdomain = ctorWithParam.Invoke(new object[] { 0 });
            StartCoroutine(LoadHotFixAssembly());
        }

        IEnumerator LoadHotFixAssembly()
        {
            var dllTask = File.ReadAllBytesAsync(Application.streamingAssetsPath + "/HotFix_Project.dll");
            var pdbTask = File.ReadAllBytesAsync(Application.streamingAssetsPath + "/HotFix_Project.pdb");
            while (!dllTask.IsCompleted || !pdbTask.IsCompleted)
            {
                yield return null;
            }

            try
            {
                var dllBytes = new MemoryStream(dllTask.Result);

                var pdbBytes = new MemoryStream(pdbTask.Result);

                appdomain.LoadAssembly(dllBytes, pdbBytes, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
            }
            catch (Exception e)
            {
                Debug.LogError("加载热更DLL失败:" + e);
            }

            InitializeILRuntime();
            OnHotFixLoaded();
        }

        void InitializeILRuntime()
        {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
            appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
            //这里做一些ILRuntime的注册，HelloWorld示例暂时没有需要注册的
        }

        void OnHotFixLoaded()
        {
            //HelloWorld，第一次方法调用
            appdomain.Invoke("HotFix_Project.InstanceClass", "StaticFunTest", null, null);

        }

        void Update()
        {

        }
    }


}
