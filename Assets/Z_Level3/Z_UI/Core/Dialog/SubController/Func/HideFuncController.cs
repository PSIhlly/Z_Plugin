using Z_Trick.BaseFunc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;
namespace Z_Ui.Dialog
{
    public class HideFuncController : Z_MonoController<DialogUiBaseManager>
{
        public Button hideBtn;
        public Button showBtn;
        public SwitchFunc hideSf;
        public void Awake()
        {

            hideBtn.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ShowTypeEvent()
                {
                    showType = ShowType.Hide
                });
                hideSf.ChangeState(1);
            });
            showBtn.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ShowTypeEvent()
                {
                    showType = ShowType.Normal
                });
                hideSf.ChangeState(0);
            });
        }
    }
}
