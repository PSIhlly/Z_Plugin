using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryMapObject.ModStoryMapObjectType;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectType
{

    public partial class UiModStoryMapObjectTypeParam
    {
    }
    public partial class UiModStoryMapObjectTypeModel
    {
    }
    public partial class UiModStoryMapObjectTypeCtrl
    {

        public override void OnCreate()
        {
            view.btn_texture.onClick.AddListener(() =>
            {
                parent.SelType(1);
            });

            view.btn_mask.onClick.AddListener(() =>
            {
                parent.SelType(2);
            });

            view.btn_object.onClick.AddListener(() =>
            {
                parent.SelType(3);
            });

        }
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {

        }
    }

}