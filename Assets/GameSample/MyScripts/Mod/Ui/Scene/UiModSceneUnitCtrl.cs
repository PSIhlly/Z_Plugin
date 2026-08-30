using Form;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem;
using Z_Map;
using Z_Map.Form;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Ui.ModSceneUnit
{
    public partial class UiModSceneUnitParam
    {
        public UnitForm.Data data;

    }
    public partial class UiModSceneUnitModel
    {
        public UnitForm.Data data;


        public string posX
        {
            set
            {
                float.TryParse(value, out float v);
                var x = MapManager.instance.utilCtrl.MapPos2RealPos(Vector3.one*GameManager.PlayerPosToMapPos(v)).x;
                var newPos = new Vector3(x, data.pos.y, data.pos.z);
                if (MapManager.instance.utilCtrl.InArea(newPos))
                {
                    MapManager.instance.updateCtrl.ApplyMove((MapUnit)data.unit, newPos, data.euler, true);
                    ModManager.instance.sceneCtrl.ForceUpdate();
                }
            }
            get
            {
                return GameManager.MapPosToPlayerPos(MapManager.instance.utilCtrl.RealPos2MapPos(data.pos)).x.ToString("0.##");
            }
        }
        public string posY
        {
            set
            {
                float.TryParse(value, out float v);

                TileUnitForm.Data belongMap = ((MapUnit)data.unit).belongTile.data;


                float minV = GameManager.MapPosToPlayerPos(belongMap.mapPos.y);
                float maxV = GameManager.MapPosToPlayerPos(belongMap.mapPos.y+ 0.9f);

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
                var newPos = new Vector3(data.pos.x,MapManager.instance.utilCtrl.MapPos2RealPos(Vector3.one*GameManager.PlayerPosToMapPos(v)) .y, data.pos.z);

                if (MapManager.instance.utilCtrl.InArea(newPos))
                {
                    MapManager.instance.updateCtrl.ApplyMove((MapUnit)data.unit, newPos, data.euler, true);
                    ModManager.instance.sceneCtrl.ForceUpdate();
                }
            }
            get
            {
                return GameManager.MapPosToPlayerPos(MapManager.instance.utilCtrl.RealPos2MapPos(data.pos)).y.ToString("0.##");
            }
        }
        public string posZ
        {
            set
            {
                float.TryParse(value, out float v);
                var z = MapManager.instance.utilCtrl.MapPos2RealPos(Vector3.one*GameManager.PlayerPosToMapPos(v)).z;

                var newPos = new Vector3(data.pos.x, data.pos.y, z);
                if (MapManager.instance.utilCtrl.InArea(newPos))
                {
                    MapManager.instance.updateCtrl.ApplyMove((MapUnit)data.unit, newPos, data.euler, true);
                    ModManager.instance.sceneCtrl.ForceUpdate();
                }
            }
            get
            {
                return GameManager.MapPosToPlayerPos(MapManager.instance.utilCtrl.RealPos2MapPos(data.pos)).z.ToString("0.##");
            }
        }
        public string angle
        {
            set
            {
                int.TryParse(value, out int v);
                v = (v % 360 + 360) % 360;
                MapManager.instance.updateCtrl.ApplyMove(
                    (MapUnit)data.unit,
                    data.pos,
                    new Vector3(data.euler.x, v, data.euler.z),
                    true);
                ModManager.instance.sceneCtrl.ForceUpdate();
            }
            get
            {
                return data.euler.y.ToString();
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
    }

    public partial class UiModSceneUnitCtrl
    {

        public override void OnCreate()
        {
            view.btn_bbg.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                if (model.data is ItemUnitForm.Data itemData)
                {
                    MapManager.instance.RemoveItem(itemData);
                }
                else if (model.data is ObjectUnitForm.Data objData)
                {
                    MapManager.instance.RemoveObject(objData);
                }
                else if (model.data is CharacterUnitForm.Data characterData)
                {
                    MapManager.instance.RemoveCharacter(characterData);
                }
                Close();
            });

            view.btn_aligh.onClick.AddListener(() =>
            {
                TileUnitForm.Data mapData = (TileUnitForm.Data)model.data.unit.superUnit.data;
                model.posX = GameManager.MapPosToPlayerPos(mapData.mapPos.x).ToString();
                model.posY = GameManager.MapPosToPlayerPos(mapData.mapPos.y).ToString();
                model.posZ = GameManager.MapPosToPlayerPos(mapData.mapPos.z).ToString();
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
            });
            view.ipt_posSetZ.onInput = ((v) =>
            {
                model.posZ = v;
                Refresh();
            });
            view.ipt_posSetY.onDeselect.AddListener((a) =>
            {
                Refresh();
            });

            view.ipt_rotateSet.onInput = ((v) =>
            {
                model.angle = v;
                Refresh();
            });
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();

        }
        public void Refresh()
        {
            view.txt_name.text = model.data.name;

            view.ipt_posSetX.Set(model.posX);
            view.ipt_posSetY.Set(model.posY);
            view.ipt_posSetZ.Set(model.posZ);
            view.ipt_rotateSet.Set(model.angle);
        }

    }
}
