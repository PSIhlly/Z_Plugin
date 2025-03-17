using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;

namespace Ui.WarRoom
{
    public partial class UiWarRoomCtrl: IZ_Listener<ItemEvent>
    {
        public override void OnCreate()
        {
            this.Register<ItemEvent>();
            base.OnCreate();
        }

        
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            var data = ItemForm.DataById[ItemDefines.SPACE_TIME_FRAGMENT_ID];
            view.img_StfIcon.sprite = TextureHelper.GetSpriteByPath(data.icon);
            view.txt_StfCnt.text = data.count.ToString();

            data = ItemForm.DataById[ItemDefines.SOUL_POWER_ID];
            view.img_SpIcon.sprite = TextureHelper.GetSpriteByPath(data.icon);
            view.txt_SpCnt.text = data.count.ToString();
        }

        public void OnEvent(ItemEvent evt)
        {
            Refresh();
        }

    }
}
