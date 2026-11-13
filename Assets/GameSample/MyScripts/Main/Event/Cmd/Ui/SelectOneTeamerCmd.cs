using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Form;
using Ui.Notify;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_Text;
using Z_Ui;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SelectOneTeamerCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new SelectOneTeamerCmd());

        public override string GetName() => "SelectOneTeamer";
        public override CmdBase GetNew() => new SelectOneTeamerCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new[] { CodeHelper.CreateBoxByStr(string.Empty) };
            var progress = GameManager.instance.curProgress;
            if (progress?.team == null)
                return true;

            var candidates = new Dictionary<int, CharacterProductForm.Data>();
            var items = new EntryItem();
            foreach (int uid in progress.team.Distinct().OrderBy(uid => uid))
            {
                if (uid <= 0 || !CharacterProductForm.DataByUid.TryGetValue(uid, out var character) || character == null)
                    continue;
                candidates.Add(uid, character);
                // EntryItem.Add keys by display name; use UID keys so names may repeat.
                string key = uid.ToString(CultureInfo.InvariantCulture);
                TexAssetForm.DataById.TryGetValue(character.avatarTex, out var avatar);
                items.subs[key] = new EntryItem
                {
                    content = character.name, sprite = avatar?.GetSprite(), id = uid,
                    parent = items, deepth = 1
                };
            }
            if (candidates.Count == 0)
                return true;

            bool finished = false;
            var choice = NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose character"), false, item =>
            {
                if (finished || asyncTask.IsCancelled())
                    return true;
                if (item == null || !candidates.TryGetValue(item.id, out var selected))
                    return false;

                finished = true;
                if (ReferenceEquals(GameManager.instance.curProgress, progress) && progress.team != null
                    && progress.team.Contains(selected.uid)
                    && CharacterProductForm.DataByUid.TryGetValue(selected.uid, out var current)
                    && ReferenceEquals(current, selected))
                    asyncTask.res[0] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(
                        GlobalEventHelper.CHARACTER, selected.uid.ToString(CultureInfo.InvariantCulture)));
                asyncTask.Complete();
                return true;
            }, items);
            var window = UiManager.instance.GetUi<UiNotifyCtrl>();
            GameManager.instance.evtCtrl.StartTask(asyncTask, () =>
            {
                if (finished)
                    return true;
                if (!ReferenceEquals(GameManager.instance.curProgress, progress) || window == null
                    || !window.active || !window.model.chooseInfos.Contains(choice))
                {
                    finished = true;
                    if (window != null && window.model.chooseInfos.Contains(choice))
                        window.RemoveChoose(choice.id);
                    asyncTask.Complete();
                    return true;
                }
                return false;
            }, () =>
            {
                finished = true;
                if (window != null && window.model.chooseInfos.Contains(choice))
                    window.RemoveChoose(choice.id);
            });
            return false;
        }
    }
}
