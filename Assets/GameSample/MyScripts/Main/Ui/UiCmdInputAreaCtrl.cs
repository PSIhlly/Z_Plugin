using System;
using Z_Text;
using Z_Ui;
using Z_Ui.Notify;

namespace Ui.CmdInputArea
{
    public partial class UiCmdInputAreaParam
    {
        public ModCmdScope scope;
        public Action onSuccess;
    }

    public partial class UiCmdInputAreaModel
    {
        public ModCmdScope scope;
        public Action onSuccess;
    }

    public partial class UiCmdInputAreaCtrl
    {
        public override void OnCreate()
        {
            view.btn_close.onClick.AddListener(Close);
            view.btn_.onClick.AddListener(Execute);
            view.btn_tips.onClick.AddListener(() =>
            {
                string tipsKey = model.scope == ModCmdScope.Story
                    ? "storyCmdTips"
                    : "sceneCmdTips";
                string content = TextManager.instance.GetTxt(tipsKey).Replace("\\n", "\n");
                NotifyManager.instance.AddPopup(
                    TextManager.instance.GetTxt("tips"),
                    content,
                    true,
                    enableScr: true);
            });
        }

        public override void OnShow()
        {
            model.scope = param?.scope ?? ModCmdScope.Story;
            model.onSuccess = param?.onSuccess;
            view.ipt_.Set(string.Empty, false);
            view.go_close.SetActive(true);
            view.go_tips.SetActive(true);
            view.txt_title.text = model.scope == ModCmdScope.Story ? "StoryCmd" : "SceneCmd";
            view.txt_.oriText = "execute";
            UiManager.Rebuild(gameObject, true);
        }

        private void Execute()
        {
            string command = view.ipt_.text?.Trim();
            if (string.IsNullOrWhiteSpace(command))
                return;

            bool success = ModCmd.TryExecute(command, model.scope, out string result);
            NotifyManager.instance.AddTip(result);
            if (!success)
                return;

            model.onSuccess?.Invoke();
            Close();
        }
    }
}
