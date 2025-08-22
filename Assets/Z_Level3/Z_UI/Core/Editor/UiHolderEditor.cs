using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Z_String;
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

            //绘制输入框
            var newName = EditorGUILayout.TextField("Name: ", uiHolder.uiName);
            if (newName!= uiHolder.uiName)
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
                if(newPath!= uiHolder.path)
                {
                    Undo.RecordObject(uiHolder, "modify test value");
                    uiHolder.path = newPath;
                    EditorUtility.SetDirty(uiHolder);
                }
                // 绘制按钮
                if (GUILayout.Button("Generate"))
                {
                    GenerateFile();
                }


            }



            // 如果需要，绘制默认的 Inspector
            DrawDefaultInspector();
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
                    uiHolder.subUiHolderLst.Add(subHolder);
                    subHolder.parent = uiHolder;

                    uiHolder.elementTrsLst.Add(o);
                    var subEditor = (UiHolderEditor)CreateEditor(subHolder);
                    switch (subHolder.uiType)
                    {
                        case UiType.Panel:
                            bindContent += $@"
            view.page_{subHolder.uiName} = new {subHolder.uiName}.Ui{subHolder.uiName}Ctrl();
            view.page_{subHolder.uiName}.BindHolderRecursively(uiHolder.subUiHolderLst[{uiHolder.subUiHolderLst.Count - 1}]);";

                            declareContent += $@"
            public {subHolder.uiName}.Ui{subHolder.uiName}Ctrl page_{subHolder.uiName};";
                            initContent += $@"
            page_{subHolder.uiName} = ({subHolder.uiName}.Ui{subHolder.uiName}Ctrl) uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<UiHolder>().ctrl;";
                            subContent += subEditor.GetCode(uiHolder.uiName);
                            break;
                        case UiType.Model:
                            namespaceContent += $"using Ui.{subHolder.uiName};\n";
                            bindContent += $@"
            view.model_{subHolder.uiName} = new Ui{subHolder.uiName}Ctrl();
            view.model_{subHolder.uiName}.BindHolderRecursively(uiHolder.subUiHolderLst[{uiHolder.subUiHolderLst.Count - 1}]);";

                            declareContent += $@"
            public Ui{subHolder.uiName}Ctrl model_{subHolder.uiName};";
                            initContent += $@"
            model_{subHolder.uiName} = (Ui{subHolder.uiName}Ctrl) uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<UiHolder>().ctrl;";
                            break;

                        case UiType.Sub:
                            bindContent += $@"
            view.sub_{subHolder.uiName} = new Ui{subHolder.uiName}Ctrl();
            view.sub_{subHolder.uiName}.BindHolderRecursively(uiHolder.subUiHolderLst[{uiHolder.subUiHolderLst.Count - 1}]);";

                            declareContent += $@"
            public Ui{subHolder.uiName}Ctrl sub_{subHolder.uiName};";
                            initContent += $@"
            sub_{subHolder.uiName} = (Ui{subHolder.uiName}Ctrl) uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<UiHolder>().ctrl;";

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
                        case "scr":
                            declareContent += $@"
            public ScrView scr_{realName};";
                            initContent += $@"
            scr_{realName} = uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<ScrView>();";
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
                        default:
                            Debug.LogError(uiHolder.uiName + " can't analysis type " + tp);
                            break;
                    }
                }


            }

        }

        public void GenerateFile()
        {
            var fullPath = Application.dataPath + uiHolder.path + "/Ui" + uiHolder.uiName + "Base.cs";
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            File.WriteAllText(fullPath, GetCode());
            Debug.Log(fullPath + " Generate Success!");
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
            return $@"
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
        }




    }


}
