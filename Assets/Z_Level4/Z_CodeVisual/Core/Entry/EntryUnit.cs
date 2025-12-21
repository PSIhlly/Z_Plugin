using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Z_Code;
using Z_Code.Form;
namespace Z_CodeVisual
{

    public class EntryUnit : MonoBehaviour
    {
        static Dictionary<float, Color> colors = new Dictionary<float, Color>() { { 50, Color.white }, { 60, Color.gray }, { 70, Color.white }, { 80, Color.gray }, { 90, Color.white }, { 100, Color.gray } };

        public Button btn;
        public TextMeshProUGUI txt;
        private Entry entry;
        public void Init(Entry entry)
        {
            this.entry = entry;
        }
        public float Draw(SyntaxNode syntaxNode)
        {
            float height = 50f;
            switch (syntaxNode.desc.type)
            {
                case CodeType.FuncName:
                case CodeType.Reserved:
                case CodeType.Operator:
                    if (CmdDataForm.DataByName.ContainsKey(syntaxNode.desc.code))
                    {
                        var form = CmdDataForm.DataByName[syntaxNode.desc.code];
                        string cur = "";

                        foreach (var ch in form.desc)
                        {
                            if (ch == '}')
                            {
                                int id = int.Parse(cur);
                                var subUnit = entry.CreateUnit(transform);
                                height = Mathf.Max(subUnit.Draw(syntaxNode.subNodes[id]) + 10, height);
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
                        CreateTxt(cur);
                    }
                    else
                    {
                        CreateTxt(syntaxNode.desc.code);
                    }
                    break;
                default:
                    CreateTxt(syntaxNode.desc.code);
                    break;
            }

            var size = gameObject.GetComponent<RectTransform>().sizeDelta;
            size.y = height;
            gameObject.GetComponent<RectTransform>().sizeDelta = size;
            gameObject.GetComponent<Image>().color = colors[height];
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
            return height;
        }
        private void CreateTxt(string desc)
        {
            var txtGo = Instantiate(txt.gameObject, transform);
            txtGo.SetActive(true);
            txtGo.GetComponent<TextMeshProUGUI>().text = desc;

        }

    }
}
