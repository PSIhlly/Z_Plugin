using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using UnityEngine.UIElements;
using Z_Text;
using UnityEngine;
using Z_DataSystem.Form;
using Z_UnitSystem;
using Z_Ui.Notify;
using Z_String;
using Z_Math;
using Z_DataSystem;
using Ui.Axis;
using Z_DesignStyle;
using Z_Ui;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectAppearance
{

    public partial class UiModStoryMapObjectObjectAppearanceParam
    {
        public MapObjectForm.Data data;
    }
    public partial class UiModStoryMapObjectObjectAppearanceModel
    {
        public MapObjectForm.Data data;
        public AnimDirecton dir;
        public int id;

        public float height
        {
            get => data.model.subPrefabUnitScale[0].y;
            set
            {
                float bottomHeight = posHeight;
                data.model.subPrefabUnitScale[0] = data.model.subPrefabUnitScale[0].NewSetY(value);
                posHeight = bottomHeight;
            }
        }

        // Combined models retain their saved center offset plus the legacy 0.5
        // pivot. Only the editor converts that offset to a bottom-relative height.
        public float posHeight
        {
            get => data.model.subPrefabUnitPos[0].y - (Mathf.Abs(height) - 1) / 2;
            set => data.model.subPrefabUnitPos[0] = data.model.subPrefabUnitPos[0]
                .NewSetY(value + (Mathf.Abs(height) - 1) / 2);
        }

        public void SetHorizontalPosition(Vector2 position)
        {
            data.model.subPrefabUnitPos[0] = new Vector3(position.x,
                data.model.subPrefabUnitPos[0].y, position.y);
        }
    }
    public partial class UiModStoryMapObjectObjectAppearanceCtrl:IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiItemCtrl> itemCon;
        UiContainer<UiDirCtrl> dirCon;
        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            itemCon = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            dirCon = new UiContainer<UiDirCtrl>(this, view.go_dir);
            view.btn_reset.onClick.AddListener(() =>
            {
                model.data.model.subPrefabUnitScale[0] = Vector3.one;
                model.data.model.subPrefabUnitPos[0] = Vector3.zero;
                Refresh();
            });
            view.btn_model.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseModel(TextManager.instance.GetTxt("chooseModel"), (item) =>
                {
                    model.data.model.subPrefabUnitName[0] = item.id;
                    Refresh();
                });

            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteObject(model.data.name);
                parent.parent.SelType(3);
                Refresh();
            });
            view.btn_deleteUnit.onClick.AddListener(() =>
            {
                if (model.id >= 0)
                {
                    ModManager.instance.assetCtrl.DeleteObjectUnitTex(model.data.name, model.dir, model.id);
                    model.id = -1;
                    Refresh();
                }
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                model.data.name = s;
                Refresh();
            };
            view.btn_label.onClick.AddListener(() =>
            {
                UiLabRenderHelper.Choose(nameof(MapObjectForm), labId =>
                {
                    model.data.labId = labId;
                    Refresh();
                });
            });
            view.ipt_posHeight.onFinishInput += (s) =>
            {
                model.posHeight = StringHelper.ToFloat(s, 0, false);
                Refresh();
            };
            view.ipt_height.onFinishInput += (s) =>
            {
                model.height = StringHelper.ToFloat(s, 1, true);
                Refresh();
            };
            view.ipt_length.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[0]= model.data.model.subPrefabUnitScale[0].NewSetZ(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.ipt_width.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[0]= model.data.model.subPrefabUnitScale[0].NewSetX(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportObjectUnitTex(model.data.id, model.dir, model.id);
                Refresh();
            });
            view.ipt_interval.onFinishInput += (s) =>
            {
                model.data.model.animTimeInterval = StringHelper.ToFloat(s, 0, true);
                Refresh();
            };
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.data = param.data;
            }
            model.data.EnsureDirectionData();
            model.dir = model.data.GetDefaultAnimDirection();
            model.id = -1;
            DisplayCameraAreaManager.instance.Show();
            Refresh();
        }

        public void Refresh()
        {
            model.data.EnsureDirectionData();
            if (model.data.faceType == FaceType.FourDirection && model.dir == AnimDirecton.Fixed)
                model.dir = AnimDirecton.Up;
            else if (model.data.faceType != FaceType.FourDirection && model.dir != AnimDirecton.Fixed)
                model.dir = AnimDirecton.Fixed;

            view.sta_show.ChangeState(model.id == -1 ? 0 : 1);

            view.ipt_name.Set(model.data.name);
            view.txt_label.text = UiLabRenderHelper.GetText(model.data.labId, false);

            DisplayCameraAreaManager.instance.Clear();

            view.model_axis.SetShow(false);

            float rate = DisplayCameraAreaManager.instance.normalized2scene;
            view.model_axis.SetShow(true, new UiAxisParam()
            {
                pos = new Vector2((model.data.model.subPrefabUnitPos[0].x + rate / 2) / rate, (model.data.model.subPrefabUnitPos[0].z + rate / 2) / rate),
                limitRtf = view.rtf_image,
                onTrsChange = (tp) => {
                    model.SetHorizontalPosition(new Vector2((tp.Item1.x * 2 - 1) * rate / 2,
                        (tp.Item1.y * 2 - 1) * rate / 2));
                    RefreshView();
                }
            });
            RefreshView();

            if (model.id != -1)
            {
                view.ipt_posHeight.Set(model.posHeight.ToString("0.##"));
                view.ipt_height.Set(model.height.ToString("0.##"));
                view.ipt_length.Set(model.data.model.subPrefabUnitScale[0].z.ToString("0.##"));
                view.ipt_width.Set(model.data.model.subPrefabUnitScale[0].x.ToString("0.##"));
            }
            view.ipt_interval.Set(model.data.model.animTimeInterval.ToString("0.##"));

            dirCon.Clear();
            switch (model.data.faceType)
            {
                case FaceType.Fixed:
                case FaceType.Flexible:
                    dirCon.Add(new UiDirParam() { dir = AnimDirecton.Fixed });
                    break;
                case FaceType.FourDirection:
                    dirCon.Add(new UiDirParam() { dir = AnimDirecton.Up });
                    dirCon.Add(new UiDirParam() { dir = AnimDirecton.Down });
                    dirCon.Add(new UiDirParam() { dir = AnimDirecton.Left });
                    dirCon.Add(new UiDirParam() { dir = AnimDirecton.Right });
                    break;
            }
            dirCon.Refresh();

            itemCon.Clear();
            var texs = model.data.GetAnimClip(model.dir);
            if (texs != null)
            {
                for (int i = 0; i < texs.Count; i++)
                {
                    itemCon.Add(new UiItemParam()
                    {
                        id = i,
                    });
                }
            }
            itemCon.Add(new UiItemParam()
            {
                id = -1,
            });
            itemCon.Refresh();
            UiManager.Rebuild(gameObject, true);

        }
        private void RefreshView()
        {
            DisplayCameraAreaManager.instance.Clear();
            model.data.SyncLegacyAnimClip(model.dir);
            var showGo = GameManager.instance.utilCtrl.CombineNewObjectByPrefabs("fakeObj", model.data, false,
                previewDirection: model.dir);
            showGo.SetActive(true);
            DisplayCameraAreaManager.instance.Add(showGo, Vector3.zero);
        }

        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }
    }

    public partial class UiDirParam
    {
        public AnimDirecton dir;
    }

    public partial class UiDirModel
    {
        public AnimDirecton dir;
    }

    public partial class UiDirCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.dir = model.dir;
                parent.model.id = -1;
                parent.Refresh();
            });
        }

        public override void OnShow()
        {
            model.dir = param.dir;
            Refresh();
        }

        public void Refresh()
        {
            view.txt_.oriText = model.dir.ToString();
            view.sta_.ChangeState(model.dir == parent.model.dir ? 1 : 0);
        }
    }


    public partial class UiItemParam
    {
        public int id;
    }
    public partial class UiItemModel
    {
        public int id;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateObjectUnitTex(parent.model.data.id, parent.model.dir);
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.id = model.id;
                parent.Refresh();
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.id = param.id;
            }
            Refresh();
        }

        public void Refresh()
        {
            view.sta_exist.ChangeState(model.id >= 0 ? 1 : 0);
            TexAssetForm.Data tex = null;
            if (model.id >= 0)
            {
                var texs = parent.model.data.GetAnimClip(parent.model.dir);
                int texId = texs != null && texs.Count > model.id ? texs[model.id] : 0;
                if (texId != 0 && texId != GlobalDefaultHelper.DefaultTexId)
                    tex = TexAssetForm.DataById.GetDv(texId, null);
            }
            view.txt_.text = string.Empty;
            view.img_.BindTexDataOrHide(tex);
            view.sta_.ChangeState(model.id >= 0 && model.id == parent.model.id ? 1 : 0);
        }
    }
}
