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
        public int layer
        {
            set
            {
                ModManager.instance.sceneCtrl.layer = value;
            }
            get
            {
                return ModManager.instance.sceneCtrl.layer;
            }
        }
        public string viewX
        {
            set
            {
                float.TryParse(value, out float v);
                var x = Mathf.Clamp(GameManager.PlayerPosToMapPos(v), 0, MapManager.instance.utilCtrl.MapPos2RealPos((Vector3)MapManager.instance.sizeLimit).x);
                ModManager.instance.sceneCtrl.SetCamera(x, CameraInstance.instance.tarTrs.position.y, CameraInstance.instance.tarTrs.position.z);
            }
            get
            {
                return GameManager.MapPosToPlayerPos(new Vector3(CameraInstance.instance.tarTrs.position.x, 0, 0)).x.ToString("0.#");
            }
        }
        public string viewY
        {
            set
            {
                float.TryParse(value, out float v);
                var y = Mathf.Clamp(GameManager.PlayerPosToMapPos(v), 0, MapManager.instance.utilCtrl.MapPos2RealPos((Vector3)MapManager.instance.sizeLimit).y);
                ModManager.instance.sceneCtrl.SetCamera(CameraInstance.instance.tarTrs.position.x, y, CameraInstance.instance.tarTrs.position.z);
            }
            get
            {
                return GameManager.MapPosToPlayerPos(new Vector3(0, CameraInstance.instance.tarTrs.position.y, 0)).y.ToString("0.#");
            }
        }
        public string viewZ
        {
            set
            {
                float.TryParse(value, out float v);
                var z = Mathf.Clamp(GameManager.PlayerPosToMapPos(v), 0, MapManager.instance.utilCtrl.MapPos2RealPos((Vector3)MapManager.instance.sizeLimit).z);
                ModManager.instance.sceneCtrl.SetCamera(CameraInstance.instance.tarTrs.position.x, CameraInstance.instance.tarTrs.position.y, z);

            }
            get
            {
                return GameManager.MapPosToPlayerPos(new Vector3(0, 0, CameraInstance.instance.tarTrs.position.z)).z.ToString("0.#");
            }
        }
    }
    public partial class UiModSceneMainCtrl : IZ_Listener<CameraMoveEvent>
    {
        public override void OnCreate()
        {
            this.Register<CameraMoveEvent>();
            view.btn_undo.onClick.AddListener(() => ModManager.instance.sceneCtrl.Undo());
            view.btn_redo.onClick.AddListener(() => ModManager.instance.sceneCtrl.Redo());
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
                var pos = GameManager.PlayerPosToMapPos(Vector3.zero);
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
            view.btn_event.onClick.AddListener(() =>
            {
                model.designType = DesignType.Event;
                Refresh();
            });
            view.btn_layer0.onClick.AddListener(() =>
            {
                model.layer = 0;
                Refresh();
            });
            view.btn_layer1.onClick.AddListener(() =>
            {
                model.layer = 1;
                Refresh();
            });
            view.btn_layer2.onClick.AddListener(() =>
            {
                model.layer = 2;
                Refresh();
            });
            Refresh();

        }
        public override void OnShow()
        {
            model.designType = DesignType.MapObject;
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
            view.sta_event.ChangeState(model.designType == DesignType.Event ? 1 : 0);

            bool editingLayer = model.designType == DesignType.MapObject;
            view.sta_layer0.ChangeState(editingLayer && model.layer == 0 ? 1 : 0);
            view.sta_layer1.ChangeState(editingLayer && model.layer == 1 ? 1 : 0);
            view.sta_layer2.ChangeState(editingLayer && model.layer == 2 ? 1 : 0);
        }
    }


}
