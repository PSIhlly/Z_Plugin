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
using UnityEngine.UI;
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
    public partial class UiPlayMapCtrl : IZ_Listener<MissionEvent>
    {
        private const float AreaEdgePadding = 200f;
        public PlayMapController mapCtrl;
        private float lastRefreshTime;
        private bool forceAreaContentRefresh;
        private bool missionRefreshPending;
        private bool centerOnNextRefresh;
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
            missionRefreshPending = true;
            displayedSceneUid = 0;
            displayedHeight = int.MinValue;
            view.go_self.SetActive(false);
        }
        public override void OnShow()
        {
            this.Register<MissionEvent>();
            missionRefreshPending = true;
            centerOnNextRefresh = true;
            Refresh();
        }
        public override void OnHide()
        {
            this.Unregister<MissionEvent>();
        }
        public void OnEvent(MissionEvent evt)
        {
            if (active)
                missionRefreshPending = true;
        }
        public override void OnDisable()
        {
            centerOnNextRefresh = false;
            view.go_self.SetActive(false);
        }
        public override void OnUpdate()
        {
            Refresh();
        }
        public void Refresh()
        {
            if (!PlayManager.instance.enable || mapCtrl == null || !mapCtrl.isReady)
            {
                view.go_self.SetActive(false);
                return;
            }
            view.sta_type.ChangeState(model.isArea ? 0 : 1);
            if (!GameManager.instance.curProgress.enableMinimap || PlayManager.instance.sceneCtrl.playerM == null || PlayManager.instance.sceneCtrl.playerM.unit.belongTile == null)
            {
                view.go_self.SetActive(false);
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

                bool refreshContent = forceAreaContentRefresh || lastRefreshTime < GameManager.instance.curProgress.seconds;
                RefreshMissionIfNeeded(refreshContent);
                if (refreshContent)
                {
                    lastRefreshTime = GameManager.instance.curProgress.seconds + 0.3f;
                    forceAreaContentRefresh = false;

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
                foreach (var scene in scenes.OrderBy(data => data.uid))
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
            RefreshSelfMarker();
            CenterOnPlayerIfPending();
            view.btn_world.gameObject.SetActive(GameManager.instance.curProgress.enableLargeMap);
        }

        private void RefreshMissionIfNeeded(bool periodicRefresh)
        {
            if (!missionRefreshPending && !periodicRefresh)
                return;
            missionRefreshPending = false;
            missionCon.Clear();
            var mission = MissionForm.DataById.GetDv(GameManager.instance.curProgress.curMissionId, null);
            if (GameManager.instance.curProgress.enableMission && MissionGuide.IsActive(mission)
                && mapCtrl.curScene != null && mission.targetSceneId == mapCtrl.curScene.uid
                && GetPositionHeight(MissionGuide.GetTargetWorldPosition(mission)) == model.y)
                missionCon.Add(new UiMissionParam { data = mission });
            missionCon.Refresh();
        }

        private void CenterOnPlayerIfPending()
        {
            if (!centerOnNextRefresh || !view.go_self.activeSelf)
                return;

            var scroll = view.rtf_area.GetComponentInParent<ScrollRect>();
            if (scroll == null || scroll.content != view.rtf_area)
            {
                centerOnNextRefresh = false;
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scroll.transform);
            var viewport = scroll.viewport != null ? scroll.viewport : (RectTransform)scroll.transform;
            if (viewport.rect.width <= 0 || viewport.rect.height <= 0)
                return;

            // Allow even an edge Tile to reach the viewport center without elastic rebound.
            var horizontal = view.rtf_area.InverseTransformVector(viewport.TransformVector(
                new Vector3(viewport.rect.width, 0, 0)));
            var vertical = view.rtf_area.InverseTransformVector(viewport.TransformVector(
                new Vector3(0, viewport.rect.height, 0)));
            var padding = new Vector2(
                Mathf.Max(AreaEdgePadding, (Mathf.Abs(horizontal.x) + Mathf.Abs(vertical.x)) * .5f),
                Mathf.Max(AreaEdgePadding, (Mathf.Abs(horizontal.y) + Mathf.Abs(vertical.y)) * .5f));
            view.rtf_area.sizeDelta = new Vector2(mapCtrl.cols * mapCtrl.tileSize,
                mapCtrl.rows * mapCtrl.tileSize) + padding * 2;
            view.rimg_unlock.rectTransform.offsetMin = padding;
            view.rimg_unlock.rectTransform.offsetMax = -padding;
            RefreshSelfMarker();

            scroll.StopMovement();
            view.rtf_area.position += viewport.TransformPoint(viewport.rect.center) - view.rtf_self.position;
            centerOnNextRefresh = false;
        }

        private void RefreshSelfMarker()
        {
            var player = PlayManager.instance.sceneCtrl?.playerM;
            bool visible = model.isArea && mapCtrl != null && mapCtrl.curScene != null
                && player != null && player.unit.belongTile != null
                && player.unit.belongTile.data.mapPos.y == model.y;
            if (visible && mapCtrl.TryGetRelativePosition(player.pos, out var relativePos))
            {
                // Use the inner map rectangle, not the ScrollRect content's 200-pixel margins.
                var area = view.rimg_unlock.rectTransform;
                var bounds = area.rect;
                view.rtf_self.position = area.TransformPoint(new Vector3(
                    bounds.xMin + bounds.width * relativePos.x,
                    bounds.yMin + bounds.height * relativePos.y, 0));
            }
            else
                visible = false;
            view.go_self.SetActive(visible);
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
                var player = PlayManager.instance.sceneCtrl.playerG;
                if (player == null)
                    return;
                var heap = new Dictionary<string, BoxDataForm.Data>();
                heap["param1"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, (model.prm.unit).productInfo.Item1.ToString()));
                heap["param2"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, player.uid.ToString()));
                (model.prm.unit).ExecuteEvt("onClickMinimapEvent", heap);
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
            if (!parent.mapCtrl.TryGetRelativePosition(model.prm.pos, out var relativePos))
                return;
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
            if (!parent.mapCtrl.TryGetRelativePosition(MissionGuide.GetTargetWorldPosition(model.prm.data), out var relativePos))
                return;
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
