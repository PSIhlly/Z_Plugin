using Z_Trick.BaseFunc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;
namespace Z_Ui.Dialog
{
    public class SkipFuncController : Z_MonoController<DialogUiBaseManager>
{
        public Button skipBtn;
        public void Awake()
        {

            skipBtn.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ClipPlayEvent()
                {
                    isOver = true
                });
            });
        }
    }
}
