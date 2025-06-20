

using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Ui.ModStory.ModStoryParameter.ModStoryConfig
{

    public partial class UiModStoryConfigParam
    {

    }
    public partial class UiModStoryConfigModel
    {

    }
    public partial class UiModStoryConfigCtrl
    {

        public override void OnCreate()
        {
            view.btn_mainCharacter.onClick.AddListener(() =>
            {
                List<string> lst = new List<string>();
                List<Sprite> spriteLst = new List<Sprite>();
                if (CharacterProductForm.DatasByIsproto.ContainsKey(true))
                {
                    foreach (var data in CharacterProductForm.DatasByIsproto[true])
                    {
                        lst.Add(data.name);
                        spriteLst.Add(TexAssetForm.DataByName[data.avatarTexName].sprite);
                    }
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose main character"),
                    true, (id) =>
                    {
                        ConfigForm.DataByUid[1].mainCharacterName = lst[id];
                        Refresh();
                        return true;
                    }, lst, spriteLst);
            });

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

            view.txt_mainCharacter.text = ConfigForm.DataByUid[1].mainCharacterName;
        }
    }

}