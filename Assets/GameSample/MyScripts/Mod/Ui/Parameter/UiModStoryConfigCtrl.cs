

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
using System.Runtime.InteropServices.ComTypes;
using Z_Map;

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
                ModManager.instance.assetCtrl.ChooseCharacter(TextManager.instance.GetTxt("Choose main character"), (data) =>
                {
                    GameManager.instance.curConfig.mainCharacterUid = data.uid;
                    if (!GameManager.instance.curConfig.defaultTeam.Contains(data.uid))
                    {
                        GameManager.instance.curConfig.defaultTeam.Add(data.uid);
                    }
                    if (!GameManager.instance.curConfig.defaultTeamActive.Contains(data.uid))
                    {
                        GameManager.instance.curConfig.defaultTeamActive.Add(data.uid);
                    }
                    Refresh();
                });
            });
            view.btn_perspective.onClick.AddListener(() =>
            {
                var items = new EntryItem();
                foreach (CameraMode tp in Enum.GetValues(typeof(CameraMode)))
                    items.Add(tp.ToString(),id:(int)tp);

                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose perspective"),
                    true, (item) =>
                    {
                        GameManager.instance.curConfig.cameraMode = (CameraMode)item.id;
                        

                        Refresh();
                        return true;
                    }, items);
            });

        }
        public override void OnShow()
        {

            Refresh();
        }

        public void Refresh()
        {

            view.txt_mainCharacter.text = CharacterProductForm.DataByUid[GameManager.instance.curConfig.mainCharacterUid].name;
            view.txt_perspective.text = TextManager.instance.GetTxt(GameManager.instance.curConfig.cameraMode.ToString());
        }
    }

}