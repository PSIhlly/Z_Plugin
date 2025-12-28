using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Loading;
using Ui.ModSceneMain;
using Ui.ModStory;
using UnityEngine;
using Z_DesignStyle;
using Z_Map;
using Z_Ui;
using Z_UnitSystem;
public enum EditorStyle
{
    Avg,
    AvgAdvanced,
    Rpg,
    RpgAdvanced,
}
public class ModManager : Z_MonoManager<ModManager>
{

    private string _folderName;




    #region life

    private InternalModSceneController _sceneCtrl;
    public ExternalModSceneController sceneCtrl;
    public ModAssetCtrl assetCtrl;
    public override void Init()
    {
        base.Init();
        var __sceneCtrl = new ModSceneController(this);
        _sceneCtrl = __sceneCtrl;
        sceneCtrl = __sceneCtrl;

        assetCtrl = new ModAssetCtrl(this);
    }
    public void OnMouse(bool click, Vector3 pos, Vector3 dir)
    {
        _sceneCtrl.OnMouse(click,pos,dir);
    }
    public void Update()
    {
        _sceneCtrl.Update();
    }
    public void BeginStory(int id)
    {
        GameManager.instance.saveCtrl.LoadCoreStory(id);
        _folderName = Main2StoryManager.GetStoryFolderNameById(id);
        UiManager.instance.ShowUi<UiModStoryCtrl>();
    }
    public void EndStory()
    {

    }
    public async void BeginScene(int id)
    {
        _sceneCtrl.Begin(id);
    }

    public void EndScene()
    {
        _sceneCtrl.End();
    }

    #endregion

    public string GetSceneCoreFileName()
    {
        return Main2StoryManager.GetStoryCoreFolder(_folderName) + _sceneCtrl.fileName;
    }

    public string GetStoryCoreFolder()
    {
        return Main2StoryManager.GetStoryCoreFolder(_folderName);
    }
    public string GetFolderName()
    {
        return _folderName;
    }

}
