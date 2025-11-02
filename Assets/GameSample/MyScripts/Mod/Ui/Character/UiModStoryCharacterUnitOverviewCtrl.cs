using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
using Z_DataSystem.Form;
using UnityEngine;
using Z_DataSystem;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitOverview
{

    public partial class UiModStoryCharacterUnitOverviewParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitOverviewModel
    {
        public CharacterProductForm.Data data;

    }
    public partial class UiModStoryCharacterUnitOverviewCtrl:IZ_Listener<AssetEvent>
    {

        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacter(model.data.uid);
                parent.parent.SelPage(0);

            });
            view.ipt_name.onFinishInput+=(s)=>
            {
                var lst = new List<string>();
                foreach(var data in CharacterProductForm.DataByName.Values)
                    lst.Add(data.name);

                if(StringHelper.IsUniqueName(lst, s))
                    model.data.name = s;
                Refresh();

            };
            view.ipt_label.onFinishInput+=(s)=>
            {
                model.data.label = s;
                Refresh();
            };
            view.btn_tachie.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportCharacterTachie(model.data.uid);
            });
            view.ipt_desc.onFinishInput += (s) =>
            {
                model.data.desc = s;
                Refresh();
            };
            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportCharacterAvatar(model.data.uid);
            });

        }

        public override void OnShow()
        {
            if (param != null)
                model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.ipt_name.Set(model.data.name);
            view.ipt_label.Set(model.data.label);
            view.img_image.sprite = TexAssetForm.DataByName[model.data.avatarTexName].GetSprite();
            view.img_tachie.sprite = TexAssetForm.DataByName[model.data.tachie].GetSprite();
            view.ipt_desc.Set(model.data.desc);
        }
        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }

    }

}