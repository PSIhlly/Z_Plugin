using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Gal.GalUI;
using Z_DesignStyle;
using Z_Gal.Database;
namespace Z_Gal.Core
{
public class GameManager : Z_MonoSingleton<GameManager>
{
    public GalUIManagerBaseType galUIManagerBaseType;
        public InputManager inputManager;
        public TopDatabase topDatabase;

        public int progress=-1;
    public int overallProgress;

        public void Start()
        {
           
        }

        public void InitDatabase(string clipRawData)
        {
            topDatabase.Register(null, Application.streamingAssetsPath + "/", "Save.data");
            topDatabase.Init();
           

        }
        public void InitSettings()
        {
            galUIManagerBaseType.settings = new Settings();
        }
        public void NextClip()
        {
            progress++;
            ClipDatabase.RawClipData rawClipData = ((ClipDatabase.RawClipData)(((ClipDatabase)topDatabase.GetRawData(0)).clipDatabase).GetRawData(progress));
            galUIManagerBaseType.StartClip(new ClipData(rawClipData.GetMainText()));
        }
        
    }
}
