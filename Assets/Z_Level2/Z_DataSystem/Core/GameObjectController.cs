using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using Z_Audio;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Os.File;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;
using static System.Net.Mime.MediaTypeNames;
namespace Z_DataSystem.Form
{
    public partial class GameObjectAssetForm
    {
        public partial class Data
        {
            private Sprite _sprite;
            private GameObject _go => (GameObject)asset;

            public GameObject GetGo()
            {
                return _go;
            }
        }
    }
    public class GameObjectController : Z_Controller<AssetManager>, IAssetController
    {
        public string GetMark() => "$g$";
        public string[] GetSupportedExtensions() => new string[]
        { 
        };

        public GameObjectController(AssetManager super) : base(super)
        {
        }


    }
}
