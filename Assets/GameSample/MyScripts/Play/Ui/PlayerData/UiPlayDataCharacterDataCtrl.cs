using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_Text;
using Unity.Burst.Intrinsics;
using Z_DataSystem.Form;

namespace Ui.PlayData.PlayDataCharacter.PlayDataCharacterData
{

    public partial class UiPlayDataCharacterDataParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiPlayDataCharacterDataModel
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiPlayDataCharacterDataCtrl
    {

        UiScrViewContainer<UiGameArgsCtrl> gameArgsCon;
        public override void OnCreate()
        {
            gameArgsCon = new UiScrViewContainer<UiGameArgsCtrl>(view.go_gameArgs, view.scr_gameArgs);
        }
        public override void OnShow()
        {
            model.data=param.data;
            Refresh();
        }
        public void Refresh()
        {

            view.img_tachie.sprite = TexAssetForm.DataByName[model.data.tachie].sprite;
            view.txt_desc.text = model.data.desc;
            view.txt_name.text= model.data.name;
            gameArgsCon.Clear();
            foreach (var pair in model.data.paramDic)
            {
                if (model.data.CanShow(pair.Key))
                    gameArgsCon.Add(new UiGameArgsParam()
                    {
                        content = pair.Key + ":" + pair.Value.v
                    });
            }
            gameArgsCon.Refresh();
        }
    }

    public partial class UiGameArgsParam
    {
        public string content;
    }
    public partial class UiGameArgsModel
    {
        public string content;

    }
    public partial class UiGameArgsCtrl
    {

        public override void OnCreate()
        {



        }
        public override void OnShow()
        {
            model.content = param.content;
            Refresh();
        }
        public void Refresh()
        {

            view.txt_.text = model.content;
        }
    }


}