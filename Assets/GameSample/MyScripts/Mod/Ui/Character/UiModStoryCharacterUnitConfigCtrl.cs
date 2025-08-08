using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using UnityEngine;
using Z_Text;
using Z_Ui.Notify;
using Z_Code.Form;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitConfig
{

    public partial class UiModStoryCharacterUnitConfigParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitConfigModel
    {

        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitConfigCtrl
    {

        public override void OnCreate()
        {

            view.btn_hpArgument.onClick.AddListener(() =>
            {
                List<(string, Sprite)> lst = new List<(string,Sprite)>();
                foreach (var data in CharacterParamForm.DataByName.Values)
                {
                    lst.Add((data.name,null));
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Hp param"),
                    true, (id) =>
                    {
                        model.data.hpParamName = lst[id].Item1;
                        Refresh();
                        return true;
                    }, lst);
            });
            view.btn_moveSpeedParameter.onClick.AddListener(() =>
            {
                List<(string, Sprite)> lst = new List<(string, Sprite)>();
                foreach (var data in CharacterParamForm.DataByName.Values)
                {
                    lst.Add((data.name, null));
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Speed param"),
                    true, (id) =>
                    {
                        model.data.hpParamName = lst[id].Item1;
                        Refresh();
                        return true;
                    }, lst);
            });
            view.btn_idleAnim.onClick.AddListener(() =>
            {
                List<(string, Sprite)> lst = new List<(string, Sprite)>();
                foreach (var key in model.data.animDic.Keys)
                {
                    lst.Add((key, null));
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Idle anim"),
                    true, (id) =>
                    {
                        model.data.hpParamName = lst[id].Item1;
                        Refresh();
                        return true;
                    }, lst);
            });
            view.btn_moveAnim.onClick.AddListener(() =>
            {
                List<(string, Sprite)> lst = new List<(string, Sprite)>();
                foreach (var key in model.data.animDic.Keys)
                {
                    lst.Add((key, null));
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Move anim"),
                    true, (id) =>
                    {
                        model.data.hpParamName = lst[id].Item1;
                        Refresh();
                        return true;
                    }, lst);
            });
            view.btn_onTouchEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Character, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onTouchEvent"), false, 2, (lst) =>
                {
                    model.data.onTouchEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onLeaveEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Character, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onLeaveEvent"), false, 2, (lst) =>
                {
                    model.data.onLeaveEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onShowEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Character, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onShowEvent"), false, 2, (lst) =>
                {
                    model.data.onShowEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });

        }
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            view.txt_hpArgument.text = model.data.hpParamName;
            view.txt_moveSpeedParameter.text = model.data.speedParamName;
            view.txt_idleAnim.text = model.data.idleAnimName;
            view.txt_moveAnim.text = model.data.moveAnimName;
            view.txt_onTouchEvent.text = model.data.onTouchEvent;
            view.txt_onLeaveEvent.text = model.data.onLeaveEvent;
            view.txt_onShowEvent.text = model.data.onShowEvent;
        }
    }

}