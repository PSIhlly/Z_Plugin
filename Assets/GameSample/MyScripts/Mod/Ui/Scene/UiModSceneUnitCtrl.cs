using Form;
using System;
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
        public System.Action beforeDataChange;
        public string posX
        {
            set
            {
                if (!float.TryParse(value, out float v))
                    return;
                var x = MapManager.instance.utilCtrl.MapPos2RealPos(Vector3.one*GameManager.PlayerPosToMapPos(v)).x;
                var newPos = new Vector3(x, data.pos.y, data.pos.z);
                if (MapManager.instance.utilCtrl.InArea(newPos))
                {
                    ApplyMove(newPos, data.euler);
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
                if (!float.TryParse(value, out float v))
                    return;

                TileUnitForm.Data belongMap = ((MapUnit)data.unit).belongTile.data;


                float minV = GameManager.MapPosToPlayerPos(belongMap.mapPos.y);

                if (v < minV)
                {
                    v = minV;
                    NotifyManager.instance.AddTip(TextManager.instance.GetTxt("minYTip"));
                }
                var newPos = new Vector3(data.pos.x,MapManager.instance.utilCtrl.MapPos2RealPos(Vector3.one*GameManager.PlayerPosToMapPos(v)) .y, data.pos.z);

                if (MapManager.instance.utilCtrl.InArea(newPos))
                {
                    ApplyMove(newPos, data.euler);
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
                if (!float.TryParse(value, out float v))
                    return;
                var z = MapManager.instance.utilCtrl.MapPos2RealPos(Vector3.one*GameManager.PlayerPosToMapPos(v)).z;

                var newPos = new Vector3(data.pos.x, data.pos.y, z);
                if (MapManager.instance.utilCtrl.InArea(newPos))
                {
                    ApplyMove(newPos, data.euler);
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
                if (!int.TryParse(value, out int v))
                    return;
                v = (v % 360 + 360) % 360;
                if (Mathf.Approximately(data.euler.y, v))
                    return;
                ApplyMove(data.pos, new Vector3(data.euler.x, v, data.euler.z));
            }
            get
            {
                return data.euler.y.ToString();
            }
        }
        private void ApplyMove(Vector3 pos, Vector3 euler)
        {
            if (data.pos == pos && data.euler == euler)
                return;
            beforeDataChange?.Invoke();
            using (ModManager.instance.sceneCtrl.BeginOperation())
            {
                ModManager.instance.sceneCtrl.Track(data);
                MapManager.instance.updateCtrl.ApplyMove((MapUnit)data.unit, pos, euler, true);
                if (data.unit is ObjectUnit objectUnit)
                    Z_EventHelper.Invoke(new ObjectEvent { type = MapEventType.Refresh, unit = objectUnit });
                ModManager.instance.sceneCtrl.ForceUpdate();
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
        private IDisposable textOperation;
        private Ipt editingInput;

        private void FinishTextOperation()
        {
            var operation = textOperation;
            textOperation = null;
            operation?.Dispose();
        }

        private void BindTextOperation(Ipt input)
        {
            input.onSelect.AddListener(_ =>
            {
                FinishTextOperation();
                editingInput = input;
            });
            input.onDeselect.AddListener(_ =>
            {
                FinishTextOperation();
                editingInput = null;
            });
            input.onSubmit.AddListener(_ =>
            {
                FinishTextOperation();
            });
        }

        public override void OnCreate()
        {
            model.beforeDataChange = () =>
            {
                if (editingInput != null && textOperation == null)
                    textOperation = ModManager.instance.sceneCtrl.BeginOperation();
            };
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
                FinishTextOperation();
                editingInput = null;
                using (ModManager.instance.sceneCtrl.BeginOperation())
                {
                    ModManager.instance.sceneCtrl.Track(model.data);
                    if (model.data is ItemUnitForm.Data itemData)
                        MapManager.instance.RemoveItem(itemData);
                    else if (model.data is ObjectUnitForm.Data objData)
                        MapManager.instance.RemoveObject(objData);
                    else if (model.data is CharacterUnitForm.Data characterData)
                        MapManager.instance.RemoveCharacter(characterData);
                }
                Close();
            });

            view.btn_aligh.onClick.AddListener(() =>
            {
                FinishTextOperation();
                editingInput = null;
                var tile = ((MapUnit)model.data.unit).belongTile;
                if (tile == null)
                    return;
                using (ModManager.instance.sceneCtrl.BeginOperation())
                {
                    model.posX = GameManager.MapPosToPlayerPos(tile.data.mapPos.x).ToString();
                    model.posY = GameManager.MapPosToPlayerPos(tile.data.mapPos.y).ToString();
                    model.posZ = GameManager.MapPosToPlayerPos(tile.data.mapPos.z).ToString();
                }
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
            BindTextOperation(view.ipt_posSetX);
            BindTextOperation(view.ipt_posSetY);
            BindTextOperation(view.ipt_posSetZ);
            BindTextOperation(view.ipt_rotateSet);
        }
        public override void OnShow()
        {
            FinishTextOperation();
            editingInput = null;
            model.data = param.data;
            Refresh();

        }
        public override void OnHide()
        {
            FinishTextOperation();
            editingInput = null;
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
