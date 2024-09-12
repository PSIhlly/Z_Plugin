using BaseFunc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;
namespace Z_Ui.Dialog
{
    public class AutoPlayFuncController : Z_MonoController<AutoPlayFuncController>
{
        public Button autoPlayBtn;
        public SwitchFunc autoPlaySf;
        public void Awake()
        {
            var dialogManager = (DialogUiBaseManager)_manager;

            autoPlaySf.ChangeState((int)dialogManager.settings.autoPlaySpeed);
            autoPlayBtn.onClick.AddListener(() =>
            {
                autoPlaySf.ChangeState();
                dialogManager.settings.autoPlaySpeed = autoPlaySf.state;
            });
        }
    }
}
