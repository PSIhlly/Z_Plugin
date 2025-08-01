using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

namespace Ui.ModStory.ModStoryMapObject
{

    public partial class UiModStoryMapObjectParam
    {
        public int type;
    }
    public partial class UiModStoryMapObjectModel
    {
        public int type;
        public MapBaseForm.Data data;
    }
    public partial class UiModStoryMapObjectCtrl
    {

        public override void OnCreate()
        {
        }
        public override void OnShow()
        {
            model.data = null;
            model.type = 0;
            if (param != null)
                model.type = param.type;
            Refresh();
        }
        public void SelType(int type)
        {
            model.type = type;
            Refresh();
        }
        public void SelData(MapBaseForm.Data data)
        {
            model.data = data;
            Refresh();
        }
        public void Refresh()
        {
            view.page_ModStoryMapObjectList.SetActive(model.type > 0);
            view.page_ModStoryMapObjectType.SetActive(model.type == 0);
            view.page_ModStoryMapObjectTexture.SetActive(model.data is MapTextureForm.Data);
            view.page_ModStoryMapObjectMask.SetActive(model.data is MapMaskForm.Data);
            view.page_ModStoryMapObjectObject.SetActive(model.data is MapObjectForm.Data);
        }
    }

}