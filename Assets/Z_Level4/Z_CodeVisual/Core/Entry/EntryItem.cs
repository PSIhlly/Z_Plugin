using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Hardware;
using UnityEngine;
using UnityEngine.UI;
using Z_Code;
using Z_Code.Form;
using Z_String;
using Z_Ui.Base;
using static UnityEngine.EventSystems.EventTrigger;
namespace Z_CodeVisual
{

    public class EntryItem : MonoBehaviour
    {
        public Button newBtn;
        public TextMeshProUGUI txt;
        private Entry entry;
        public void Init(Entry entry)
        {
            this.entry = entry;

        }
        public void Draw(int layer, SyntaxNode syntaxNode)
        {
            

            newBtn.onClick.RemoveAllListeners();
            newBtn.onClick.AddListener(() =>
            {
                entry.DrawUnit(syntaxNode);
            });
            foreach (var sub in syntaxNode.subNodes)
            {
                if (sub.desc.type == CodeType.Action)
                {
                    var subItem = entry.CreateItem();
                    subItem.Draw(layer + 1, sub);
                }

            }

            if (syntaxNode.desc.type == CodeType.Action)
            {
                foreach (var sub in syntaxNode.subNodes)
                {
                    var subItem = entry.CreateItem();
                    subItem.Draw(layer + 1, sub);
                }
            }

            txt.text = " ".Repeat(layer)+entry.GetNodeDesc(syntaxNode);
            
        }


    }
}
