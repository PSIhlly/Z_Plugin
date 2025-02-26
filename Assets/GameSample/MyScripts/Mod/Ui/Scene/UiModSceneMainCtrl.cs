using Form;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Map;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;

namespace Ui
{
    public partial class UiModSceneMainModel
    {
        public string viewX
        {
            set
            {
                int.TryParse(value, out int v);
                var x = Mathf.Max(Mathf.Min(v + ModSceneManager.instance.offset, (int)MapManager.instance.sizeLimit.x), 0)*MapManager.instance.dataCtrl.mainData.mapUnitSize.x;
                ModSceneManager.instance.SetCamera( x, (int)CameraInstance.instance.tarTrs.position.y, (int)CameraInstance.instance.tarTrs.position.z);
            }
            get
            {
                return ((int)(CameraInstance.instance.tarTrs.position.x/ MapManager.instance.dataCtrl.mainData.mapUnitSize.x) - ModSceneManager.instance.offset).ToString();
            }
        }
        public string viewY
        {
            set
            {
                int.TryParse(value, out int v);
                var y = Mathf.Max(Mathf.Min(v+ ModSceneManager.instance.offset, (int) MapManager.instance.sizeLimit.y), 0) * MapManager.instance.dataCtrl.mainData.mapUnitSize.y;
                ModSceneManager.instance.SetCamera((int)CameraInstance.instance.tarTrs.position.x, y, (int)CameraInstance.instance.tarTrs.position.z);
            }
            get
            {
                return ((int)(CameraInstance.instance.tarTrs.position.y/ MapManager.instance.dataCtrl.mainData.mapUnitSize.y) - ModSceneManager.instance.offset).ToString();
            }
        }
        public string viewZ
        {
            set
            {
                int.TryParse(value, out int v);
                var z = Mathf.Max(Mathf.Min(v+ ModSceneManager.instance.offset, (int)MapManager.instance.sizeLimit.z),0) * MapManager.instance.dataCtrl.mainData.mapUnitSize.z;
                ModSceneManager.instance.SetCamera((int)CameraInstance.instance.tarTrs.position.x, (int)CameraInstance.instance.tarTrs.position.y, z);

            }
            get
            {
                return ((int)(CameraInstance.instance.tarTrs.position.z/ MapManager.instance.dataCtrl.mainData.mapUnitSize.z) - ModSceneManager.instance.offset).ToString();
            }
        }
    }
        public partial class UiModSceneMainCtrl:IZ_Listener<CameraMoveEvent>
    {
        public override void OnCreate()
        {
            this.Register<CameraMoveEvent>();
            view.btn_menu.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModSceneMenuCtrl>();
            });
            view.btn_view.onClick.AddListener(() =>
            {
                ModSceneManager.instance.SetCamera(ModSceneManager.instance.offset * MapManager.instance.dataCtrl.mainData.mapUnitSize.x, ModSceneManager.instance.offset * MapManager.instance.dataCtrl.mainData.mapUnitSize.y, ModSceneManager.instance.offset * MapManager.instance.dataCtrl.mainData.mapUnitSize.z);
                Refresh();
            });
            view.ipt_viewPosSetX.onInput = (v) =>
            {
                model.viewX = v;
                Refresh();
            };
            view.ipt_viewPosSetY.onInput = (v) =>
            {
                model.viewY = v;
                Refresh();
            };
            view.ipt_viewPosSetZ.onInput = (v) =>
            {
                model.viewZ = v;
                Refresh();
            };

            Refresh();

        }

        public void OnEvent(CameraMoveEvent evt)
        {
            Refresh();
        }

        public void Refresh()
        {
                view.ipt_viewPosSetX.Set(model.viewX);
            
                view.ipt_viewPosSetY.Set(model.viewY);

                view.ipt_viewPosSetZ.Set(model.viewZ);
        }
    }
   
   
}
