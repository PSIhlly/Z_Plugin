using System;
using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Z_String;
using Z_Ui;
using Z_Ui.Base;

namespace Z_Ui_Editor
{
    [CustomEditor(typeof(UiHolder))]
    public class UiHolderEditor : Editor
    {

        public string GetQuickCode()
        {
            string modelCode = "";
            string showCode = "";
            string paramCode = "";

            string declareCode = "";
            string initCode = "";
            string refreshCode = "";

            string subCode = "";

            if (uiHolder.subUiHolderLst.Find(x => x.uiType == UiType.Panel) != null)
            {
                modelCode += $@"
            public int selPage;";
                paramCode += $@"
            public int selPage;";
                showCode += $@"
            if(param!=null)
                model.selPage=param.selPage;";
            }

            HashSet<Transform> exist = new HashSet<Transform>();

            foreach (var o in uiHolder.elementTrsLst)
            {
                if (exist.Contains(o))
                    continue;
                exist.Add(o);
                if (o.TryGetComponent<UiHolder>(out var subHolder))
                {
                    if (subHolder != uiHolder)
                    {
                        if (subHolder.uiType == UiType.Sub)
                        {
                            string scrCode = "view." + (o.transform.GetComponentInParent<ScrView>() == null ? "" : o.transform.GetComponentInParent<ScrView>().transform.name);

                            declareCode += $@"
            UiScrViewContainer<Ui{subHolder.uiName}Ctrl> {subHolder.uiName.FirstToLower()}Con;";
                            initCode += $@"
            {subHolder.uiName.FirstToLower()}Con = new UiScrViewContainer<Ui{subHolder.uiName}Ctrl>(view.go_{o.gameObject.name.Split("_")[1]},{scrCode});";
                            refreshCode += $@"
            {subHolder.uiName.FirstToLower()}Con.Clear();
            for(int i=0,icnt= ;i<icnt;i++)
            {{
                {subHolder.uiName.FirstToLower()}Con.Add(new Ui{subHolder.uiName}Param()
                {{
                    
                }});
            }}
            {subHolder.uiName.FirstToLower()}Con.Refresh();";
                            if (subHolder != uiHolder)
                                subCode += ((UiHolderEditor)CreateEditor(subHolder)).GetQuickCode();
                        }
                        else if (subHolder.uiType == UiType.Panel)
                        {
                            refreshCode += $@"
            view.page_{subHolder.uiName}.SetActive(model.selPage == 0);";
                        }
                    }
                }
                else if (o.name.Split("_")[0].Split("|").Contains("btn"))
                {
                    initCode += $@"
            view.btn_{o.name.Split("_")[1]}.onClick.AddListener(() =>
            {{

            }});";
                }
                else if (o.name.Split("_")[0].Split("|").Contains("sta"))
                {
                    refreshCode += $@"
            view.sta_{o.name.Split("_")[1]}.ChangeState(0);";
                }
                else if (o.name.Split("_")[0].Split("|").Contains("img"))
                {
                    refreshCode += $@"
            view.img_{o.name.Split("_")[1]}.sprite=TextureHelper.transparentSprite;";
                }
                else if (o.name.Split("_")[0].Split("|").Contains("rimg"))
                {
                    refreshCode += $@"
            view.rimg_{o.name.Split("_")[1]}.sprite=TextureHelper.transparentSprite;";
                }
                else if (o.name.Split("_")[0].Split("|").Contains("mp"))
                {
                    initCode += $@"
            view.mp_{o.name.Split("_")[1]}.OpenMedia(new MediaPath(Path.Combine(Application.persistentDataPath, "".mp4""), MediaPathType.AbsolutePathOrURL), true); ";
                }
                else if (o.name.Split("_")[0].Split("|").Contains("as"))
                {
                    initCode += $@"
            view.as_{o.name.Split("_")[1]}.Play("");";
                }
                else if (o.name.Split("_")[0].Split("|").Contains("txt"))
                {
                    refreshCode += $@"
            view.txt_{o.name.Split("_")[1]}.text="""" ;";
                }
                else if (o.name.Split("_")[0].Split("|").Contains("ipt"))
                {
                    initCode += $@"
            view.ipt_{o.name.Split("_")[1]}.onFinishInput+=(s)=>
            {{

            }};";
                    refreshCode += $@"
            view.ipt_{o.name.Split("_")[1]}.Set("""") ;";
                }
                else if (o.name.Split("_")[0].Split("|").Contains("sld"))
                {

                    refreshCode += $@"
            view.sld{o.name.Split("_")[1]}.value = 0;";
                }
                else if (o.name.Split("_")[0].Split("|").Contains("dp"))
                {
                    initCode += $@"
            view.dp_{o.name.Split("_")[1]}.onFinishSelect=(v) => 
            {{

            }};";
                    refreshCode += $@"
            view.dp_{o.name.Split("_")[1]}.ClearOptions();
            for(int i=0,icnt= ;i<icnt;i++)
            {{
                view.dp_{o.name.Split("_")[1]}.options.Add(new TMPro.TMP_Dropdown.OptionData( ));
            }}";
                }
            }
            var tmp = uiHolder;
            string namespaceStr = uiHolder.uiName;
            while (tmp.parent != null && uiHolder.uiType == UiType.Panel)
            {
                tmp = tmp.parent;
                namespaceStr = tmp.uiName + "." + namespaceStr;
            }
            namespaceStr = "namespace Ui." + namespaceStr;

            return $@"{(uiHolder.uiType == UiType.Sub ? "" : $@"using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

{namespaceContent}
{namespaceStr}
{{
")}
    public partial class Ui{uiHolder.uiName}Param
    {{
{paramCode}
    }}
    public partial class Ui{uiHolder.uiName}Model
    {{
{modelCode}
    }}
    public partial class Ui{uiHolder.uiName}Ctrl
    {{
{declareCode}
        public override void OnCreate()
        {{
{initCode}

        }}
        public override void OnShow()
        {{
{showCode}
            Refresh();
        }}
        public void Refresh()
        {{
{refreshCode}            
        }}
    }}
{subCode}
{(uiHolder.uiType == UiType.Sub ? "" : "}")}";
        }
        public bool binded;
        UiHolder uiHolder => (UiHolder)target;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //绘制输入框
            var newName = EditorGUILayout.TextField("Name: ", uiHolder.uiName);
            if (newName != uiHolder.uiName)
            {
                Undo.RecordObject(uiHolder, "modify test value");
                uiHolder.uiName = newName;
                EditorUtility.SetDirty(uiHolder);
            }
            if (GUILayout.Button("CopyQuickCode"))
            {
                GUIUtility.systemCopyBuffer = GetQuickCode();
            }
            if (uiHolder.uiType != UiType.Sub && uiHolder.GetComponentsInParent<UiHolder>(true).Length == 1)
            {
                var newPath = EditorGUILayout.TextField("Path: ", uiHolder.path);
                if (newPath != uiHolder.path)
                {
                    Undo.RecordObject(uiHolder, "modify test value");
                    uiHolder.path = newPath;
                    EditorUtility.SetDirty(uiHolder);
                }
                // 绘制按钮
                if (uiHolder.uiType == UiType.Panel)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("preloadConfig"), new GUIContent("Preload Config"));
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("Generate"))
                {
                    GenerateFile();
                }


            }



