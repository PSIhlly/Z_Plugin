using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryMap.ModStoryMapScene.ModStoryMapSceneUnit;

namespace Ui.ModStory.ModStoryMap.ModStoryMapScene
{

    public partial class UiModStoryMapSceneParam
    {

        public int selPage;
    }
    public partial class UiModStoryMapSceneModel
    {

        public int selPage;
        public SceneForm.Data data;
    }
    public partial class UiModStoryMapSceneCtrl
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
        public void SelPage(int id, SceneForm.Data data=null)
        {
            model.selPage = id;
            model.data = data;
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStoryMapSceneList.SetActive(model.selPage == 0);
            view.page_ModStoryMapSceneUnit.SetActive(model.selPage == 1, new UiModStoryMapSceneUnitParam() { data = model.data });
        }
    }

}