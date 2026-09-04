using Form;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ui.CmdInputArea;
using Ui.ModSceneMenu;
using UnityEngine;
using Z_Map;
using Z_Math;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

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
        public int tileLayerDisplayMode
        {
            set
            {
                ModManager.instance.sceneCtrl.tileLayerDisplayMode = value;
            }
            get
            {
                return ModManager.instance.sceneCtrl.tileLayerDisplayMode;
            }
        }
        public string viewX
        {
            set
            {
                int.TryParse(value, out int v);
                var x = Mathf.Max(Mathf.Min(GameManager.PlayerPosToMapPos(v), (int)MapManager.instance.sizeLimit.x), 0) * MapManager.instance.data.mainData.mapUnitSize.x;
                ModManager.instance.sceneCtrl.SetCamera(x, (int)CameraInstance.instance.tarTrs.position.y, (int)CameraInstance.instance.tarTrs.position.z);
            }
            get
            {
                return GameManager.MapPosToPlayerPos(MapManager.instance.utilCtrl.RealPos2MapPos(new Vector3(CameraInstance.instance.tarTrs.position.x, 0, 0))).x.ToString("0.#");
            }
        }
        public string viewY
        {
            set
            {
                int.TryParse(value, out int v);
                var y = Mathf.Max(Mathf.Min(GameManager.PlayerPosToMapPos(v), (int)MapManager.instance.sizeLimit.y), 0) * MapManager.instance.data.mainData.mapUnitSize.y;
                ModManager.instance.sceneCtrl.SetCamera((int)CameraInstance.instance.tarTrs.position.x, y, (int)CameraInstance.instance.tarTrs.position.z);
            }
            get
            {
                return GameManager.MapPosToPlayerPos(MapManager.instance.utilCtrl.RealPos2MapPos(new Vector3(0, CameraInstance.instance.tarTrs.position.y, 0))).y.ToString("0.#");
            }
        }
        public string viewZ
        {
            set
            {
                int.TryParse(value, out int v);
                var z = Mathf.Max(Mathf.Min(GameManager.PlayerPosToMapPos(v), (int)MapManager.instance.sizeLimit.z), 0) * MapManager.instance.data.mainData.mapUnitSize.z;
                ModManager.instance.sceneCtrl.SetCamera((int)CameraInstance.instance.tarTrs.position.x, (int)CameraInstance.instance.tarTrs.position.y, z);

            }
            get
            {
                return GameManager.MapPosToPlayerPos(MapManager.instance.utilCtrl.RealPos2MapPos(new Vector3(0, 0, CameraInstance.instance.tarTrs.position.z))).z.ToString("0.#");
            }
        }
    }
    public partial class UiModSceneMainCtrl : IZ_Listener<CameraMoveEvent>
    {
        public override void OnCreate()
        {
            this.Register<CameraMoveEvent>();
            view.btn_menu.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModSceneMenuCtrl>();
            });
            view.btn_modCmd.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiCmdInputAreaCtrl>(new UiCmdInputAreaParam
                {
                    scope = ModCmdScope.Scene,
                    onSuccess = Refresh
                });
            });
            view.btn_view.onClick.AddListener(() =>
            {
                var pos = MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(Vector3.zero));
                ModManager.instance.sceneCtrl.SetCamera(pos.x, pos.y, pos.z);
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
            view.btn_layer0.onClick.AddListener(() =>
            {
                model.tileLayerDisplayMode = 0;
                Refresh();
            });
            view.btn_layer1.onClick.AddListener(() =>
            {
                model.tileLayerDisplayMode = 1;
                Refresh();
            });
            view.btn_layer2.onClick.AddListener(() =>
            {
                model.tileLayerDisplayMode = 2;
                Refresh();
            });
            Refresh();

        }
        public override void OnShow()
        {
            model.designType = DesignType.MapObject;

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
            view.sta_mapObject.ChangeState(model.designType == DesignType.MapObject ? 1 : 0);
            view.sta_event.ChangeState(model.designType == DesignType.Event ? 1 : 0);

            var mode = model.tileLayerDisplayMode;
            view.sta_layer0.ChangeState(mode == 0 ? 1 : 0);
            view.sta_layer1.ChangeState(mode == 1 ? 1 : 0);
            view.sta_layer2.ChangeState(mode >= 2 ? 1 : 0);
        }
    }


}
