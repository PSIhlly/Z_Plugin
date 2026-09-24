using Form;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Ui.Base;
namespace Ui.ModStoryEventEditWindow
{
    public partial class UiUnitParam
    {
        public Transform parent;
        public UiContainer<UiUnitCtrl> con;
        public SyntaxNode node;
        public string txt;
    }
    public partial class UiUnitModel
    {
        public Transform parent;
        public UiContainer<UiUnitCtrl> con;
        public SyntaxNode node;
        public string txt;
    }
    public partial class UiUnitCtrl
    {


        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelUnit(model.node, parent.model.prm.node);
            });
        }
        public override void OnEnable()
        {
            if (param == null)
            {
                gameObject.SetActive(false);
                return;
            }
            model.parent = param.parent;
            model.node = param.node;
            model.txt = param.txt;
            model.con = param.con;
            parent.RegisterRenderedUnit(this);
            Refresh();
        }


        public void Refresh()
        {
            RefreshSelection();
            gameObject.transform.parent = model.parent;
            if (model.node == null)
            {
                view.txt_.text = model.txt;
                view.btn_.gameObject.SetActive(false);
                view.go_image.SetActive(false);
                view.txt_.gameObject.SetActive(true);
            }
            else
            {
                view.txt_.text = "";
                view.btn_.gameObject.SetActive(true);

                if (SyntaxAnalysis.IsEmptyArgumentNode(model.node))
                {
                    view.txt_.text = string.Empty;
                }
                else switch (model.node.desc.type)
                {
                    case CodeType.FuncName:
                    case CodeType.Reserved:
                    case CodeType.Operator:
                        if (CmdDataForm.DataByName.ContainsKey(model.node.desc.code))
                        {
                            var form = CmdDataForm.DataByName[model.node.desc.code];
                            string cur = "";

                            foreach (var ch in form.desc)
                            {
                                if (ch == '}')
                                {
                                    int id = int.Parse(cur);
                                    if (id < model.node.subNodes.Count)
                                        CreateNode(model.node.subNodes[id]);
                                    cur = "";
                                }
                                else if (ch == '{')
                                {
                                    CreateTxt(cur);
                                    cur = "";
                                }
                                else
                                {
                                    cur += ch;
                                }
                            }
                            if (cur != "")
                            {
                                CreateTxt(cur);
                            }
                        }
                        else if (int.TryParse(model.node.desc.code, out int evtId) && EventProgramDataForm.DataByUid.ContainsKey(evtId))
                        {
                            var evtData = EventProgramDataForm.DataByUid[evtId];
                            CreateTxt(evtData.name + "(");
                            bool hasVisibleArgument = false;
                            for (int i = 0; i < model.node.subNodes.Count; i++)
                            {
                                var argument = model.node.subNodes[i];
                                if (!IsVisibleArgument(argument))
                                    continue;
                                CreateTxt($"{(hasVisibleArgument ? "," : "")}param{i + 1}=");
                                CreateNode(argument);
                                hasVisibleArgument = true;
                            }
                            CreateTxt(")");
                        }
                        else
                        {
                            view.txt_.text = model.node.desc.code;
                        }
                        break;
                    default:
                        view.txt_.text = model.node.desc.code;
                        break;
                }
                var isImage = AssetManager.instance.texCtrl.IsAsset(model.node.desc.code);
                view.go_image.SetActive(isImage);
                view.txt_.gameObject.SetActive(!isImage);

                if (isImage)
                {
                    view.img_.BindTexData(TexAssetForm.DataById.GetDk(AssetManager.instance.texCtrl.GetId(model.node.desc.code), GlobalDefaultHelper.DefaultTexId));
                }
            }

        }

        public void RefreshSelection()
        {
            view.sta_.ChangeState(parent.parent.model.selUnit != null && parent.parent.model.selUnit == model.node ? 1 : 0);
        }

        private void CreateTxt(string desc)
        {
            if (string.IsNullOrEmpty(desc))
                return;
            model.con.Add(new UiUnitParam()
            {
                con = model.con,
                parent = view.rtf_root.transform,
                txt = desc,
                node = null
            });
        }
        private void CreateNode(SyntaxNode node)
        {
            if (!IsVisibleArgument(node))
                return;
            model.con.Add(new UiUnitParam()
            {
                con = model.con,
                parent = view.rtf_root.transform,
                node = node
            });
        }

        private static bool IsVisibleArgument(SyntaxNode node)
        {
            return node != null && !SyntaxAnalysis.IsEmptyArgumentNode(node) &&
                   (node.desc.type != CodeType.Str || !string.IsNullOrEmpty(node.desc.code));
        }
    }
}
