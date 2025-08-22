using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryMapObject.ModStoryMapObjectList;
using Ui.ModStory.ModStoryMapObject.ModStoryMapObjectTexture;
using Ui.ModStory.ModStoryMapObject.ModStoryMapObjectMask;
using Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject;

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
        public override void OnHide()
        {
            GameManager.instance.saveCtrl.SaveObject(ModManager.instance.GetStoryCoreFolder());
            GameManager.instance.saveCtrl.SaveMaterial(ModManager.instance.GetStoryCoreFolder());
        }
        public void SelType(int type)
        {
            model.type = type;
            model.data = null;
            Refresh();
        }
        public void SelData(MapBaseForm.Data data)
        {
            model.data = data;
            Refresh();
        }
        public void Refresh()
        {
            view.page_ModStoryMapObjectList.SetActive(model.type > 0&& model.data==null, new UiModStoryMapObjectListParam()
            {
                 type = model.type,
            });
            view.page_ModStoryMapObjectType.SetActive(model.type == 0);
            view.page_ModStoryMapObjectTexture.SetActive(model.data is MapTextureForm.Data, new UiModStoryMapObjectTextureParam()
            {
                data = model.data is MapTextureForm.Data ? (MapTextureForm.Data)model.data : null,
            });
            view.page_ModStoryMapObjectMask.SetActive(model.data is MapMaskForm.Data, new UiModStoryMapObjectMaskParam()
            {
                data = model.data is MapMaskForm.Data ? (MapMaskForm.Data)model.data : null,
            });
            view.page_ModStoryMapObjectObject.SetActive(model.data is MapObjectForm.Data, new UiModStoryMapObjectObjectParam()
            {
                data = model.data is MapObjectForm.Data?(MapObjectForm.Data)model.data:null,
            });
        }
    }

}