

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
using Z_DesignStyle;

namespace Ui.ModStory.ModStoryParameter.ModStoryConfig
{

    public partial class UiModStoryConfigParam
    {

    }
    public partial class UiModStoryConfigModel
    {
        public int selTeamerUid;
        public int selActiveTeamerUid;
        public int selItemUid;
    }
    public partial class UiModStoryConfigCtrl
    {
        UiScrViewContainer<UiTeamerCtrl> teamCon;
        UiScrViewContainer<UiActiveTeamerCtrl> activeTeamCon;
        UiScrViewContainer<UiItemCtrl> bagCon;

        public override void OnCreate()
        {
            view.btn_mainCharacter.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacter(TextManager.instance.GetTxt("Choose main character"), (data) =>
                {
                    var oldUid = GameManager.instance.curProgress.characterUid;
                    GameManager.instance.curProgress.team.Remove(oldUid);
                    GameManager.instance.curProgress.teamActive.Remove(oldUid);
                    GameManager.instance.curProgress.characterUid = data.uid;

                    if (!GameManager.instance.curProgress.team.Contains(data.uid))
                    {
                        GameManager.instance.curProgress.team.Add(data.uid);
                    }
                    if (!GameManager.instance.curProgress.teamActive.Contains(data.uid))
                    {
                        GameManager.instance.curProgress.teamActive.Add(data.uid);
                    }
                    Refresh();
                });
            });
            view.btn_perspective.onClick.AddListener(() =>
            {
                var items = new EntryItem();
                foreach (CameraMode tp in Enum.GetValues(typeof(CameraMode)))
                    items.Add(tp.ToString(), id: (int)tp);

                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose perspective"),
                    true, (item) =>
                    {
                        GameManager.instance.curProgress.cameraMode = (CameraMode)item.id;


                        Refresh();
                        return true;
                    }, items);
            });

            view.btn_equip.onClick.AddListener(() =>
            {
                GameManager.instance.curProgress.enableEquip = !GameManager.instance.curProgress.enableEquip;
                Refresh();
            });

            view.btn_skill.onClick.AddListener(() =>
            {
                GameManager.instance.curProgress.enableSkill = !GameManager.instance.curProgress.enableSkill;
                Refresh();
            });

            view.btn_minimap.onClick.AddListener(() =>
            {
                GameManager.instance.curProgress.enableMinimap = !GameManager.instance.curProgress.enableMinimap;
                Refresh();
            });

            view.btn_largeMap.onClick.AddListener(() =>
            {
                GameManager.instance.curProgress.enableLargeMap = !GameManager.instance.curProgress.enableLargeMap;
                Refresh();
            });

            view.btn_mission.onClick.AddListener(() =>
            {
                GameManager.instance.curProgress.enableMission = !GameManager.instance.curProgress.enableMission;
                Refresh();
            });

            view.btn_deleteActiveTeamer.onClick.AddListener(() =>
            {
                if (GameManager.instance.curProgress.characterUid == model.selActiveTeamerUid)
                {
                    GameManager.instance.curProgress.characterUid = 0;
                }
                GameManager.instance.curProgress.teamActive.Remove(model.selActiveTeamerUid);
                model.selActiveTeamerUid = 0;
                Refresh();
            });
            view.btn_deleteTeamer.onClick.AddListener(() =>
            {
                if (GameManager.instance.curProgress.characterUid == model.selTeamerUid)
                {
                    GameManager.instance.curProgress.characterUid = 0;
                }
                GameManager.instance.curProgress.teamActive.Remove(model.selTeamerUid);
                GameManager.instance.curProgress.team.Remove(model.selTeamerUid);
                model.selTeamerUid = 0;
                Refresh();
            });
            view.btn_deleteItem.onClick.AddListener(() =>
            {
                GameManager.instance.curProgress.bag.Remove(model.selItemUid);
                model.selItemUid = 0;
                Refresh();
            });

            teamCon = new UiScrViewContainer<UiTeamerCtrl>(this, view.go_teamer, view.scr_teamers);
            activeTeamCon = new UiScrViewContainer<UiActiveTeamerCtrl>(this, view.go_activeTeamer, view.scr_activeTeamers);
            bagCon = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);

        }
        public override void OnShow()
        {

            Refresh();
        }

        public void Refresh()
        {

            teamCon.Clear();
            foreach (var uid in GameManager.instance.curProgress.team)
            {
                teamCon.Add(new UiTeamerParam() { uid = uid });
            }
            teamCon.Add(new UiTeamerParam() { uid = 0 });
            teamCon.Refresh();

            activeTeamCon.Clear();
            foreach (var uid in GameManager.instance.curProgress.teamActive)
            {
                activeTeamCon.Add(new UiActiveTeamerParam() { uid = uid });
            }
            activeTeamCon.Add(new UiActiveTeamerParam() { uid = 0 });
            activeTeamCon.Refresh();

            bagCon.Clear();
            foreach (var uid in GameManager.instance.curProgress.bag)
            {
                bagCon.Add(new UiItemParam() { uid = uid });
            }
            bagCon.Add(new UiItemParam() { uid = 0 });
            bagCon.Refresh();

            view.txt_mainCharacter.text = CharacterProductForm.DataByUid[GameManager.instance.curProgress.characterUid].name;
            view.txt_perspective.text = TextManager.instance.GetTxt(GameManager.instance.curProgress.cameraMode.ToString());

            view.go_teamerExist.SetActive(model.selTeamerUid != 0);
            view.go_activeTeamerExist.SetActive(model.selActiveTeamerUid != 0);
            view.go_itemExist.SetActive(model.selItemUid != 0);

            view.sta_equip.ChangeState(GameManager.instance.curProgress.enableEquip ? 1 : 0);
            view.sta_skill.ChangeState(GameManager.instance.curProgress.enableSkill ? 1 : 0);
            view.sta_minimap.ChangeState(GameManager.instance.curProgress.enableMinimap ? 1 : 0);
            view.sta_largeMap.ChangeState(GameManager.instance.curProgress.enableLargeMap ? 1 : 0);
            view.sta_mission.ChangeState(GameManager.instance.curProgress.enableMission ? 1 : 0);
        }
    }

    public partial class UiTeamerParam
    {
        public int uid;
    }
    public partial class UiTeamerModel
    {
        public int uid;
    }
    public partial class UiTeamerCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacter(TextManager.instance.GetTxt("Choose character"), (data) =>
                {
                    if (data != null && !GameManager.instance.curProgress.team.Contains(data.uid))
                    {
                        GameManager.instance.curProgress.team.Add(data.uid);
                        parent.Refresh();
                    }
                });
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.selTeamerUid = model.uid;
                parent.Refresh();
            });
        }
        public override void OnShow()
        {
            model.uid = param.uid;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.uid == 0 ? 0 : 1);
            view.sta_.ChangeState(model.uid == parent.model.selTeamerUid && model.uid != 0 ? 1 : 0);
            if (model.uid != 0)
            {
                var data = CharacterProductForm.DataByUid.GetDv(model.uid, null);
                if (data != null)
                {
                    view.txt_.text = data.name;
                    view.img_.BindTexData(TexAssetForm.DataById[data.avatarTex]);
                }
            }
        }
    }

    public partial class UiActiveTeamerParam
    {
        public int uid;
    }
    public partial class UiActiveTeamerModel
    {
        public int uid;
    }
    public partial class UiActiveTeamerCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseActiveCharacter(TextManager.instance.GetTxt("Choose active character"), (data) =>
                {
                    if (data != null && !GameManager.instance.curProgress.teamActive.Contains(data.uid))
                    {
                        GameManager.instance.curProgress.teamActive.Add(data.uid);
                        parent.Refresh();
                    }
                });
            });
            view.btn_.onClick.AddListener(() =>
            {

                parent.model.selActiveTeamerUid = model.uid;
                parent.Refresh();
            });
        }
        public override void OnShow()
        {
            model.uid = param.uid;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.uid == 0 ? 0 : 1);
            view.sta_.ChangeState(model.uid == parent.model.selActiveTeamerUid && model.uid != 0 ? 1 : 0);
            if (model.uid != 0)
            {
                var data = CharacterProductForm.DataByUid.GetDv(model.uid, null);
                if (data != null)
                {
                    view.txt_.text = data.name;
                    view.img_.BindTexData(TexAssetForm.DataById[data.avatarTex]);
                }
            }
        }
    }

    public partial class UiItemParam
    {
        public int uid;
    }
    public partial class UiItemModel
    {
        public int uid;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseItem(TextManager.instance.GetTxt("Choose item"), (data) =>
                {
                    if (data != null && !GameManager.instance.curProgress.bag.Contains(data.uid))
                    {
                        GameManager.instance.curProgress.bag.Add(data.uid);
                        parent.Refresh();
                    }
                });
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.selItemUid = model.uid;
                parent.Refresh();
            });
        }
        public override void OnShow()
        {
            model.uid = param.uid;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.uid == 0 ? 0 : 1);
            view.sta_.ChangeState(model.uid == parent.model.selItemUid && model.uid != 0 ? 1 : 0);
            if (model.uid != 0)
            {
                var data = ItemProductForm.DataByUid.GetDv(model.uid, null);
                if (data != null)
                {
                    view.txt_.text = data.name;
                    view.img_.BindTexData(TexAssetForm.DataById[data.iconTexName]);
                }
            }
        }
    }

}