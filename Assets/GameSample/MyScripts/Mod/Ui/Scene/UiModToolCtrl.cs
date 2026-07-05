using Form;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.ModSceneMain.ModTool
{
    public partial class UiModToolModel
    {
        public bool show;
        public MapTypeForm.Data curType;
        public string curLab;
        public MapBaseForm.Data curData
        {
            set
            {
                ModManager.instance.sceneCtrl.curData = value;
            }
            get
            {
                return ModManager.instance.sceneCtrl.curData;
            }
        }
        public string cntX
        {
            set
            {
                int.TryParse(value, out int v);
                ModManager.instance.sceneCtrl.cntX = v;
            }
            get
            {
                return ModManager.instance.sceneCtrl.cntX.ToString();
            }
        }
        public string cntY
        {
            set
            {
                int.TryParse(value, out int v);
                ModManager.instance.sceneCtrl.cntY = v;
            }
            get
            {
                return ModManager.instance.sceneCtrl.cntY.ToString();
            }
        }
        public string posX
        {
            set
            {
                float.TryParse(value, out float v);
                ModManager.instance.sceneCtrl.posX = v * MapManager.instance.data.mainData.mapUnitSize.x;
            }
            get
            {
                return (ModManager.instance.sceneCtrl.posX / MapManager.instance.data.mainData.mapUnitSize.x).ToString("0.##");
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
                ModManager.instance.sceneCtrl.posY = v * MapManager.instance.data.mainData.mapUnitSize.y;
            }
            get
            {
                return (ModManager.instance.sceneCtrl.posY / MapManager.instance.data.mainData.mapUnitSize.y).ToString("0.##");
            }
        }
        public string posZ
        {
            set
            {
                float.TryParse(value, out float v);
                ModManager.instance.sceneCtrl.posZ = v * MapManager.instance.data.mainData.mapUnitSize.z;
            }
            get
            {
                return (ModManager.instance.sceneCtrl.posZ / MapManager.instance.data.mainData.mapUnitSize.z).ToString("0.##");
            }
        }
        public string angle
        {
            set
            {
                int.TryParse(value, out int v);
                v = (v % 360 + 360) % 360;
                ModManager.instance.sceneCtrl.angle = v;
            }
            get
            {
                return ModManager.instance.sceneCtrl.angle.ToString();
            }
        }
        public bool posing
        {
            set
            {

                ModManager.instance.sceneCtrl.posing = value;
            }
            get
            {
                return ModManager.instance.sceneCtrl.posing;
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
    }

    public partial class UiModToolCtrl
    {
        private Controller aniCon;
        UiScrViewContainer<UiToolItemCtrl> conData;
        UiScrViewContainer<UiToolTypeItemCtrl> conType;
        UiScrViewContainer<UiLabCtrl> conLab;


        public override void OnCreate()
        {
            conData = new UiScrViewContainer<UiToolItemCtrl>(this, view.go_toolItem, view.scr_tool);
            conType = new UiScrViewContainer<UiToolTypeItemCtrl>(this, view.go_toolTypeItem, view.scr_toolType);
            conLab = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);

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

            view.btn_resetCount.onClick.AddListener(() =>
            {
                model.cntX = "1";
                model.cntY = "1";
                Refresh();

            });
            view.btn_align.onClick.AddListener(() =>
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
            model.curLab = null;
            model.curData = null;
            Refresh();

        }
        public void Refresh()
        {
            RefreshLabs();
            conType.Clear();

            foreach (var data in MapTypeForm.DataById.Values)
            {
                if (data.id == 3)
                    continue;
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
                            if ((model.curLab == null ) || data.label == model.curLab)
                            {
                                conData.Add(new UiToolItemParam()
                                {
                                    data = data
                                });
                            }
                        }
                    }
                    break;
                case 3:
                    {
                        foreach (var data in MapMaskForm.DataById.Values)
                        {
                            if ((model.curLab == null ) || data.label == model.curLab)
                            {
                                conData.Add(new UiToolItemParam()
                                {
                                    data = data
                                });
                            }
                        }
                    }
                    break;
                case 4:
                    {
                        foreach (var data in MapObjectForm.DataById.Values)
                        {
                            if ((model.curLab == null ) || data.label == model.curLab)
                            {
                                conData.Add(new UiToolItemParam()
                                {
                                    data = data
                                });
                            }
                        }
                    }
                    break;
                case 5:
                    {
                        foreach (var data in MapItemForm.DataById.Values)
                        {
                            if ((model.curLab == null ) || data.label == model.curLab)
                            {
                                conData.Add(new UiToolItemParam()
                                {
                                    data = data
                                });
                            }
                        }
                        
                        
                    }
                    break;
                case 6:
                    {
                        foreach (var data in MapCharacterForm.DataById.Values)
                        {
                            if ((model.curLab == null ) || data.label == model.curLab)
                            {
                                conData.Add(new UiToolItemParam()
                                {
                                    data = data
                                });
                            }
                        }


                    }
                    break;
                case 100:
                    {
                        foreach (var data in MapEraseForm.DataById.Values)
                        {
                            if ((model.curLab == null) || data.label == model.curLab)
                            {
                                conData.Add(new UiToolItemParam()
                                {
                                    data = data
                                });
                            }
                        }
                    }
                    break;
                case 1:
                default:
                    {
                        foreach (var data in MapTerrainForm.DataById.Values)
                        {
                            if ((model.curLab == null ) || data.label == model.curLab)
                            {
                                conData.Add(new UiToolItemParam()
                                {
                                    data = data
                                });
                            }
                        }
                    }
                    break;
            }

            conData.Refresh();
            view.sta_align.ChangeState(model.posing ? 1 : 0);
            view.ipt_cntSetX.Set(model.cntX);
            view.ipt_cntSetY.Set(model.cntY);
            view.ipt_posSetX.Set(model.posX);
            view.ipt_posSetY.Set(model.posY);
            view.ipt_posSetZ.Set(model.posZ);
            view.ipt_rotateSet.Set(model.angle);

            view.go_layer.SetActive(model.curType.needLayer
                || (model.curData != null && model.curData is MapEraseForm.Data erase && erase.texture));

            view.sta_layer0.ChangeState(model.layer == 0 ? 1 : 0);
            view.sta_layer1.ChangeState(model.layer == 1 ? 1 : 0);
            view.sta_layer2.ChangeState(model.layer == 2 ? 1 : 0);

        }
        void RefreshLabs()
        {
            conLab.Clear();
            HashSet<string> labs = new HashSet<string>();
            switch (model.curType.id)
            {
                case 2:
                    foreach (var data in MapTextureForm.DataById.Values)
                    {
                        if (!string.IsNullOrEmpty(data.label))
                            labs.Add(data.label);
                    }
                    break;
                case 3:
                    foreach (var data in MapMaskForm.DataById.Values)
                    {
                        if (!string.IsNullOrEmpty(data.label))
                            labs.Add(data.label);
                    }
                    break;
                case 4:
                    foreach (var data in MapObjectForm.DataById.Values)
                    {
                        if (!string.IsNullOrEmpty(data.label))
                            labs.Add(data.label);
                    }
                    break;
                case 5:
                    foreach (var data in MapItemForm.DataById.Values)
                    {
                        if (!string.IsNullOrEmpty(data.label))
                            labs.Add(data.label);
                    }
                    break;
                case 6:
                    foreach (var data in MapCharacterForm.DataById.Values)
                    {
                        if (!string.IsNullOrEmpty(data.label))
                            labs.Add(data.label);
                    }
                    break;
                case 100:
                    foreach (var data in MapEraseForm.DataById.Values)
                    {
                        if (!string.IsNullOrEmpty(data.label))
                            labs.Add(data.label);
                    }
                    break;
                case 1:
                default:
                    foreach (var data in MapTerrainForm.DataById.Values)
                    {
                        if (!string.IsNullOrEmpty(data.label))
                            labs.Add(data.label);
                    }
                    break;
            }
            conLab.Add(new UiLabParam() { lab = null });
            foreach (var lab in labs)
            {
                conLab.Add(new UiLabParam() { lab = lab });
            }
            conLab.Refresh();
        }
        public void SetCurType(MapTypeForm.Data type)
        {
            model.curType = type;
            model.curLab = null;
            Refresh();
        }
        public void SetCurLab(string lab)
        {
            model.curLab = lab;
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
            view.btn_.onClick.AddListener(() =>
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
            view.sta_exist.ChangeState(1);

            view.txt_.text = model.data.name;
            view.img_.BindTexData(TexAssetForm.DataById.GetDv(model.data.icon, TexAssetForm.DataById[GlobalDefaultHelper.ExternDefaultTexId]));

            view.sta_.ChangeState(parent.model.curData == model.data ? 1 : 0);

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
            view.btn_.onClick.AddListener(() =>
            {
                parent.SetCurType(model.data);
            });
        }
        public override void OnShow()
        {
            model.data = param.data;
            view.sta_exist.ChangeState(1);
            view.txt_.text = TextManager.instance.GetTxt(model.data.NameKey);
            view.img_.BindTexData(TexAssetForm.DataById.GetDk(model.data.icon,GlobalDefaultHelper.DefaultTexId));


            view.sta_.ChangeState(parent.model.curType == model.data ? 1 : 0);
        }


    }

    public partial class UiLabParam
    {
        public string lab;
    }
    public partial class UiLabModel
    {
        public UiLabParam prm;
    }
    public partial class UiLabCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.SetCurLab(model.prm.lab);
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public void Refresh()
        {
            if (model.prm.lab != null)
            {
                view.txt_.text = model.prm.lab;
            }
            view.sta_.ChangeState(parent.model.curLab == model.prm.lab ? 1 : 0);
            view.sta_valid.ChangeState(model.prm.lab == null ? 0 : 1);
        }
    }

}
