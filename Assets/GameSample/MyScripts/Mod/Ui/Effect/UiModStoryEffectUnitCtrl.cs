using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
using Z_Math;
using Z_DataSystem.Form;

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

        UiScrViewContainer<UiEffectCtrl> effectCon;
        public override void OnCreate()
        {
            effectCon = new UiScrViewContainer<UiEffectCtrl>(view.go_effect,view.scr_effects);
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

            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportEffectImage(parent.model.data.uid);
                Refresh();
            });
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

            effectCon.Clear();
            for(int i=0; i<model.data.clips.Count;i++)
            {
                effectCon.Add(new UiEffectParam()
                {
                    id=i
                });
            }
            effectCon.Add(new UiEffectParam()
            {
                id = -1
            });
            effectCon.Refresh();

            view.img_image.sprite = TexAssetForm.DataByName[model.data.clips[0].tex].GetSprite();
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
        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                parent.model.data.clips.Add(ModManager.instance.assetCtrl.CreateEffectClip(parent.model.data.clips[0].tex));
                parent.Refresh();
            });
            view.ipt_posSetX.onFinishInput = (s) =>
            {
                parent.model.data.clips[model.id].pos=parent.model.data.clips[model.id].pos.NewSetX(StringHelper.ToFloat(s,0));
                Refresh();
            };
            view.ipt_posSetY.onFinishInput = (s) =>
            {
                parent.model.data.clips[model.id].pos = parent.model.data.clips[model.id].pos.NewSetY(StringHelper.ToFloat(s, 0));
                Refresh();
            };
            view.ipt_posSetZ.onFinishInput = (s) =>
            {
                parent.model.data.clips[model.id].pos = parent.model.data.clips[model.id].pos.NewSetZ(StringHelper.ToFloat(s, 0));
                Refresh();
            };

            view.ipt_scaleSetX.onFinishInput = (s) =>
            {
                parent.model.data.clips[model.id].scale = parent.model.data.clips[model.id].scale.NewSetX(StringHelper.ToFloat(s, 1));
                Refresh();
            };
            view.ipt_scaleSetY.onFinishInput = (s) =>
            {
                parent.model.data.clips[model.id].scale = parent.model.data.clips[model.id].scale.NewSetY(StringHelper.ToFloat(s, 1));
            };
            view.ipt_scaleSetZ.onFinishInput = (s) =>
            {
                parent.model.data.clips[model.id].scale = parent.model.data.clips[model.id].scale.NewSetZ(StringHelper.ToFloat(s, 1));
                Refresh();
            };

            view.ipt_rotate.onFinishInput = (s) =>
            {
                parent.model.data.clips[model.id].rot = StringHelper.ToInt(s, 0);
                Refresh();
            };

            view.ipt_sustain.onFinishInput = (s) =>
            {
                parent.model.data.clips[model.id].time = StringHelper.ToInt(s, 0);
                Refresh();
            };

            view.ipt_opacity.onFinishInput = (s) =>
            {
                parent.model.data.clips[model.id].opacity = StringHelper.ToFloat(s, 1);
                Refresh();
            };
            view.btn_transparence.onClick.AddListener(() =>
            {
                parent.model.data.clips[model.id].transition = !parent.model.data.clips[model.id].transition;
                Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteEffectClip(parent.model.data.uid,model.id);
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
                var clip =parent.model.data.clips[model.id];
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