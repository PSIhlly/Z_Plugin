using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;
using Z_Code;
using Z_CodeVisual;
using Z_Ui.Base;
using Z_String;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEditor.Experimental.GraphView.GraphView;
using Z_Code.Form;
using System;
using Z_Ui.Notify;
using Z_Text;
using UnityEditor.Hardware;
using Form;
using System.Numerics;
namespace Ui.ModStoryEventEditWindow
{
    public partial class UiItemParam
    {
        public UiContainer<UiItemCtrl> con;
        public SyntaxNode node;
        public int deepth;
        public Action<SyntaxNode> onClick;
    }
    public partial class UiItemModel
    {
        public UiContainer<UiItemCtrl> con;
        public SyntaxNode node;
        public int deepth;
    }
    public partial class UiItemCtrl
    {


        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if(model.node==null)
                {
                    ModManager.instance.assetCtrl.ChooseCmd( EventType.All, CmdTypeDataForm.defaultData.name, (item) =>
                    {
                        GameCmdDataForm.Data sel = GameCmdDataForm.DataByName[item.content];
                        parent.model.cpr.Compile(sel.defaultCode, out var res);
                        parent.model.curEntry.AddRange(res);
                        parent.ApplyEntry();
                    });
                }
                else
                {
                    parent.SelItem(model.node);
                }
            });
           
        }
        public override void OnShow()
        {
            model.con = param.con;
            model.node = param.node;
            model.deepth = param.deepth;
            Refresh();
        }


        public void Refresh()
        {
            view.sta_isEmpty.ChangeState(model.node == null?0:1);
            if (model.node == null)
            {

            }else
            {
                int curRender = model.con.GetNowRenderId();
                foreach (var sub in model.node.subNodes)
                {
                    if (sub.desc.type == CodeType.Action)
                    {
                        model.con.Add(new UiItemParam()
                        {
                            con = model.con,
                            node = sub,
                            deepth = model.deepth + 1,
                        }, ++curRender);
                    }

                }

                if (model.node.desc.type == CodeType.Action)
                {
                    foreach (var sub in model.node.subNodes)
                    {
                        model.con.Add(new UiItemParam()
                        {
                            con = model.con,
                            node = sub,
                            deepth = model.deepth + 1,
                        }, ++curRender);
                    }
                }

                view.txt_.text = " ".Repeat(model.deepth) + GetNodeDesc(model.node);
            }

            

        }


        public string GetNodeDesc(SyntaxNode node)
        {

            string res = "";
            switch (node.desc.type)
            {
                case CodeType.FuncName:
                case CodeType.Reserved:
                case CodeType.Operator:
                    if (CmdDataForm.DataByName.ContainsKey(node.desc.code))
                    {
                        var form = CmdDataForm.DataByName[node.desc.code];
                        res = form.desc;
                        for (int i = 0; i < form.prmNames.Count; i++)
                        {
                            res = res.Replace($"{{{i}}}", GetNodeDesc(node.subNodes[i]));
                        }
                    }
                    else
                    {
                        res = node.desc.code;
                    }
                    break;
                default:
                    res = node.desc.code;
                    break;
            }
            return res;
        }
    }
}
