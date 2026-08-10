using System;
using System.Collections.Generic;
using System.Linq;
using Z_DataSystem.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Ui
{
    public static class UiLabRenderHelper
    {
        public const int AllState = 0;
        public const int UnclassifiedState = 1;
        public const int LabState = 2;
        public const int NewState = 3;

        public static IReadOnlyList<int> GetLabIds(string belong)
        {
            if (string.IsNullOrEmpty(belong) ||
                !LabForm.DatasByBelong.TryGetValue(belong, out var labs))
            {
                return Array.Empty<int>();
            }

            return labs
                .Where(lab => lab != null && lab.id != LabForm.NoneId)
                .Select(lab => new
                {
                    lab.id,
                    lab.lv1Lab,
                    lab.lv2Lab,
                    lab.lv3Lab,
                    displayName = LabForm.GetDisplayName(lab.id)
                })
                .Where(lab => !string.IsNullOrWhiteSpace(lab.displayName))
                .GroupBy(lab => (lab.lv1Lab, lab.lv2Lab, lab.lv3Lab))
                .Select(group => group.OrderBy(lab => lab.id).First())
                .OrderBy(lab => lab.displayName, StringComparer.Ordinal)
                .ThenBy(lab => lab.id)
                .Select(lab => lab.id)
                .ToList();
        }

        public static string GetText(int? labId, bool isNew)
        {
            if (isNew)
                return TextManager.instance.GetTxt("new");
            if (labId == LabForm.NoneId)
                return TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab);
            return labId.HasValue
                ? LabForm.GetDisplayName(labId.Value)
                : TextManager.instance.GetTxt("all");
        }

        public static int GetState(int? labId, bool isNew)
        {
            if (isNew)
                return NewState;
            if (!labId.HasValue)
                return AllState;
            return labId.Value == LabForm.NoneId ? UnclassifiedState : LabState;
        }

        public static void Create(string belong, Action<int> onCreated)
        {
            if (string.IsNullOrWhiteSpace(belong))
                return;

            NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("new"), true, value =>
            {
                var displayName = value?.Trim() ?? string.Empty;
                if (displayName.Length == 0)
                    return false;

                var labId = LabForm.GetOrCreateDisplayName(displayName, belong);
                if (labId == LabForm.NoneId)
                    return false;

                onCreated?.Invoke(labId);
                return true;
            });
        }

        public static void Choose(string belong, Action<int> onSelected)
        {
            if (string.IsNullOrWhiteSpace(belong))
                return;

            var items = new EntryItem();
            var unclassifiedText = TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab);
            items.Add(unclassifiedText, null, LabForm.NoneId);

            var displayNames = new HashSet<string>(StringComparer.Ordinal) { unclassifiedText };
            foreach (var labId in GetLabIds(belong))
            {
                var displayName = LabForm.GetDisplayName(labId);
                if (string.IsNullOrWhiteSpace(displayName) || !displayNames.Add(displayName))
                    continue;
                items.Add(displayName, null, labId);
            }

            NotifyManager.instance.AddChoose(
                TextManager.instance.GetTxt("label"),
                true,
                item =>
                {
                    onSelected?.Invoke(item.id);
                    return true;
                },
                items);
        }
    }
}
