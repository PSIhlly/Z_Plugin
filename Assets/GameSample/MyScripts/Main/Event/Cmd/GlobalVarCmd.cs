using UnityEngine;
using Z_Code.Form;
using Z_DesignStyle;

namespace Z_Code
{
    public sealed class GlobalVariableChangedEvent : Z_Event
    {
        public int uid;
        public string name;
        public BoxDataForm.Data oldValue;
        public BoxDataForm.Data newValue;
    }

    public class GetGlobalVarCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetGlobalVarCmd());
        }

        public override string GetName() => "GetGlobalVar";
        public override CmdBase GetNew() => new GetGlobalVarCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var name = prm[0].str;
            var value = !string.IsNullOrEmpty(name)
                && EventGlobalVariableForm.DataByName.TryGetValue(name, out var variable)
                ? variable.data?.DeepCopy() : null;
            asyncTask.res = new[] { value ?? CodeHelper.CreateBoxByNum(0) };
            return true;
        }
    }

    public class SetGlobalVarCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetGlobalVarCmd());
        }

        public override string GetName() => "SetGlobalVar";
        public override CmdBase GetNew() => new SetGlobalVarCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var name = prm[0].str;
            if (string.IsNullOrEmpty(name))
                return true;

            // Stored globals own their entire Box tree, not interpreter/local values.
            var value = prm[1].DeepCopy();
            EventGlobalVariableForm.DataByName.TryGetValue(name, out var variable);
            var previous = variable?.data;
            bool changed = !SameValue(previous, value);
            var oldValue = changed ? previous?.DeepCopy() ?? CodeHelper.CreateBoxByNum(0) : null;
            if (variable != null)
                variable.data = value;
            else
            {
                variable = new EventGlobalVariableForm.Data(-1, name, value);
                if (EventGlobalVariableForm.AddData(variable) < 0)
                    return true;
            }
            if (changed)
                Z_EventHelper.Invoke(new GlobalVariableChangedEvent
                {
                    uid = variable.uid,
                    name = variable.name,
                    oldValue = oldValue,
                    newValue = value.DeepCopy()
                });
            return true;
        }

        private static bool SameValue(BoxDataForm.Data left, BoxDataForm.Data right)
        {
            if (ReferenceEquals(left, right))
                return true;
            var leftDictionary = left?.dic;
            var rightDictionary = right?.dic;
            int count = leftDictionary?.Count ?? 0;
            if (count != (rightDictionary?.Count ?? 0))
                return false;
            if (count == 0)
                return left?.str == right?.str
                    && (left?.str != null || (left?.num ?? 0).Equals(right?.num ?? 0));
            foreach (var pair in leftDictionary)
                if (!rightDictionary.TryGetValue(pair.Key, out var other) || !SameValue(pair.Value, other))
                    return false;
            return true;
        }
    }
}
