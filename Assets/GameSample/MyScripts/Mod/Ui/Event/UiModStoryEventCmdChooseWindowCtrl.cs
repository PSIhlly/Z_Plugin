using System;
using System.Collections.Generic;
using Form;
using UnityEngine;
using UnityEngine.UI;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.ModStoryEventCmdChooseWindow
{
    public partial class UiModStoryEventCmdChooseWindowParam
    {
        public SceneEventType type;
        public string returnType;
        public Action<EntryItem> onChoose;
    }

    public partial class UiModStoryEventCmdChooseWindowModel
    {
        public readonly List<UiModStoryEventCmdChooseWindowCtrl.CommandGroup> groups =
            new List<UiModStoryEventCmdChooseWindowCtrl.CommandGroup>();
        public int selectedGroup;
        public Action<EntryItem> onChoose;
    }

    public partial class UiModStoryEventCmdChooseWindowCtrl
    {
        private TemplateButtonList labRows;
        private TemplateButtonList commandRows;

        public override void OnCreate()
        {
            labRows = new TemplateButtonList(view.btn_lab);
            commandRows = new TemplateButtonList(view.btn_);
            view.btn_close.onClick.AddListener(Close);
            view.btn_bbg.onClick.AddListener(Close);
        }

        public override void OnShow()
        {
            if (param == null)
            {
                Close();
                return;
            }

            model.onChoose = param.onChoose;
            model.groups.Clear();

            var commands = GameManager.instance.evtCtrl.GetCmdEntry(param.type, param.returnType, out _);
            foreach (var category in commands.subs)
            {
                var group = new CommandGroup(category.Key);
                foreach (var item in category.Value.subs.Values)
                    group.options.Add(new CommandOption(item.content, item));
                if (group.options.Count > 0)
                    model.groups.Add(group);
            }

            var programs = GameManager.instance.evtCtrl.GetEventEntry(param.type, param.returnType);
            foreach (var category in programs.subs)
            {
                var group = new CommandGroup("#" + category.Key);
                foreach (var item in category.Value.subs.Values)
                {
                    if (!EventProgramDataForm.DataByUid.TryGetValue(item.id, out var program))
                        continue;
                    // Existing editor callbacks distinguish Programs from GameCmds by UID.
                    group.options.Add(new CommandOption(item.content,
                        new EntryItem { content = program.name, id = -1 }));
                }
                if (group.options.Count > 0)
                    model.groups.Add(group);
            }

            model.selectedGroup = 0;
            labRows.Render(model.groups.Count, (index, row) =>
            {
                row.text.text = model.groups[index].name;
                row.button.onClick.RemoveAllListeners();
                var groupIndex = index;
                row.button.onClick.AddListener(() => SelectGroup(groupIndex));
                row.state?.ChangeState(index == model.selectedGroup ? 1 : 0);
            });
            SelectGroup(0);
        }

        public override void OnDisable()
        {
            model.onChoose = null;
            model.groups.Clear();
        }

        private void SelectGroup(int index)
        {
            model.selectedGroup = index;
            for (var i = 0; i < model.groups.Count; i++)
                labRows.Get(i).state?.ChangeState(i == index ? 1 : 0);

            var options = index < model.groups.Count
                ? model.groups[index].options
                : null;
            commandRows.Render(options?.Count ?? 0, (itemIndex, row) =>
            {
                var option = options[itemIndex];
                row.text.text = option.name;
                row.button.onClick.RemoveAllListeners();
                row.button.onClick.AddListener(() =>
                {
                    var callback = model.onChoose;
                    Close();
                    callback?.Invoke(option.item);
                });
            });
        }

        public sealed class CommandGroup
        {
            public readonly string name;
            public readonly List<CommandOption> options = new List<CommandOption>();

            public CommandGroup(string name) => this.name = name;
        }

        public sealed class CommandOption
        {
            public readonly string name;
            public readonly EntryItem item;

            public CommandOption(string name, EntryItem item)
            {
                this.name = name;
                this.item = item;
            }
        }

        // This window's two list templates are plain buttons, not generated UiHolders.
        // Reuse their instances and let their existing LayoutGroups place them.
        private sealed class TemplateButtonList
        {
            private readonly GameObject template;
            private readonly ScrollRect scroll;
            private readonly List<ButtonRow> rows = new List<ButtonRow>();

            public TemplateButtonList(Btn templateButton)
            {
                template = templateButton.transform.parent.gameObject;
                scroll = templateButton.GetComponentInParent<ScrollRect>();
                template.SetActive(false);
            }

            public ButtonRow Get(int index) => rows[index];

            public void Render(int count, Action<int, ButtonRow> bind)
            {
                while (rows.Count < count)
                {
                    var instance = UnityEngine.Object.Instantiate(template, template.transform.parent);
                    rows.Add(new ButtonRow(instance));
                }

                for (var i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (i < count)
                    {
                        bind(i, row);
                        row.root.SetActive(true);
                    }
                    else
                    {
                        row.root.SetActive(false);
                    }
                }

                var content = scroll.content;
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                var height = LayoutUtility.GetPreferredHeight(content);
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    Mathf.Max(scroll.viewport.rect.height, height));
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                scroll.verticalNormalizedPosition = 1f;
            }
        }

        private sealed class ButtonRow
        {
            public readonly GameObject root;
            public readonly Btn button;
            public readonly Txt text;
            public readonly Sta state;

            public ButtonRow(GameObject root)
            {
                this.root = root;
                button = root.GetComponentInChildren<Btn>(true);
                text = root.GetComponentInChildren<Txt>(true);
                state = root.GetComponentInChildren<Sta>(true);
            }
        }
    }
}
