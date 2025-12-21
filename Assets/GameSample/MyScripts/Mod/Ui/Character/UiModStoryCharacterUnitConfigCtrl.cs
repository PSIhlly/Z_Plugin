using Form;
using System;
using Ui.AnimChoose;
using Z_Text;
using Z_Ui.Notify;

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
        UiAnimChooseCtrl[] idles;
        UiAnimChooseCtrl[] moves;
        public override void OnCreate()
        {
            idles = new UiAnimChooseCtrl[] { view.model_idle0AnimChoose, view.model_idle1AnimChoose, view.model_idle2AnimChoose, view.model_idle3AnimChoose };
            moves = new UiAnimChooseCtrl[] { view.model_move0AnimChoose, view.model_move1AnimChoose, view.model_move2AnimChoose, view.model_move3AnimChoose };
            view.btn_faceType.onClick.AddListener(() =>
            {
                var items = new EntryItem();
                foreach (FaceType tp in Enum.GetValues(typeof(FaceType)))
                {
                    items.Add(TextManager.instance.GetTxt(tp.ToString()), null, (int)tp);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose faceType"),
                    true, (item) =>
                    {
                        model.data.faceType = (FaceType)item.id;
                        Refresh();
                        return true;
                    }, items);

            });
            view.btn_hpArgument.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacterParam(TextManager.instance.GetTxt("Choose Hp param"), (item) =>
                {
                    model.data.hpParamName = item.content;
                    Refresh();
                });
            });
            view.btn_moveSpeedParameter.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacterParam(TextManager.instance.GetTxt("Choose Speed param"), (item) =>
                {
                    model.data.speedParamName = item.content;
                    Refresh();
                });
            });

            view.btn_unique.onClick.AddListener(() =>
            {
                model.data.unique = !model.data.unique;
                Refresh();
            });


        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_hpArgument.text = model.data.hpParamName;
            view.txt_moveSpeedParameter.text = model.data.speedParamName;

            view.txt_faceType.oriText = model.data.faceType.ToString();

            idles[0].Set(new UiAnimChooseParam() { dic = model.data.defaultAnimName, options = model.data.animDic, key = (model.data.faceType == FaceType.FourDirection ? "idle0" : "idle") });
            moves[0].Set(new UiAnimChooseParam() { dic = model.data.defaultAnimName, options = model.data.animDic, key = (model.data.faceType == FaceType.FourDirection ? "move0" : "move") });
            for (int i = 1; i < 4; i++)
            {
                idles[i].gameObject.SetActive(model.data.faceType == FaceType.FourDirection);
                idles[i].Set(new UiAnimChooseParam() { dic = model.data.defaultAnimName, options = model.data.animDic, key = "idle" + i });
                moves[i].gameObject.SetActive(model.data.faceType == FaceType.FourDirection);
                moves[i].Set(new UiAnimChooseParam() { dic = model.data.defaultAnimName, options = model.data.animDic, key = "move" + i });
            }

            view.sta_unique.ChangeState(model.data.unique ? 1 : 0);

            view.model_EventChooseCharacterTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterTouchEvent" });
            view.model_EventChooseCharacterLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterLeaveEvent" });
            view.model_EventChooseObjectTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectTouchEvent" });
            view.model_EventChooseObjectLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectLeaveEvent" });
            view.model_EventChooseShow.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onShowEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onPerSecondEvent" });

        }
    }

}