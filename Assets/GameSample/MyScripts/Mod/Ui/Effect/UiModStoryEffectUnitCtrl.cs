using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Code;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Math;
using Z_String;
using Z_Texture;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using static UnityEngine.GUI;

namespace Ui.ModStory.ModStoryEffect.ModStoryEffectUnit
{

    public partial class UiModStoryEffectUnitParam
    {
        public EffectForm.Data data;
    }
    public partial class UiModStoryEffectUnitModel
    {
        public EffectForm.Data data;
    }
    public partial class UiModStoryEffectUnitCtrl
    {

        UiContainer<UiClipsCtrl> clipsCon;
        public override void OnCreate()
        {
            clipsCon = new UiContainer<UiClipsCtrl>(view.go_clips);
            view.btn_back.onClick.AddListener(() =>
            {
                parent.SelPage(0);
            });
            view.ipt_name.onFinishInput = (s) =>
            {
                var lst = new List<string>();
                foreach (var data in EffectForm.DataByName.Values)
                    lst.Add(data.name);

                if (StringHelper.IsUniqueName(lst, s))
                    model.data.name = s;
                Refresh();
            };
            view.ipt_label.onFinishInput = (s) =>
            {
                model.data.label = s;
                Refresh();
            };
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteEffect(parent.model.data.uid);
                parent.SelPage(0);
            });
        }
        public override void OnShow()
        {

            if (param != null)
            {
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.ipt_name.Set(model.data.name);
            view.ipt_label.Set(model.data.label);

            clipsCon.Clear();
            for (int i = 0; i < parent.model.data.clips.Count; i++)
            {
                clipsCon.Add(new UiClipsParam()
                {
                    id = i
                });
            }
            clipsCon.Add(new UiClipsParam()
            {
                id = -1
            });
            clipsCon.Refresh();
            TimeManager.instance.AddCurLateUpdateAction(() =>
            {
                UiManager.Rebuild(view.go_content, true);
            },gameObject);
        }
    }
    public partial class UiClipsParam
    {
        public int id;
    }
    public partial class UiClipsModel
    {
        public int id;
    }
    public partial class UiClipsCtrl:IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiEffectCtrl> effectCon;
        public override void OnCreate()
        {
            effectCon = new UiScrViewContainer<UiEffectCtrl>(view.go_effect, view.scr_effects);

            view.btn_new.onClick.AddListener(() =>
            {
                parent.model.data.clips.Add(ModManager.instance.assetCtrl.CreateEffectClips());
                parent.Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteEffectClips(parent.model.data.uid,model.id);
                parent.Refresh();
            });

            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportEffectImage(parent.model.data.uid,model.id);
                Refresh();
            });
        }
        public override void OnShow()
        {
            this.Register();
            if (param != null)
            {
                model.id = param.id;
            }
            Refresh();
        }
        public override void OnDisable()
        {
            this.Unregister();
        }
        public void Refresh()
        {
            view.sta_.ChangeState(model.id == -1 ? 0 : 1);
            if (model.id != -1)
            {
                effectCon.Clear();
                for (int i = 0; i < parent.model.data.clips[model.id].Count; i++)
                {
                    effectCon.Add(new UiEffectParam()
                    {
                        id = i
                    });
                }
                effectCon.Add(new UiEffectParam()
                {
                    id = -1
                });
                effectCon.Refresh();
                view.img_image.sprite = TexAssetForm.DataByName[parent.model.data.clips[model.id][0].tex].GetSprite();
            }
        }

        void IZ_Listener<AssetEvent>.OnEvent(AssetEvent evt)
        {
            Refresh();
        }
    }
    public partial class UiEffectParam
    {
        public int id;
    }
    public partial class UiEffectModel
    {
        public int id;
    }
    public partial class UiEffectCtrl
    {
        EffectClipForm.Data data => parent.parent.model.data.clips[parent.model.id][model.id];
        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                parent.parent.model.data.clips[parent.model.id].Add(ModManager.instance.assetCtrl.CreateEffectClip(parent.parent.model.data.clips[parent.model.id][0].tex));
                parent.Refresh();
            });
            view.ipt_posSetX.onFinishInput = (s) =>
            {
                data.pos= data.pos.NewSetX(StringHelper.ToFloat(s,0));
                Refresh();
            };
            view.ipt_posSetY.onFinishInput = (s) =>
            {
                data.pos = data.pos.NewSetY(StringHelper.ToFloat(s, 0));
                Refresh();
            };
            view.ipt_posSetZ.onFinishInput = (s) =>
            {
                data.pos = data.pos.NewSetZ(StringHelper.ToFloat(s, 0));
                Refresh();
            };

            view.ipt_scaleSetX.onFinishInput = (s) =>
            {
                data.scale = data.scale.NewSetX(StringHelper.ToFloat(s, 1));
                Refresh();
            };
            view.ipt_scaleSetY.onFinishInput = (s) =>
            {
                data.scale = data.scale.NewSetY(StringHelper.ToFloat(s, 1));
                Refresh();
            };
            view.ipt_scaleSetZ.onFinishInput = (s) =>
            {
                data.scale = data.scale.NewSetZ(StringHelper.ToFloat(s, 1));
                Refresh();
            };

            view.ipt_rotate.onFinishInput = (s) =>
            {
                data.rot = StringHelper.ToInt(s, 0);
                Refresh();
            };

            view.ipt_sustain.onFinishInput = (s) =>
            {
                data.time = StringHelper.ToFloat(s, 0);
                Refresh();
            };

            view.ipt_opacity.onFinishInput = (s) =>
            {
                data.opacity = StringHelper.ToFloat(s, 1);
                Refresh();
            };
            view.btn_transparence.onClick.AddListener(() =>
            {
                data.transition = !data.transition;
                Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteEffectClip(parent.parent.model.data.uid,parent.model.id,model.id);
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
            view.sta_exist.ChangeState(model.id == -1 ? 0 : 1);
            if(model.id != -1)
            {
                var clip =data;
                view.ipt_posSetX.Set(clip.pos.x.ToString("0.##"));
                view.ipt_posSetY.Set(clip.pos.y.ToString("0.##"));
                view.ipt_posSetZ.Set(clip.pos.z.ToString("0.##"));

                view.ipt_rotate.Set(clip.rot.ToString());
                view.ipt_opacity.Set(clip.opacity.ToString("0.##"));

                view.ipt_scaleSetX.Set(clip.scale.x.ToString("0.##"));
                view.ipt_scaleSetY.Set(clip.scale.y.ToString("0.##"));
                view.ipt_scaleSetZ.Set(clip.scale.z.ToString("0.##"));

                view.ipt_sustain.Set(clip.time.ToString("0.##"));
                view.sta_transparence.ChangeState(clip.transition ? 1 : 0);
                view.btn_delete.gameObject.SetActive(model.id > 0);
            }


        }
    }
}