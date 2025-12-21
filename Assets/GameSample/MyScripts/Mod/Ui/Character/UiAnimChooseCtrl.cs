using Form;
using System.Collections.Generic;
using Z_DesignStyle;
using Z_Text;

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
                ModManager.instance.assetCtrl.ChooseCharacterAnim(model.prm.options, TextManager.instance.GetTxt("Choose anim"), (res) =>
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