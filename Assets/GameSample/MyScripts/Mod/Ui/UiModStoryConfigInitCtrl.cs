using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Z_Map;
using Z_Ui.Notify;
using Z_Text;
using Z_DataSystem.Form;

namespace Ui.ModStoryConfig.ModStoryConfigInit
{
    public partial class UiModStoryConfigInitModel
    {
    }
    public partial class UiModStoryConfigInitCtrl
    {
        public override void OnCreate()
        {
            view.btn_mainCharacter.onClick.AddListener(() =>
            {
                List<string> lst=new List<string>();
                List<Sprite> spriteLst=new List<Sprite>();
                foreach(var data in CharacterProductForm.DataByName.Values)
                {
                    lst.Add(data.name);
                    spriteLst.Add(TexAssetForm.DataByName[data.avatarTexName].sprite);
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

        public override void Close()
        {
            base.Close();
        }
        public void Refresh()
        {
            view.txt_mainCharacter.text = ConfigForm.DataByUid[1].mainCharacterName;
        }
    }

}

