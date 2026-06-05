using Form;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;
using Z_Ui.Base;
using Z_UnitSystem.Form;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Ui.PlayMap
{

    public partial class UiPlayMapParam
    {

    }
    public partial class UiPlayMapModel
    {
        public int cur = 0;
        public int y = 0;
        public bool isArea;
    }
    public partial class UiPlayMapCtrl
    {
        public PlayMapController mapCtrl;
        private float lastRefreshTime;
        UiContainer<UiMarkCtrl> markCon;
        UiContainer<UiSceneCtrl> sceneCon;
        UiContainer<UiMissionCtrl> missionCon;
        public override void OnCreate()
        {
            markCon = new UiContainer<UiMarkCtrl>(this, view.go_mark);
            sceneCon = new UiContainer<UiSceneCtrl>(this, view.go_scene);
            missionCon = new UiContainer<UiMissionCtrl>(this, view.go_mission);
            view.btn_world.onClick.AddListener(() =>
            {
                model.isArea = false;
                Refresh();
            });
            view.btn_area.onClick.AddListener(() =>
            {
                model.isArea = true;
                Refresh();
            });
            view.btn_bg.onClick.AddListener(() => {
                Close();
            });
        }
        public override void OnEnable()
        {
            model.isArea = true;
            mapCtrl = PlayManager.instance.mapCtrl;
            model.cur = 0;
            model.y = 0;
        }
        public override void OnDisable()
        {
        }
        public override void OnUpdate()
        {
            Refresh();
        }
        public void Refresh()
        {
            view.sta_type.ChangeState(model.isArea ? 0 : 1);
            if (!GameManager.instance.curProgress.enableMinimap || PlayManager.instance.sceneCtrl.playerM == null || PlayManager.instance.sceneCtrl.playerM.unit.belongTile == null)
            {
                Close();
                return;
            }
            if (model.isArea)
            {
                int y = PlayManager.instance.sceneCtrl.playerM.unit.belongTile.data.mapPos.y;
                bool hasMinimap = PlayManager.instance.mapCtrl.HasMinimap();
                if (mapCtrl.curScene != null)
                {
                    if (model.cur != mapCtrl.curScene.uid)
                    {
                        model.cur = mapCtrl.curScene.uid;

                        if (!hasMinimap)
                        {
                            model.y = y;
                            view.img_real.sprite = mapCtrl.heightMap.GetDv(y);
                        }
                        else
                        {
                            view.img_real.sprite = StoryTexAssetForm.DataByName[mapCtrl.curScene.miniMap].GetSprite();
                        }
                        view.rimg_unlock.texture = mapCtrl.unlockTextureMap.GetDv(y);
                        view.rtf_area.sizeDelta = new UnityEngine.Vector2(mapCtrl.cols * mapCtrl.tileSize, mapCtrl.rows * mapCtrl.tileSize);
                    }

                    if (!hasMinimap && model.y != y)
                    {
                        model.y = y;
                        view.img_real.sprite = mapCtrl.heightMap.GetDv(y);
                        view.rimg_unlock.texture = mapCtrl.unlockTextureMap.GetDv(y);
                    }
                }
                var pos = PlayManager.instance.sceneCtrl.GetPlayerPos();
                
                if (lastRefreshTime < GameManager.instance.curProgress.seconds)
                {
                    lastRefreshTime = GameManager.instance.curProgress.seconds + 0.3f;

                    missionCon.Clear();
                    var missionData = MissionForm.DataById.GetDv(GameManager.instance.curProgress.curMissionId, null);
                    if (missionData == null)
                    {
                        missionCon.Add(new UiMissionParam()
                        {
                            data = missionData
                        });
                    }
                    missionCon.Refresh();




                    markCon.Clear();
                    var marks = PlayManager.instance.mapCtrl.dic;
                    foreach (var mark in marks)
                    {
                        var data = UnitForm.DataByUid.GetDv(mark.Key, null);
                        if(data!=null)
                        {
                            markCon.Add(new UiMarkParam()
                            {
                                pos = mark.Value.Item1,
                                icon = mark.Value.Item2,
                                unit = (MapUnit)data.unit
                        });
                        }
                        
                    }
                    markCon.Refresh();

                  
                }
            }
            else
            {
                view.img_largeMap.sprite = StoryTexAssetForm.DataByName.GetDk(GameManager.instance.curProgress.largeMap, GlobalNameHelper.GetDefaultTexName()).GetSprite();
                sceneCon.Clear();
                var scenes = SceneForm.DataByUid.Values;
                foreach (var scene in scenes)
                {
                    if (scene.unlock&&!scene.hideInLargeMap)
                    {
                        sceneCon.Add(new UiSceneParam()
                        {
                            data = scene
                        });
                    }
                }
                sceneCon.Refresh();
            }
            view.btn_world.gameObject.SetActive(GameManager.instance.curProgress.enableLargeMap);
        }

    }
    public partial class UiMarkParam
    {
        public MapUnit unit;
        public Vector3 pos;
        public Sprite icon;
    }
    public partial class UiMarkModel
    {
        public UiMarkParam prm;
    }
    public partial class UiMarkCtrl
    {
        public override void OnCreate()
        {
            view.btn_mark.onClick.AddListener(() =>
            {
                var heap = new Dictionary<string, BoxDataForm.Data>();
                heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, (model.prm.unit).productInfo.Item1.ToString()));
                heap["target"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, PlayManager.instance.sceneCtrl.playerG.uid.ToString()));
                (model.prm.unit).ExecuteEvt("onClickMinimapEvent", null);
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }

        public void Refresh()
        {
            view.img_mark.sprite = model.prm.icon;
            var relativePos = new Vector2((model.prm.pos.x - parent.mapCtrl.size.Item3) / (parent.mapCtrl.size.Item4 - parent.mapCtrl.size.Item3), (model.prm.pos.z - parent.mapCtrl.size.Item2) / (parent.mapCtrl.size.Item1 - parent.mapCtrl.size.Item2));
            view.go_mark.transform.position = Z_Math.Graph.GetRealPos(relativePos, parent.view.rtf_area);
            
        }
    }

    public partial class UiMissionParam
    {
        public MissionForm.Data data;
    }
    public partial class UiMissionModel
    {
        public UiMissionParam prm;
    }
    public partial class UiMissionCtrl
    {
        Timer timer;
        public override void OnCreate()
        {
            view.btn_mission.onClick.AddListener(() =>
            {
                TimeManager.instance.CancelTimer(timer);
                view.txt_.text = model.prm.data.name;
                TimeManager.instance.StartTimer(5, 0, () =>
                {
                    view.txt_.text = "";
                    return true;
                },uiHolder);
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public override void OnDisable()
        {
            TimeManager.instance.CancelTimer(timer);
            view.txt_.text="";
        }

        public void Refresh()
        {
            var relativePos = new Vector2((model.prm.data.targetPos.x - parent.mapCtrl.size.Item3) / (parent.mapCtrl.size.Item4 - parent.mapCtrl.size.Item3), (model.prm.data.targetPos.z - parent.mapCtrl.size.Item2) / (parent.mapCtrl.size.Item1 - parent.mapCtrl.size.Item2));
            view.go_mission.transform.position = Z_Math.Graph.GetRealPos(relativePos, parent.view.rtf_area);
            view.rtf_area.sizeDelta.Set(model.prm.data.radius*2, model.prm.data.radius*2);
            
        }
    }

    public partial class UiSceneParam
    {
        public SceneForm.Data data;
    }
    public partial class UiSceneModel
    {
        public UiSceneParam prm;
    }
    public partial class UiSceneCtrl
    {
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }

        public void Refresh()
        {
            view.txt_.text = model.prm.data.name;
            view.go_scene.transform.position = Z_Math.Graph.GetRealPos(model.prm.data.pos, parent.view.rtf_world);
        }
    }
}