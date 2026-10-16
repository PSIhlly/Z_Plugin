using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Ui.Start;
using Ui.EnterMain;
using Z_Text;
using Z_DataSystem.Form;
using Ui.ModStoryEditorStyleWindow;
using Z_DesignStyle;
using Z_DataSystem;
using System;
using System.Linq;
using System.IO;
using System.Threading;
using Z_Ui.Notify;
using Z_Ui.Loading;

namespace Ui.Mod
{
    public partial class UiModModel
    {
        public bool importing;
        public int importVersion;
    }
    public partial class UiModCtrl
    {
        UiScrViewContainer<UiItemCtrl> con;
        private CancellationTokenSource importCancellation;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            view.btn_import.onClick.AddListener(ImportStoryPackage);
            view.btn_back.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiEnterMainCtrl>();
                Close();
            });
        }


        public override void OnShow()
        {
            Refresh();
        }
        public override void OnHide()
        {
            ++model.importVersion;
            importCancellation?.Cancel();
            model.importing = false;
        }

        private async void ImportStoryPackage()
        {
            if (model.importing)
                return;
            model.importing = true;
            var version = ++model.importVersion;
            var cancellation = new CancellationTokenSource();
            importCancellation = cancellation;
            try
            {
                await LoadingManager.instance.RunAsync("modImport", async () =>
                {
                    var path = await AssetFilePicker.GetModPackageAsync(TextManager.instance.GetTxt("import"), cancellation.Token);
                    if (!active || version != model.importVersion || !model.importing)
                        return;
                    if (string.IsNullOrEmpty(path))
                        return;
                    var story = await GameManager.instance.saveCtrl.ImportPackageAsync(path, cancellation.Token);
                    if (active && version == model.importVersion)
                    {
                        Refresh();
                        NotifyManager.instance.AddTip(TextManager.instance.GetTxt("import") + ": " + story.name);
                    }
                }, cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // Closing the panel or canceling the picker is not an error.
            }
            catch (Exception exception)
            {
                if (active && version == model.importVersion)
                    ShowImportError(exception.Message);
            }
            finally
            {
                if (version == model.importVersion)
                    model.importing = false;
                if (ReferenceEquals(importCancellation, cancellation))
                    importCancellation = null;
                cancellation.Dispose();
            }
        }

        private static void ShowImportError(string error)
        {
            NotifyManager.instance.AddPopup(TextManager.instance.GetTxt("import"), error, true, enableScr: true);
        }
        public void Refresh()
        {
            con.Clear();
            foreach (var data in StoryForm.DataById.Values.OrderBy(d => d.id))
            {
                con.Add(new UiItemParam()
                {
                    data = data
                });
            }
            con.Add(new UiItemParam()
            {
                data = null
            });
            con.Refresh();
        }



    }
    public partial class UiItemParam
    {
        public StoryForm.Data data;
    }
    public partial class UiItemModel
    {
        public StoryForm.Data data;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryEditorStyleWindowCtrl>(new UiModStoryEditorStyleWindowParam()
                {
                     onSelect = (type) =>
                     {
                         Main2StoryManager.instance.StartLoadStoryUgc(StoryForm.idChain.PeekId(), type);
                         parent.Close();
                     }
                });
            });
            view.btn_.onClick.AddListener(() =>
            {

                Main2StoryManager.instance.StartLoadStoryUgc(model.data.id,default);

                parent.Close();
            });
        }
        public override void OnShow()
        {
            model.data = param.data;

            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data != null ? 1 : 0);
            if (model.data != null)
            {
                view.txt_.text = model.data.name;
                try
                {
                    view.img_.BindTexData(AssetManager.instance.texCtrl.CreateDataByBytes(Convert.FromBase64String(model.data.icon), "tmp"));
                }
                catch(Exception ex)
                {
                    Debug.Log(ex);
                    TexAssetForm.DataById[GlobalDefaultHelper.ExternDefaultTexId].GetSprite();
                }
            }
        }



    }
}
