using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryItem.ModStoryItemUnit;

namespace Ui.ModStory.ModStoryItem
{

    public partial class UiModStoryItemParam
    {

        public int selPage;
    }
    public partial class UiModStoryItemModel
    {

        public int selPage;
        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemCtrl
    {

        public override void OnCreate()
        {


        }
        public override void OnShow()
        {
            model.selPage = 0;
            if (param != null)
                model.selPage = param.selPage;
            Refresh();
        }
        public override void OnHide()
        {
            GameManager.instance.saveCtrl.SaveItem(ModManager.instance.GetStoryCoreFolder());
        }
        public void SelPage(int id, ItemProductForm.Data data = null)
        {
            model.selPage = id;
            model.data = data; 
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStoryItemList.SetActive(model.selPage == 0);
            view.page_ModStoryItemUnit.SetActive(model.selPage == 1, new UiModStoryItemUnitParam() { data = model.data });
        }
    }

}