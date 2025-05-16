using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_Ui.Form;
using Z_ByteSerialize;
using Newtonsoft.Json.Linq;
using Z_Ui.Notify;
using Z_DataSystem.Form;

namespace Ui.ModStoryEventCmdClips
{

    public partial class UiModStoryEventCmdClipsParam
    {
        public string clipsJo;
        public Func<List<ClipForm.Data>, bool> func;
    }
    public partial class UiModStoryEventCmdClipsModel
    {
        public List<ClipForm.Data> clips;
        public Func<List<ClipForm.Data>, bool> func;


    }
    public partial class UiModStoryEventCmdClipsCtrl
    {

        UiScrViewContainer<UiClipCtrl> con;
        public override void OnCreate()
        {

            view.btn_confirm.onClick.AddListener(() =>
            {
                if (model.func(model.clips))
                {
                    Close();
                }
            });
            con = new UiScrViewContainer<UiClipCtrl>(view.go_clip, view.scr_tt);

        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.func = param.func;
                if (!string.IsNullOrEmpty(param.clipsJo))
                {

                    model.clips = JObject.Parse(param.clipsJo).Get<List<ClipForm.Data>>(EvtValType.Clips.ToString());
                }
                else
                    model.clips = new List<ClipForm.Data>();
            }
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();
            for (int i = 0, icnt = model.clips.Count; i < icnt; i++)
            {
                con.Add(new UiClipParam()
                {
                    data = model.clips[i],
                    id = i
                });
            }
            con.Add(new UiClipParam()
            {
                data = null,
                id = -1
            });
            con.Refresh();
        }
    }


    public partial class UiClipParam
    {
        public ClipForm.Data data;
        public int id;
    }
    public partial class UiClipModel
    {
        public ClipForm.Data data;
        public int id;

    }
    public partial class UiClipCtrl
    {

        public override void OnCreate()
        {
            view.btn_add.onClick.AddListener(() =>
            {
                int id = model.id == -1 ? parent.model.clips.Count : model.id;
                parent.model.clips.Insert(id, id == 0 ? new ClipForm.Data(-1, "", "", "", "") : parent.model.clips[id - 1].Copy());
                parent.Refresh();
            });
            view.btn_profilePicture.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportClipTex((nm) =>
                {
                    model.data.profilePicture = nm;
                    parent.Refresh();
                });
            });
            view.btn_mainPicture.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportClipTex((nm) =>
                {
                    model.data.mainPicture = nm;
                    parent.Refresh();
                });
            });
            view.btn_title.onClick.AddListener(() =>
            {
                NotifyManager.instance.AddInputArea("", false, (s) =>
                {
                    model.data.title = s;
                    parent.Refresh();
                    return true;
                });
            });
            view.btn_mainText.onClick.AddListener(() =>
            {
                NotifyManager.instance.AddInputArea("", false, (s) =>
                {
                    model.data.mainText = s;
                    parent.Refresh();
                    return true;
                });
            });


        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.data = param.data;
                model.id = param.id;
            }
            Refresh();
        }
        
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.id == -1 ? 0 : 1);
            if (model.id != -1)
            {
                view.txt_mainText.text = model.data.mainText;
                view.txt_title.text = model.data.title;
                view.img_mainPicture.sprite = TexAssetForm.DataByName[model.data.mainPicture].sprite;
                view.img_profilePicture.sprite = TexAssetForm.DataByName[model.data.profilePicture].sprite;
            }
        }
    }


}