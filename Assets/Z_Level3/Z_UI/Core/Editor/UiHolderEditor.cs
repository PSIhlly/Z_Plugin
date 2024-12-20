using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Z_Ui.Base;

namespace Z_Ui_Editor
{
    [CustomEditor(typeof(UiHolder))]
    public class UiHolderEditor : Editor
    {


        public bool binded;
        UiHolder uiHolder=> (UiHolder)target;
        public override void OnInspectorGUI()
        {

            //绘制输入框
            uiHolder.uiName = EditorGUILayout.TextField("Name: ", uiHolder.uiName);

            if (uiHolder.uiType == UiType.Panel)
            {
                uiHolder.path = EditorGUILayout.TextField("Path: ", uiHolder.path);
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


        public void RefreshPanelElementContent()
        {
            uiHolder.elementTrsLst.Clear();
            uiHolder.subUiHolderLst.Clear();

            declareContent = "";
            initContent = "";
            bindContent = "";

            Queue<Transform> queue = new Queue<Transform>();

            queue.Enqueue(uiHolder.gameObject.transform);
            while(queue.Count>0)
            {
                var o = queue.Dequeue();
                AddBasicElement(o);
                if (o!= uiHolder.gameObject.transform&&o.TryGetComponent<UiHolder>(out var subHolder))
                {
                    uiHolder.subUiHolderLst.Add(subHolder);
                    subHolder.parent = uiHolder;

                    uiHolder.elementTrsLst.Add(o);
                    var subEditor = (UiHolderEditor)CreateEditor(subHolder);
                    switch (subHolder.uiType)
                    {
                        case UiType.Panel:
                            bindContent += $@"
            view.page_{subHolder.uiName} = new Ui{subHolder.uiName}Ctrl();
            view.page_{subHolder.uiName}.BindHolderRecursively(uiHolder.subUiHolderLst[{uiHolder.subUiHolderLst.Count - 1}]);";

                            declareContent += $@"
            public Ui{subHolder.uiName}Ctrl page_{subHolder.uiName};";
                            initContent += $@"
            page_{subHolder.uiName} = (Ui{subHolder.uiName}Ctrl) uiHolder.elementTrsLst[{uiHolder.elementTrsLst.Count - 1}].GetComponent<UiHolder>().ctrl;";

                            subEditor.GenerateFile();
                            break;
                        case UiType.Model:
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
                }else
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
                        default:
                            Debug.LogError("can't analysis type " + tp);
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

        string GetCode(string parentClass="")
        {
            RefreshPanelElementContent();
            switch (uiHolder.uiType)
            {
                case UiType.Panel:
                case UiType.Model:
                    return $@"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
{namespaceContent}
namespace Ui.{uiHolder.uiName}
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
        {(string.IsNullOrEmpty(parent)?"":$"public Ui{parent}Ctrl parent=>(Ui{parent}Ctrl)uiHolder.parent.ctrl;")}

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
