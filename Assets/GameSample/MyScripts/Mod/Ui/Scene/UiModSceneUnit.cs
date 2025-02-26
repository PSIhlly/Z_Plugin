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
using Z_UnitSystem.Form;

namespace Ui
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
                var x = (v + ModSceneManager.instance.offset)* MapManager.instance.dataCtrl.mainData.mapUnitSize.x;
                var newPos = new Vector3(x, data.pos.y, data.pos.z);
                if (MapManager.instance.utilCtrl.InArea(newPos))
                { 
                    data.pos = newPos;
                    ModSceneManager.instance.ForceUpdate();
                }
            }
            get
            {
                return (data.pos.x / MapManager.instance.dataCtrl.mainData.mapUnitSize.x - ModSceneManager.instance.offset).ToString("0.##");
            }
        }
        public string posY
        {
            set
            {
                float.TryParse(value, out float v); 
                
                MapUnitForm.Data belongMap = (MapUnitForm.Data)data.unit.superUnit.data;
                float minV = belongMap.mapPos.y - ModSceneManager.instance.offset;
                float maxV = belongMap.mapPos.y - ModSceneManager.instance.offset+0.9f;

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
                var newPos = new Vector3(data.pos.x, (v + ModSceneManager.instance.offset) * MapManager.instance.dataCtrl.mainData.mapUnitSize.y, data.pos.z);

                if (MapManager.instance.utilCtrl.InArea(newPos))
                {
                    data.pos = newPos;
                    ModSceneManager.instance.ForceUpdate();
                }
            }
            get
            {
                return (data.pos.y/ MapManager.instance.dataCtrl.mainData.mapUnitSize.y - ModSceneManager.instance.offset).ToString("0.##");
            }
        }
        public string posZ
        {
            set
            {
                float.TryParse(value, out float v);
                var z = (v + ModSceneManager.instance.offset) * MapManager.instance.dataCtrl.mainData.mapUnitSize.z;

                var newPos = new Vector3(data.pos.x, data.pos.y, z);
                if (MapManager.instance.utilCtrl.InArea(newPos))
                {
                    data.pos = newPos;
                    ModSceneManager.instance.ForceUpdate();
                }
            }
            get
            {
                return (data.pos.z / MapManager.instance.dataCtrl.mainData.mapUnitSize.z - ModSceneManager.instance.offset).ToString("0.##");
            }
        }
        public string angle
        {
            set
            {
                int.TryParse(value, out int v);
                v = (v % 360 + 360) % 360;
                data.euler = new Vector3(data.euler.x,v,data.euler.z);
                ModSceneManager.instance.ForceUpdate();
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

                ModSceneManager.instance.posing = value;
            }
            get
            {
                return ModSceneManager.instance.posing;
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
                model.data.unit.Remove();
                Close();
            });

            view.btn_pos.onClick.AddListener(() =>
            {
                MapUnitForm.Data mapData= (MapUnitForm.Data)model.data.unit.superUnit.data;
                model.posX = (mapData.mapPos.x- ModSceneManager.instance.offset).ToString();
                model.posY = (mapData.mapPos.y  - ModSceneManager.instance.offset).ToString();
                model.posZ = (mapData.mapPos.z - ModSceneManager.instance.offset).ToString();
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
