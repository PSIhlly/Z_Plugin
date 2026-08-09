using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem.Form;
using Z_Text;
using UnityEngine;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterList
{

    public partial class UiModStoryCharacterListParam
    {

    }
    public partial class UiModStoryCharacterListModel
    {
        public int? labId;
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterListCtrl
    {

        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiBigItemCtrl> itemCon;
        public override void OnCreate()
        {

            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);
            itemCon = new UiScrViewContainer<UiBigItemCtrl>(this, view.go_bigItem, view.scr_bigItems);

        }
        public override void OnShow()
        {
            model.labId = HasUnclassified() ? LabForm.NoneId : (int?)null;
            Refresh();
        }
        bool HasUnclassified()
        {
            return CharacterProductForm.DatasByLabidProtouid.ContainsKey((LabForm.NoneId, 0));
        }
        public void Refresh()
        {

            var hasUnclassified = HasUnclassified();
            if (model.labId == LabForm.NoneId && !hasUnclassified)
                model.labId = null;
            labCon.Clear();
            labCon.Add(new UiLabParam()
            {
                state = UiLabRenderHelper.AllState
            });
            if (hasUnclassified)
            {
                labCon.Add(new UiLabParam()
                {
                    labId = LabForm.NoneId,
                    state = UiLabRenderHelper.UnclassifiedState
                });
            }
            foreach (var labId in UiLabRenderHelper.GetLabIds(nameof(CharacterProductForm)))
            {
                labCon.Add(new UiLabParam()
                {
                    labId = labId,
                    state = UiLabRenderHelper.LabState
                });
            }
            labCon.Add(new UiLabParam()
            {
                state = UiLabRenderHelper.NewState
            });
            labCon.Refresh();

            itemCon.Clear();
            var datas = model.labId == null
                ? (CharacterProductForm.DatasByProtouid.ContainsKey(0) ? CharacterProductForm.DatasByProtouid[0] : new List<CharacterProductForm.Data>())
                : (CharacterProductForm.DatasByLabidProtouid.ContainsKey((model.labId.Value, 0))
                    ? CharacterProductForm.DatasByLabidProtouid[(model.labId.Value, 0)]
                    : new List<CharacterProductForm.Data>());
            foreach (var data in datas.OrderBy(d => d.uid))
            {
                itemCon.Add(new UiBigItemParam()
                {
                    data = data
                });
            }
            itemCon.Add(new UiBigItemParam()
            {
                data=null
            });
            itemCon.Refresh();



        }
    }

    public partial class UiLabParam
    {
        public int? labId;
        public int state;
    }
    public partial class UiLabModel
    {
        public int? labId;
        public int state;
    }
    public partial class UiLabCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                if (model.state == UiLabRenderHelper.NewState)
                {
                    UiLabRenderHelper.Create(nameof(CharacterProductForm), labId =>
                    {
                        parent.model.labId = labId;
                        parent.Refresh();
                    });
                    return;
                }

                parent.model.labId = model.labId;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.labId = param.labId;
            model.state = param.state;
            Refresh();
        }
        public void Refresh()
        {
            var isNew = model.state == UiLabRenderHelper.NewState;
            view.sta_state.ChangeState(model.state);
            view.sta_.ChangeState(!isNew && parent.model.labId == model.labId ? 1 : 0);
            view.txt_.text = UiLabRenderHelper.GetText(model.labId, isNew);
        }
    }


    public partial class UiBigItemParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiBigItemModel
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiBigItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateCharacter(parent.model.labId ?? 0);
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelPage(1,model.data);
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null)
            {
                view.txt_.text = model.data.name;
                view.img_.BindTexData(TexAssetForm.DataById[model.data.avatarTex]);
            }
        }
    }


}
