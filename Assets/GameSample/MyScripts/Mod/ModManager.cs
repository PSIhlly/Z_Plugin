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
    public void BeginStory(string storyName)
    {
        this._folderName = storyName;
        UiManager.instance.ShowUi<UiModStoryCtrl>();
    }
    public void EndStory()
    {

    }
    public async void BeginScene(string fileName)
    {
        _sceneCtrl.Begin(fileName);
    }

    public void EndScene()
    {
        _sceneCtrl.End();
    }

    #endregion
    public string GetSceneFileName()
    {
        return  _folderName + "/Core/"+ _sceneCtrl.fileName;
    }
   
    public string GetStoryCoreFolder()
    {
        return _folderName+"/Core/";
    }
    public string GetFolderName()
    {
        return _folderName;
    }
}
