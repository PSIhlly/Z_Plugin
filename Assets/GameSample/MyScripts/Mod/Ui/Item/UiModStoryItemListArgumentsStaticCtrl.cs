

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

namespace Ui.ModStoryItemListArguments.ModStoryItemListArgumentsStatic
{

    public partial class UiModStoryItemListArgumentsStaticParam
    {
        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemListArgumentsStaticModel
    {
        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemListArgumentsStaticCtrl
    {

        public override void OnCreate()
        {

           

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
        }
    }

}