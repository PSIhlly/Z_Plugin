using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_Code;
using Z_Code.Form;
using Z_Ui.Base;
namespace Z_CodeVisual
{

    public class Entry
    {        


        EntryUnit unitProto;
        EntryItem itemProto;
        Transform unitRoot;
        Transform itemRoot;
        public Entry(EntryUnit unitProto, EntryItem itemProto, Transform unitRoot, Transform itemRoot)
        {
            this.unitProto = unitProto;
            this.itemProto = itemProto;
            this.unitRoot = unitRoot;
            this.itemRoot = itemRoot;
        }
        public void DrawItems(List<SyntaxNode> nodes)
        {
            ClearItems();
            for (int i = 0; i < nodes.Count; i++)
            {
                var item = CreateItem();
                item.Draw(0,nodes[i]);
            }
        }

        public string GetNodeDesc(SyntaxNode node) {

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
                            res=res.Replace($"{{{i}}}", GetNodeDesc(node.subNodes[i]));
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


        public void DrawUnit(SyntaxNode syntaxNode)
        {
            ClearUnits();
            var unit = CreateUnit(unitRoot);
            unit.Draw(syntaxNode);
        }

        public EntryUnit CreateUnit(Transform root)
        {
            var unit = GameObject.Instantiate(unitProto.gameObject, root).GetComponent<EntryUnit>();
            unit.Init(this);
            unit.gameObject.SetActive(true);
            return unit;
        }





        public void ClearUnits()
        {
            foreach (var o in unitRoot)
            {
                if (o is Transform tf && tf != unitProto.transform)
                {
                    GameObject.Destroy(tf.gameObject);
                }
            }
        }
        public EntryItem CreateItem()
        {
            var item = GameObject.Instantiate(itemProto.gameObject, itemRoot).GetComponent<EntryItem>();
            item.Init(this);
            item.gameObject.SetActive(true);
            return item;
        }
        public void ClearItems()
        {
            foreach (var o in itemRoot)
            {
                if (o is Transform tf && tf != itemProto.transform)
                {
                    GameObject.Destroy(tf.gameObject);
                }
            }
        }
    }
}
