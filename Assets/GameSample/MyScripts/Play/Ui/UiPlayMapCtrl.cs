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
        private const float AreaEdgePadding = 200f;
        public PlayMapController mapCtrl;
        private float lastRefreshTime;
        private bool forceAreaContentRefresh;
        private int displayedSceneUid;
        private int displayedHeight = int.MinValue;
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
                forceAreaContentRefresh = true;
                Refresh();
            });
            view.btn_up.onClick.AddListener(() => ChangeHeight(1));
            view.btn_down.onClick.AddListener(() => ChangeHeight(-1));
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
            lastRefreshTime = 0;
            forceAreaContentRefresh = true;
            displayedSceneUid = 0;
            displayedHeight = int.MinValue;
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
                int playerHeight = PlayManager.instance.sceneCtrl.playerM.unit.belongTile.data.mapPos.y;
                bool hasMinimap = PlayManager.instance.mapCtrl.HasMinimap();
                if (mapCtrl.curScene != null)
                {
                    if (model.cur != mapCtrl.curScene.uid)
                    {
                        model.cur = mapCtrl.curScene.uid;
                        model.y = GetClosestHeight(playerHeight);
                        view.rtf_area.sizeDelta = new Vector2(
                            mapCtrl.cols * mapCtrl.tileSize + AreaEdgePadding * 2,
                            mapCtrl.rows * mapCtrl.tileSize + AreaEdgePadding * 2);
                        view.rimg_unlock.rectTransform.offsetMin = Vector2.one * AreaEdgePadding;
                        view.rimg_unlock.rectTransform.offsetMax = Vector2.one * -AreaEdgePadding;
                        forceAreaContentRefresh = true;
                    }

                    EnsureCurrentHeight(playerHeight);
                    RefreshHeightView(hasMinimap);
                }

                if (forceAreaContentRefresh || lastRefreshTime < GameManager.instance.curProgress.seconds)
                {
                    lastRefreshTime = GameManager.instance.curProgress.seconds + 0.3f;
                    forceAreaContentRefresh = false;

                    missionCon.Clear();
                    var missionData = MissionForm.DataById.GetDv(GameManager.instance.curProgress.curMissionId, null);
                    if (missionData != null && GetPositionHeight(missionData.targetPos) == model.y)
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
                        if (data != null && data.unit is MapUnit unit && GetUnitHeight(unit, mark.Value.Item1) == model.y)
                        {
                            markCon.Add(new UiMarkParam()
                            {
                                pos = mark.Value.Item1,
                                icon = mark.Value.Item2,
                                unit = unit
                            });
                        }
                    }
                    markCon.Refresh();
                }
            }
            else
            {
                SetHeightControlsVisible(false, false, false);
                view.img_largeMap.BindTexData(StoryTexAssetForm.DataById.GetDv(GameManager.instance.curProgress.largeMap, StoryTexAssetForm.DataById[GlobalDefaultHelper.DefaultTexId]));
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

        private List<int> GetHeights()
        {
            return mapCtrl.heightMap.Keys.OrderBy(height => height).ToList();
        }

        private int GetClosestHeight(int targetHeight)
        {
            var heights = GetHeights();
            if (heights.Count == 0)
                return targetHeight;

            int closest = heights[0];
            int closestDistance = Math.Abs(closest - targetHeight);
            for (int i = 1; i < heights.Count; i++)
            {
                int distance = Math.Abs(heights[i] - targetHeight);
                if (distance < closestDistance)
                {
                    closest = heights[i];
                    closestDistance = distance;
                }
            }
            return closest;
        }

        private void EnsureCurrentHeight(int fallbackHeight)
        {
            if (!mapCtrl.heightMap.ContainsKey(model.y))
                model.y = GetClosestHeight(fallbackHeight);
        }

        private void ChangeHeight(int direction)
        {
            if (!model.isArea || mapCtrl == null)
                return;

            var heights = GetHeights();
            int index = heights.IndexOf(model.y);
            if (index < 0)
                return;

            int nextIndex = index + direction;
            if (nextIndex < 0 || nextIndex >= heights.Count)
                return;

            model.y = heights[nextIndex];
            forceAreaContentRefresh = true;
            Refresh();
        }

        private void RefreshHeightView(bool hasMinimap)
        {
            var heights = GetHeights();
            int index = heights.IndexOf(model.y);
            bool hasHeight = index >= 0;
            SetHeightControlsVisible(hasHeight, hasHeight && index < heights.Count - 1, hasHeight && index > 0);
            if (!hasHeight)
                return;

            view.txt_curHeight.text = GameManager.MapPosToPlayerPos(model.y).ToString("0.##");
            if (displayedSceneUid == model.cur && displayedHeight == model.y)
                return;

            if (hasMinimap)
                view.img_real.BindTexData(StoryTexAssetForm.DataById[mapCtrl.curScene.miniMap]);
            else
                view.img_real.sprite = mapCtrl.heightMap.GetDv(model.y);
            view.rimg_unlock.texture = mapCtrl.unlockTextureMap.GetDv(model.y);
            displayedSceneUid = model.cur;
            displayedHeight = model.y;
        }

        private void SetHeightControlsVisible(bool showText, bool showUp, bool showDown)
        {
            view.txt_curHeight.gameObject.SetActive(showText);
            view.btn_up.gameObject.SetActive(showUp);
            view.btn_down.gameObject.SetActive(showDown);
        }

        private static int GetPositionHeight(Vector3 position)
        {
            return MapManager.instance.utilCtrl.RealPos2MapPosInt(position).y;
        }

        private static int GetUnitHeight(MapUnit unit, Vector3 position)
        {
            return unit.belongTile != null ? unit.belongTile.data.mapPos.y : GetPositionHeight(position);
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
            view.go_mark.transform.position = Z_Math.Graph.GetRealPos(relativePos, parent.view.rimg_unlock.rectTransform);
            
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
            view.go_mission.transform.position = Z_Math.Graph.GetRealPos(relativePos, parent.view.rimg_unlock.rectTransform);
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
