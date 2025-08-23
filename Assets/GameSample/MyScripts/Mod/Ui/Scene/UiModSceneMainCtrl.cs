using Form;
using System.Collections;
using System.Collections.Generic;
using Ui.ModSceneMenu;
using UnityEngine;
using Z_Map;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;

namespace Ui.ModSceneMain
{
    public partial class UiModSceneMainModel
    {
        public DesignType designType
        {
            set
            {
                ModManager.instance.sceneCtrl.designType = value;
            }
            get
            {
                return ModManager.instance.sceneCtrl.designType;
            }
        }
        public string viewX
        {
            set
            {
                int.TryParse(value, out int v);
                var x = Mathf.Max(Mathf.Min(v + ModManager.instance.sceneCtrl.offset, (int)MapManager.instance.sizeLimit.x), 0)*MapManager.instance.data.mainData.mapUnitSize.x;
                ModManager.instance.sceneCtrl.SetCamera( x, (int)CameraInstance.instance.tarTrs.position.y, (int)CameraInstance.instance.tarTrs.position.z);
            }
            get
            {
                return ((int)(CameraInstance.instance.tarTrs.position.x/ MapManager.instance.data.mainData.mapUnitSize.x) - ModManager.instance.sceneCtrl.offset).ToString();
            }
        }
        public string viewY
        {
            set
            {
                int.TryParse(value, out int v);
                var y = Mathf.Max(Mathf.Min(v+ ModManager.instance.sceneCtrl.offset, (int) MapManager.instance.sizeLimit.y), 0) * MapManager.instance.data.mainData.mapUnitSize.y;
                ModManager.instance.sceneCtrl.SetCamera((int)CameraInstance.instance.tarTrs.position.x, y, (int)CameraInstance.instance.tarTrs.position.z);
            }
            get
            {
                return ((int)(CameraInstance.instance.tarTrs.position.y/ MapManager.instance.data.mainData.mapUnitSize.y) - ModManager.instance.sceneCtrl.offset).ToString();
            }
        }
        public string viewZ
        {
            set
            {
                int.TryParse(value, out int v);
                var z = Mathf.Max(Mathf.Min(v+ ModManager.instance.sceneCtrl.offset, (int)MapManager.instance.sizeLimit.z),0) * MapManager.instance.data.mainData.mapUnitSize.z;
                ModManager.instance.sceneCtrl.SetCamera((int)CameraInstance.instance.tarTrs.position.x, (int)CameraInstance.instance.tarTrs.position.y, z);

            }
            get
            {
                return ((int)(CameraInstance.instance.tarTrs.position.z/ MapManager.instance.data.mainData.mapUnitSize.z) - ModManager.instance.sceneCtrl.offset).ToString();
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
                ModManager.instance.sceneCtrl.SetCamera(ModManager.instance.sceneCtrl.offset * MapManager.instance.data.mainData.mapUnitSize.x, ModManager.instance.sceneCtrl.offset * MapManager.instance.data.mainData.mapUnitSize.y, ModManager.instance.sceneCtrl.offset * MapManager.instance.data.mainData.mapUnitSize.z);
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
            view.btn_mapObject.onClick.AddListener(() =>
            {
                model.designType = DesignType.MapObject;
                Refresh();
            });
            view.btn_event.onClick.AddListener(() =>
            {
                model.designType = DesignType.Event;
                Refresh();
            });
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
            view.sta_mapObject.ChangeState(model.designType == DesignType.MapObject?1:0);
            view.sta_event.ChangeState(model.designType == DesignType.Event?1:0);
        }
    }
   
   
}
