using Form;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Map;
using Z_Map.Form;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui
{
    public partial class UiModToolModel
    {
        public bool show;
        public MapTypeForm.Data curType;
        public MapBaseForm.Data curData
        {
            set
            {
                ModSceneManager.instance.curData = value;
            }
            get
            {
                return ModSceneManager.instance.curData;
            }
        }
        public string cntX
        {
            set
            {
                int.TryParse(value, out int v);
                ModSceneManager.instance.cntX = v;
            }
            get
            {
                return ModSceneManager.instance.cntX.ToString();
            }
        }
        public string cntY
        {
            set
            {
                int.TryParse(value, out int v);
                ModSceneManager.instance.cntY = v;
            }
            get
            {
                return ModSceneManager.instance.cntY.ToString();
            }
        }
        public string posX
        {
            set
            {
                float.TryParse(value, out float v);
                ModSceneManager.instance.posX = v * MapManager.instance.dataCtrl.mainData.mapUnitSize.x;
            }
            get
            {
                return (ModSceneManager.instance.posX / MapManager.instance.dataCtrl.mainData.mapUnitSize.x).ToString("0.##");
            }
        }
        public string posY
        {
            set
            {
                float.TryParse(value, out float v);
                float minV = 0;
                float maxV = 0.9f;

                if (v < minV)
                {
                    v = minV;
                    NotifyManager.instance.AddTip(TextManager.instance.GetTxt("minYTip"));
                }
                if (v > maxV)
                {
                    v = maxV;
                    NotifyManager.instance.AddTip(TextManager.instance.GetTxt("maxYTip"));
                }
                ModSceneManager.instance.posY = v * MapManager.instance.dataCtrl.mainData.mapUnitSize.y;
            }
            get
            {
                return (ModSceneManager.instance.posY / MapManager.instance.dataCtrl.mainData.mapUnitSize.y).ToString("0.##");
            }
        }
        public string posZ
        {
            set
            {
                float.TryParse(value, out float v);
                ModSceneManager.instance.posZ = v * MapManager.instance.dataCtrl.mainData.mapUnitSize.z;
            }
            get
            {
                return (ModSceneManager.instance.posZ / MapManager.instance.dataCtrl.mainData.mapUnitSize.z).ToString("0.##");
            }
        }
        public string angle
        {
            set
            {
                int.TryParse(value, out int v);
                v = (v % 360 + 360) % 360;
                ModSceneManager.instance.angle = v;
            }
            get
            {
                return ModSceneManager.instance.angle.ToString();
            }
        }
        public bool posing
        {
            set
            {

                ModSceneManager.instance.posing = value;
            }
            get
            {
                return ModSceneManager.instance.posing;
            }
        }
        public int layer
        {
            set
            {
                ModSceneManager.instance.layer = value;
            }
            get
            {
                return ModSceneManager.instance.layer;
            }
        }
    }

    public partial class UiModToolCtrl
    {
        private Controller aniCon;
        UiScrViewContainer<UiToolItemCtrl> conData;
        UiScrViewContainer<UiToolTypeItemCtrl> conType;


        public override void OnCreate()
        {
            conData = new UiScrViewContainer<UiToolItemCtrl>(view.go_toolItem, view.scr_tool);
            conType = new UiScrViewContainer<UiToolTypeItemCtrl>(view.go_toolTypeItem, view.scr_toolType);

            var to = parent.view.go_toolPos.transform.position - view.go_toolContentPos.transform.position;
            var showAct = new Action(uiHolder, new PositionSetEvent(uiHolder.transform, uiHolder.transform.position, uiHolder.transform.position + to, 0.2f));
            var hideAct = new Action(uiHolder, new PositionSetEvent(uiHolder.transform, uiHolder.transform.position + to, uiHolder.transform.position, 0.2f));

            var showGroup = new ActionGroup(showAct, StopType.MoveToEnd, false);
            var hideGroup = new ActionGroup(hideAct, StopType.MoveToEnd, false);
            var groups = new List<ActionGroup>() { showGroup, hideGroup };
            aniCon = new Controller(groups);

            view.btn_showTool.onClick.AddListener(() =>
            {
                if (model.show)
                {
                    aniCon.Stop(0);
                    if (aniCon.GetState(1) != GroupState.Playing)
                    {
                        aniCon.Play(1, () =>
                        {
                            model.show = false;
                            SetCurData(null);
                        });
                    }
                }
                else
                {
                    aniCon.Stop(1);
                    if (aniCon.GetState(0) != GroupState.Playing)
                    {
                        aniCon.Play(0, () =>
                        {
                            model.show = true;
                        });
                    }
                }
            });

            view.btn_cnt.onClick.AddListener(() =>
            {
                model.cntX = "1";
                model.cntY = "1";
                Refresh();

            });
            view.btn_pos.onClick.AddListener(() =>
            {
                model.posX = "0";
                model.posY = "0";
                model.posZ = "0";
                Refresh();

            });
            view.btn_rotate.onClick.AddListener(() =>
            {
                model.angle = ((int.Parse(model.angle) + 90)).ToString();
                Refresh();
            });

            view.ipt_cntSetX.onInput = ((v) =>
            {
                model.cntX = v;
                Refresh();
            });
            view.ipt_cntSetY.onInput = ((v) =>
            {
                model.cntY = v;
                Refresh();
            });
            view.ipt_posSetX.onInput = ((v) =>
            {
                model.posX = v;
                Refresh();
            });
            view.ipt_posSetY.onInput = ((v) =>
            {
                model.posY = v;
                Refresh();
            });
            view.ipt_posSetY.onDeselect.AddListener((v) =>
            {
                Refresh();
            });

            view.ipt_posSetZ.onInput = ((v) =>
            {
                model.posZ = v;
                Refresh();
            });
            view.ipt_rotateSet.onInput = ((v) =>
              {
                  model.angle = v;
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
        }
        public override void OnShow()
        {
            model.curType = MapTypeForm.DataById[1];
            Refresh();

        }
        public void Refresh()
        {
            conType.Clear();

            foreach (var data in MapTypeForm.DataById.Values)
            {
                conType.Add(new UiToolTypeItemParam()
                {
                    data = data
                });
            }
            conType.Refresh();


            conData.Clear();
            switch (model.curType.id)
            {
                case 2:
                    {
                        foreach (var data in MapTextureForm.DataById.Values)
                        {
                            conData.Add(new UiToolItemParam()
                            {
                                data = data
                            });
                        }
                    }
                    break;
                case 3:
                    {
                        foreach (var data in MapTransitionMaskForm.DataById.Values)
                        {
                            conData.Add(new UiToolItemParam()
                            {
                                data = data
                            });
                        }
                    }
                    break;
                case 4:
                    {
                        foreach (var data in MapObstacleForm.DataById.Values)
                        {
                            conData.Add(new UiToolItemParam()
                            {
                                data = data
                            });
                        }
                    }
                    break;
                case 100:
                    {
                        foreach (var data in MapEraseForm.DataById.Values)
                        {
                            conData.Add(new UiToolItemParam()
                            {
                                data = data
                            });
                        }
                    }
                    break;
                case 1:
                default:
                    {
                        foreach (var data in MapTerrainForm.DataById.Values)
                        {
                            conData.Add(new UiToolItemParam()
                            {
                                data = data
                            });
                        }
                    }
                    break;
            }

            conData.Refresh();
            view.sta_pos.ChangeState(model.posing ? 1 : 0);
            view.ipt_cntSetX.Set(model.cntX);
            view.ipt_cntSetY.Set(model.cntY);
            view.ipt_posSetX.Set(model.posX);
            view.ipt_posSetY.Set(model.posY);
            view.ipt_posSetZ.Set(model.posZ);
            view.ipt_rotateSet.Set(model.angle);

            view.txt_textureLayerSet.gameObject.SetActive(model.curType.needLayer);
            view.sta_layer0.ChangeState(model.layer == 0 ? 1 : 0);
            view.sta_layer1.ChangeState(model.layer == 1 ? 1 : 0);
            view.sta_layer2.ChangeState(model.layer == 2 ? 1 : 0);

        }
        public void SetCurType(MapTypeForm.Data type)
        {
            model.curType = type;
            Refresh();
        }
        public void SetCurData(MapBaseForm.Data data)
        {
            model.curData = data;
            Refresh();
        }
    }
    public partial class UiToolItemModel
    {
        public MapBaseForm.Data data;
    }
    public partial class UiToolItemParam
    {
        public MapBaseForm.Data data;
    }
    public partial class UiToolItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_tool.onClick.AddListener(() =>
            {
                if (parent.model.curData == model.data)
                {
                    parent.SetCurData(null);

                }
                else
                {
                    parent.SetCurData(model.data);

                }
            });
        }
        public override void OnShow()
        {
            model.data = param.data;

            view.txt_name.text = model.data.name;
            view.img_.sprite = TextureHelper.GetSpriteByPath(model.data.icon);
            
            if (parent.model.curData == model.data)
            {
                view.sta_tool.ChangeState(1);
            }
            else
            {
                view.sta_tool.ChangeState(0);
            }
        }


    }



    public partial class UiToolTypeItemModel
    {
        public MapTypeForm.Data data;
    }
    public partial class UiToolTypeItemParam
    {
        public MapTypeForm.Data data;
    }
    public partial class UiToolTypeItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_tool.onClick.AddListener(() =>
            {
                parent.SetCurType(model.data);
            });
        }
        public override void OnShow()
        {
            model.data = param.data;

            view.txt_name.text = TextManager.instance.GetTxt(model.data.NameKey);
            view.img_.sprite = TextureHelper.GetSpriteByPath(model.data.icon);

            if (parent.model.curType == model.data)
            {
                view.sta_tool.ChangeState(1);
            }
            else
            {
                view.sta_tool.ChangeState(0);
            }
        }


    }

}
