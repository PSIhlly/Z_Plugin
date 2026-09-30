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
                    case CodeType.Operator:
                        RenderOperator();
                        break;
                    case CodeType.FuncName:
                    case CodeType.Reserved:
                        if (CmdDataForm.DataByName.ContainsKey(model.node.desc.code))
                        {
                            var form = CmdDataForm.DataByName[model.node.desc.code];
                            RenderDescription(form.desc);
                        }
                        else if (ProgramDataForm.DataByName.TryGetValue(model.node.desc.code, out var program)
                                 && program is EventProgramDataForm.Data evtData)
                        {
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
                    case CodeType.Str:
                        view.txt_.text = string.IsNullOrEmpty(model.node.desc.code) ? "\"\"" : model.node.desc.code;
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

        private void RenderOperator()
        {
            var node = model.node;
            if (node.subNodes.Count == 1)
            {
                // Unary +/- must not use the table's binary description.
                if (node.desc.code != "++")
                    CreateTxt(node.desc.code);
                CreateNode(node.subNodes[0]);
                if (node.desc.code == "++")
                    CreateTxt("++");
            }
            else if (node.subNodes.Count == 2)
            {
                if (CmdDataForm.DataByName.TryGetValue(node.desc.code, out var form)
                    && !string.IsNullOrEmpty(form.desc))
                    RenderDescription(form.desc);
                else
                {
                    // Binary AST children are stored right, left. Missing command
                    // metadata must never hide either editable operand.
                    CreateNode(node.subNodes[1]);
                    CreateTxt(" " + node.desc.code + " ");
                    CreateNode(node.subNodes[0]);
                }
            }
            else
            {
                CreateTxt(node.desc.code);
                if (node.subNodes.Count > 0)
                {
                    CreateTxt("(");
                    for (int i = 0; i < node.subNodes.Count; i++)
                    {
                        if (i > 0)
                            CreateTxt(", ");
                        CreateNode(node.subNodes[i]);
                    }
                    CreateTxt(")");
                }
            }
        }

        private void RenderDescription(string description)
        {
            string cur = "";
            foreach (var ch in description)
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
                    cur += ch;
            }
            CreateTxt(cur);
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
            bool isOperand = model.node?.desc.type == CodeType.Operator;
            if (isOperand ? node == null || SyntaxAnalysis.IsEmptyArgumentNode(node) : !IsVisibleArgument(node))
                return;
            // Descriptions are visual prose, so explicitly group nested operators
            // instead of relying on mathematical symbol precedence in the text.
            bool grouped = isOperand && node.desc.type == CodeType.Operator;
            if (grouped)
                CreateTxt("(");
            model.con.Add(new UiUnitParam()
            {
                con = model.con,
                parent = view.rtf_root.transform,
                node = node
            });
            if (grouped)
                CreateTxt(")");
        }

        private static bool IsVisibleArgument(SyntaxNode node)
        {
            return node != null && !SyntaxAnalysis.IsEmptyArgumentNode(node) &&
                   (node.desc.type != CodeType.Str || !string.IsNullOrEmpty(node.desc.code));
        }
    }
}
