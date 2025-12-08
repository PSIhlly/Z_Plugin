using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Video;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Text;
using Z_Texture;
using Z_Ui.Base;
using Z_Video;
using static UnityEditor.Progress;

namespace Ui.AnimChoose
{

    public partial class UiAnimChooseParam
    {
        public string key;
        public Dictionary<string, CharacterAnimForm.Data> options;
        public Dictionary<string, string> dic;
    }
    public partial class UiAnimChooseModel
    {
        public UiAnimChooseParam prm;
    }
    public partial class UiAnimChooseCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacterAnim(model.prm.options, TextManager.instance.GetTxt("Choose anim"),(res) =>
                {
                    model.prm.dic[model.prm.key] = res.content;
                    Refresh();
                });
            });

        }
        public override void OnShow()
        {
        }
        public void Set(UiAnimChooseParam prm)
        {
            model.prm = prm;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.oriText = model.prm.key;
            view.txt_anim.text = model.prm.dic.GetDv(model.prm.key, "");
        }
    }

}