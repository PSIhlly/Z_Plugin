using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;
using Z_Code;
using Z_CodeVisual;
using Z_Ui.Base;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UI;
using Z_Code.Form;
using Z_DataSystem.Form;
namespace Ui.ModStoryEventEditWindow
{
    public partial class UiUnitParam
    {
        public Transform parent;
        public UiContainer<UiUnitCtrl> con;
        public SyntaxNode node;
        public int deepth;
        public string txt;
    }
    public partial class UiUnitModel
    {
        public Transform parent;
        public UiContainer<UiUnitCtrl> con;
        public SyntaxNode node;
        public int deepth;
        public string txt;
    }
    public partial class UiUnitCtrl
    {


        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.SelUnit(model.node);
            });
        }
        public override void OnShow()
        {
            model.parent = param.parent;
            model.node = param.node;
            model.deepth = param.deepth;
            model.txt = param.txt;
            model.con = param.con;
            Refresh();
        }


        public void Refresh()
        {
            view.sta_.ChangeState(parent.model.selUnit!=null&& parent.model.selUnit == model.node?1:0) ;
            gameObject.transform.parent = model.parent;
            float height = 200f - 20 * model.deepth;
            if (model.node == null)
            {
                view.txt_.text = model.txt;
                view.btn_.gameObject.SetActive(false);
            }
            else
            {
                view.txt_.text = "";
                view.btn_.gameObject.SetActive(true);

                switch (model.node.desc.type)
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
                        else
                        {
                            view.txt_.text = model.node.desc.code;
                        }
                        break;
                    default:
                        view.txt_.text = model.node.desc.code;
                        break;
                }
                view.img_.gameObject.SetActive(GlobalEventHelper.IsEventTex(model.node.desc.code));
                view.txt_.gameObject.SetActive(!GlobalEventHelper.IsEventTex(model.node.desc.code));

                if (GlobalEventHelper.IsEventTex(model.node.desc.code))
                {
                    view.img_.sprite = TexAssetForm.DataByName[string.IsNullOrEmpty(model.node.desc.code) ? GlobalNameHelper.GetDefaultTexName() : GlobalEventHelper.GetEventAssetTexName(model.node.desc.code)].GetSprite();
                }
            }

            var size = view.rtf_root.sizeDelta;
            size.y = height;
            view.rtf_root.sizeDelta = size;
            view.img_.rectTransform.sizeDelta = new Vector2(height,height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(view.img_.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(view.rtf_unit);
        }

        private void CreateTxt(string desc)
        {
            model.con.Add(new UiUnitParam()
            {
                con = model.con,
                deepth = model.deepth + 1,
                parent = view.rtf_root.transform,
                txt = desc,
                node = null
            });
        }
        private void CreateNode(SyntaxNode node)
        {
            model.con.Add(new UiUnitParam()
            {
                con = model.con,
                deepth = model.deepth + 1,
                parent = view.rtf_root.transform,
                node = node
            });
        }
    }
}
