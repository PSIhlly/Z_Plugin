using BaseFunc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;
namespace Z_Ui.Dialog
{

    public class HistoryFuncController : Z_MonoController<HideFuncController>
    {
        public Button hideHistoryBtn;
        public Button showHistoryBtn;
        public SwitchFunc hideSf;
        public void Awake()
        {
            var dialogManager = (DialogUiBaseManager)_manager;

            hideHistoryBtn.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ShowTypeEvent()
                {
                    showType = ShowType.Normal
                });
                hideSf.ChangeState(1);
            });

            showHistoryBtn.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ShowTypeEvent()
                {
                    showType = ShowType.History
                });
                hideSf.ChangeState(0);
            });
        }
    }
}
