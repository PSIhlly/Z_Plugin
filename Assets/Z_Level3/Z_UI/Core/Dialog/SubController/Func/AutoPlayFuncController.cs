using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;
using Z_Trick.BaseFunc;

namespace Z_Ui.Dialog
{
    public class AutoPlayFuncController : Z_MonoController<DialogUiBaseManager>
{
        public Button autoPlayBtn;
        public SwitchFunc autoPlaySf;
        public void Awake()
        {

            autoPlaySf.ChangeState((int)_manager.settings.autoPlaySpeed);
            autoPlayBtn.onClick.AddListener(() =>
            {
                autoPlaySf.ChangeState();
                _manager.settings.autoPlaySpeed = autoPlaySf.state;
            });
        }
    }
}
