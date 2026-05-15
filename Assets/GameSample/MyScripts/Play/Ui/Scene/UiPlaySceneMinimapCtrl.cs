using Form;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Ui.PlayMap;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using static UnityEngine.Rendering.DebugUI.Table;


namespace Ui.PlaySceneMain.PlaySceneMinimap
{

    public partial class UiPlaySceneMinimapParam
    {

    }
    public partial class UiPlaySceneMinimapModel
    {
        public int cur = 0;
        public int y = 0;
    }
    public partial class UiPlaySceneMinimapCtrl
    {
        public PlayMapController mapCtrl;
        private float lastRefreshTime;
        UiContainer<UiMarkCtrl> con;
        UiContainer<UiMissionCtrl> missionCon;
        public override void OnCreate()
        {
            con = new UiContainer<UiMarkCtrl>(this, view.go_mark);
            missionCon = new UiContainer<UiMissionCtrl>(this, view.go_mission);
            view.btn_map.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiPlayMapCtrl>();
            });
        }
        public override void OnEnable()
        {
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
            gameObject.SetActive(GameManager.instance.curProgress.enableMinimap);
            if (!GameManager.instance.curProgress.enableMinimap || PlayManager.instance.sceneCtrl.playerM == null || PlayManager.instance.sceneCtrl.playerM.unit.belongTile == null)
            {
                return;
            }

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
            var relativePos = new Vector2((pos.x - mapCtrl.size.Item3) / (mapCtrl.size.Item4 - mapCtrl.size.Item3) - 0.5f, (pos.z - mapCtrl.size.Item2) / (mapCtrl.size.Item1 - mapCtrl.size.Item2) - 0.5f);
            view.rtf_area.localPosition = -new Vector3(relativePos.x * mapCtrl.cols * mapCtrl.tileSize, relativePos.y * mapCtrl.rows * mapCtrl.tileSize, 0);

            if (lastRefreshTime < GameManager.instance.curProgress.seconds)
            {
                lastRefreshTime = GameManager.instance.curProgress.seconds + 0.3f;
                missionCon.Clear();
                view.rtf_guide.gameObject.SetActive(false);
                var missionData = MissionForm.DataById.GetDv(GameManager.instance.curProgress.curMissionId, null);
                if (missionData != null)
                {
                    missionCon.Add(new UiMissionParam()
                    {
                        data = missionData
                    });
                }
                missionCon.Refresh();

                con.Clear();
                var marks = PlayManager.instance.mapCtrl.dic.Values;
                foreach (var mark in marks)
                {
                    con.Add(new UiMarkParam()
                    {
                        pos = mark.Item1,
                        icon = mark.Item2
                    });
                }
                con.Refresh();



            }




        }

    }
    public partial class UiMarkParam
    {
        public Vector3 pos;
        public Sprite icon;
    }
    public partial class UiMarkModel
    {
        public UiMarkParam prm;
    }
    public partial class UiMarkCtrl
    {
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
        public override void OnCreate()
        {
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }

        public void Refresh()
        {
            var relativePos = new Vector2((model.prm.data.targetPos.x - parent.mapCtrl.size.Item3) / (parent.mapCtrl.size.Item4 - parent.mapCtrl.size.Item3), (model.prm.data.targetPos.z - parent.mapCtrl.size.Item2) / (parent.mapCtrl.size.Item1 - parent.mapCtrl.size.Item2));
            view.go_mission.transform.position = Z_Math.Graph.GetRealPos(relativePos, parent.view.rtf_area);
            view.rtf_area.sizeDelta.Set(model.prm.data.radius * 2, model.prm.data.radius * 2);
            if(GameManager.instance.curProgress.curMissionId == model.prm.data.id )
            {
            Vector2 center = parent.view.btn_map.transform.position;
            float diameter = parent.view.btn_map.image.rectTransform.rect.width;
            float radius = diameter / 2;
            Vector2 missionPos = view.go_mission.transform.position;
            Vector2 direction = missionPos - center;
            float distance = direction.magnitude;

            if (distance > radius)
            {
                parent.view.rtf_guide.gameObject.SetActive(true);
                Vector2 guidePos = center + direction.normalized * radius;
                parent.view.rtf_guide.position = guidePos;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                parent.view.rtf_guide.eulerAngles = new Vector3(0, 0, angle - 90);
            }
            }
            
        }
    }

}