            // 如果需要，绘制默认的 Inspector
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "preloadConfig");
            serializedObject.ApplyModifiedProperties();
        }
        //中间参数
        string declareContent = "";
        string initContent = "";

        string bindContent = "";
        string subContent = "";
        string namespaceContent = "";
        string subNamespaceContent = "";


        public void RefreshPanelElementContent()
        {
            uiHolder.elementTrsLst.Clear();
            uiHolder.subUiHolderLst.Clear();

            declareContent = "";
            initContent = "";
            bindContent = "";
            subContent = "";

            Queue<Transform> queue = new Queue<Transform>();

            queue.Enqueue(uiHolder.gameObject.transform);
            while (queue.Count > 0)
            {
                var o = queue.Dequeue();
                AddBasicElement(o);
                if (o != uiHolder.gameObject.transform && o.TryGetComponent<UiHolder>(out var subHolder))
                {
                    var tmp = subHolder.name.Split("_");
                    string realName = tmp[tmp.Length - 1];
                    uiHolder.subUiHolderLst.Add(subHolder);
                    subHolder.parent = uiHolder;
                    Debug.Log(subHolder.name + " +" + uiHolder.name);
                    uiHolder.elementTrsLst.Add(o);
                    var subEditor = (UiHolderEditor)CreateEditor(subHolder);
                    switch (subHolder.uiType)
                    {
                        case UiType.Panel:
                            bindContent += $@"
            view.page_{realName} = new {subHolder.uiName}.Ui{subHolder.uiName}Ctrl();
            view.page_{realName}.BindHolderRecursively(uiHolder.subUiHolderLst[{uiHolder.subUiHolderLst.Count - 1}]);";

                            declareContent += $@"
            public {subHolder.uiName}.Ui{subHolder.uiName}Ctrl page_{realName};";
                            initContent += $@"
            page_{realName} = ({subHolder.uiName}.Ui{subHolder.uiName}Ctrl) uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<UiHolder>().ctrl;";
                            subContent += subEditor.GetCode(uiHolder.uiName);
                            break;
                        case UiType.Model:
                            namespaceContent += $"using Ui.{subHolder.uiName};\n";

                            bindContent += $@"
            view.model_{realName} = new Ui{subHolder.uiName}Ctrl();
            view.model_{realName}.BindHolderRecursively(uiHolder.subUiHolderLst[{uiHolder.subUiHolderLst.Count - 1}]);";

                            declareContent += $@"
            public Ui{subHolder.uiName}Ctrl model_{realName};";
                            initContent += $@"
            model_{realName} = (Ui{subHolder.uiName}Ctrl) uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<UiHolder>().ctrl;";
                            break;

                        case UiType.Sub:
                            bindContent += $@"
            view.sub_{realName} = new Ui{subHolder.uiName}Ctrl();
            view.sub_{realName}.BindHolderRecursively(uiHolder.subUiHolderLst[{uiHolder.subUiHolderLst.Count - 1}]);";

                            declareContent += $@"
            public Ui{subHolder.uiName}Ctrl sub_{realName};";
                            initContent += $@"
            sub_{realName} = (Ui{subHolder.uiName}Ctrl) uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<UiHolder>().ctrl;";

                            subContent += subEditor.GetCode(uiHolder.uiName);
                            break;

                        default:
                            Debug.LogError("can't analysis uiType " + subHolder.uiType);
                            break;
                    }

                }
                else
                {
                    foreach (Transform ch in o)
                    {
                        queue.Enqueue(ch);
                    }
                }

            }

        }



        private void AddBasicElement(Transform o)
        {
            int split = o.name.IndexOf("_");
            if (split != -1)
            {
                string realName = o.name.Substring(split + 1);
                string[] typeDescs = o.name.Substring(0, split).Split("|");

                foreach (var tp in typeDescs)
                {
                    uiHolder.elementTrsLst.Add(o);
                    switch (tp)
                    {
                        case "btn":
                            declareContent += $@"
            public Btn btn_{realName};";
                            initContent += $@"
            btn_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<Btn>();";
                            break;
                        case "mp":
                            declareContent += $@"
            public MediaPlayer mp_{realName};";
                            initContent += $@"
            mp_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<MediaPlayer>();";
                            break;
                        case "txt":
                            declareContent += $@"
            public Txt txt_{realName};";
                            initContent += $@"
            txt_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<Txt>();";
                            break;
                        case "ipt":
                            declareContent += $@"
            public Ipt ipt_{realName};";
                            initContent += $@"
            ipt_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<Ipt>();";
                            break;

                        case "go":
                            declareContent += $@"
            public GameObject go_{realName};";
                            initContent += $@"
            go_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].gameObject;";
                            break;

                        case "img":
                            declareContent += $@"
            public Img img_{realName};";
                            initContent += $@"
            img_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<Img>();";
                            break;
                        case "rimg":
                            declareContent += $@"
            public RImg rimg_{realName};";
                            initContent += $@"
            rimg_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<RImg>();";
                            break;
                        case "as":
                            declareContent += $@"
            public AudioSource as_{realName};";
                            initContent += $@"
            as_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<AudioSource>();";
                            break;
                        case "scr":
                            declareContent += $@"
            public ScrView scr_{realName};";
                            initContent += $@"
            scr_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<ScrView>();";
                            break;
                        case "sld":
                            declareContent += $@"
            public Sld sld_{realName};";
                            initContent += $@"
            sld_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<Sld>();";
                            break;
                        case "sta":
                            declareContent += $@"
            public Sta sta_{realName};";
                            initContent += $@"
            sta_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<Sta>();";
                            break;

                        case "rtf":
                            declareContent += $@"
            public RectTransform rtf_{realName};";
                            initContent += $@"
            rtf_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<RectTransform>();";
                            break;
                        case "dp":
                            declareContent += $@"
            public Dp dp_{realName};";
                            initContent += $@"
            dp_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<Dp>();";
                            break;
                        default:
                            Debug.LogError(uiHolder.uiName + " can't analysis type " + tp);
                            break;
                    }
                }


            }

        }

        public void GenerateFile()
        {
            if (!SaveCurrentPrefabChanges())
                return;

            if (uiHolder.uiType != UiType.Panel)
            {
                var generatedPath = WriteGeneratedFile();
                Debug.Log(generatedPath + $" Generate Success! {uiHolder.uiType} holders are generated as embedded UI and are not added to the preload registry.");
                return;
            }

            var preloadConfig = uiHolder.PreloadConfig;
            if (preloadConfig == null || !EditorUtility.IsPersistent(preloadConfig))
            {
                Debug.LogError($"{uiHolder.name} requires a persistent {nameof(UiPreloadConfig)} reference in the Inspector before Generate.");
                return;
            }

            if (!TryGetPersistentUi(out var persistentUi))
            {
                Debug.LogError($"{uiHolder.name} must be the root of a Prefab asset or instance with a valid Panel UiHolder before Generate. Pure scene objects cannot be registered in {AssetDatabase.GetAssetPath(preloadConfig)}.");
                return;
            }

            if (uiHolder.gameObject != persistentUi)
            {
                var persistentHolder = persistentUi.GetComponent<UiHolder>();
                var sourceEditor = (UiHolderEditor)CreateEditor(persistentHolder);
                try
                {
                    Debug.Log($"Generate uses the saved Prefab source '{AssetDatabase.GetAssetPath(persistentUi)}'.");
                    sourceEditor.GeneratePanelFile(persistentUi, preloadConfig);
                }
                finally
                {
                    DestroyImmediate(sourceEditor);
                }
                return;
            }

            GeneratePanelFile(persistentUi, preloadConfig);
        }

        private bool SaveCurrentPrefabChanges()
        {
            var sourceUi = uiHolder.gameObject;
            var prefabStage = PrefabStageUtility.GetPrefabStage(sourceUi);
            if (prefabStage != null && prefabStage.prefabContentsRoot != null)
            {
                if (!prefabStage.scene.isDirty)
                    return true;

                PrefabUtility.SaveAsPrefabAsset(
                    prefabStage.prefabContentsRoot,
                    prefabStage.assetPath,
                    out var savedSuccessfully);
                if (!savedSuccessfully)
                {
                    Debug.LogError($"Failed to save Prefab '{prefabStage.assetPath}' before Generate.");
                    return false;
                }

                prefabStage.ClearDirtiness();
                AssetDatabase.SaveAssets();
                return true;
            }

            if (EditorUtility.IsPersistent(sourceUi))
            {
                var prefabRoot = sourceUi.transform.root.gameObject;
                var assetPath = AssetDatabase.GetAssetPath(prefabRoot);
                if (!assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    return true;

                PrefabUtility.SavePrefabAsset(prefabRoot, out var savedSuccessfully);
                if (!savedSuccessfully)
                {
                    Debug.LogError($"Failed to save Prefab '{assetPath}' before Generate.");
                    return false;
                }

                AssetDatabase.SaveAssets();
                return true;
            }

            var instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(sourceUi);
            if (instanceRoot == null || !PrefabUtility.HasPrefabInstanceAnyOverrides(instanceRoot, false))
                return true;

            try
            {
                PrefabUtility.ApplyPrefabInstance(instanceRoot, InteractionMode.AutomatedAction);
                AssetDatabase.SaveAssets();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to apply Prefab instance changes before Generate: {exception.Message}");
                return false;
            }
        }

        private void GeneratePanelFile(GameObject persistentUi, UiPreloadConfig preloadConfig)
        {
            var fullPath = WriteGeneratedFile();
            var registryChanged = RegisterPreloadUi(preloadConfig, persistentUi);
            AssetDatabase.SaveAssets();
            var registrationResult = registryChanged ? "Registered" : "Already registered";
            Debug.Log(fullPath + $" Generate Success! {registrationResult}: {persistentUi.name} in {AssetDatabase.GetAssetPath(preloadConfig)}.");
        }

        private string WriteGeneratedFile()
        {
            var fullPath = Application.dataPath + uiHolder.path + "/Ui" + uiHolder.uiName + "Base.cs";
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            uiHolder.parent = null;
            File.WriteAllText(fullPath, GetCode());
            return fullPath;
        }

        private bool TryGetPersistentUi(out GameObject persistentUi)
        {
            var sourceUi = uiHolder.gameObject;
            persistentUi = EditorUtility.IsPersistent(sourceUi) ? sourceUi : null;

            if (persistentUi == null)
            {
                var prefabStage = PrefabStageUtility.GetPrefabStage(sourceUi);
                if (prefabStage != null)
                {
                    persistentUi = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(sourceUi, prefabStage.assetPath);
                    if (persistentUi == null && sourceUi == prefabStage.prefabContentsRoot)
                        persistentUi = AssetDatabase.LoadAssetAtPath<GameObject>(prefabStage.assetPath);
                }
                else
                {
                    persistentUi = PrefabUtility.GetCorrespondingObjectFromSource(sourceUi);
                }
            }

            if (persistentUi == null || !EditorUtility.IsPersistent(persistentUi))
                return false;

            var assetPath = AssetDatabase.GetAssetPath(persistentUi);
            var persistentHolder = persistentUi.GetComponent<UiHolder>();
            return persistentUi.transform.parent == null &&
                   assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) &&
                   persistentHolder != null &&
                   persistentHolder.uiType == UiType.Panel &&
                   !string.IsNullOrWhiteSpace(persistentHolder.uiName);
        }

        private static bool RegisterPreloadUi(UiPreloadConfig preloadConfig, GameObject persistentUi)
        {
            Undo.RecordObject(preloadConfig, "Register UI preload prefab");
            var changed = preloadConfig.Register(persistentUi);
            if (!changed)
                return false;

            EditorUtility.SetDirty(preloadConfig);
            return true;
        }

        string GetCode(string parentClass = "")
        {
            Undo.RecordObject(uiHolder, "modify test value");
            RefreshPanelElementContent();
            EditorUtility.SetDirty(uiHolder);

            var res = "";
            if (string.IsNullOrEmpty(parentClass))
            {

                res = $@"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.{uiHolder.uiName}
";
            }
            else
            {
                res = $@"
namespace {uiHolder.uiName}
";
            }

            switch (uiHolder.uiType)
            {
                case UiType.Panel:
                case UiType.Model:


                    return res + $@"
{{
{GetCoreCode(parentClass)}
}}
";
                case UiType.Sub:
                    return GetCoreCode(parentClass);
            }
            return null;

        }

        string GetCoreCode(string parent)
        {
            int id = subContent.IndexOf("using ");
            while (id != -1)
            {
                int end = subContent.IndexOf("\n", id) + 1;
                namespaceContent += subContent.Substring(id, end - id);
                subContent = subContent.Remove(id, end - id);
                id = subContent.IndexOf("using ");
            }
            var res = $@"
{StringHelper.RemoveMultiLine(namespaceContent)}
{subContent}
    public partial class Ui{uiHolder.uiName}Param:UiParam
    {{
    }}

    public partial class Ui{uiHolder.uiName}View:UiView
    {{
{declareContent}
        public Ui{uiHolder.uiName}View(UiHolder uiHolder):base(uiHolder)
        {{
{initContent}
        }}

    }}
    public partial class Ui{uiHolder.uiName}Ctrl:UiCtrl
    {{
        public Ui{uiHolder.uiName}View view;
        public Ui{uiHolder.uiName}Model model;
        public Ui{uiHolder.uiName}Param param;
        {(string.IsNullOrEmpty(parent) ? "" : $"public Ui{parent}Ctrl parent=>(Ui{parent}Ctrl)uiHolder.parent.ctrl;")}

        public override void SetParam(UiParam param)
        {{
            this.param = (Ui{uiHolder.uiName}Param)param;
        }}

        public override void BindHolderRecursively(UiHolder uiHolder)
        {{

            base.BindHolderRecursively(uiHolder);

            view = new Ui{uiHolder.uiName}View(uiHolder);
            model=new Ui{uiHolder.uiName}Model();

{bindContent}
        }}

    }}
    public partial class Ui{uiHolder.uiName}Model:UiModel
    {{
        
    }}";

            return res;
        }




    }


}
