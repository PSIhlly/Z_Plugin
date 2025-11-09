using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using Z_Audio;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Os.File;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;
namespace Z_DataSystem.Form
{
    public class VideoController : Z_Controller<AssetManager>
    {
        public VideoController(AssetManager super) : base(super)
        {
        }
    }
}
