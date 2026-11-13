using System.Collections.Generic;
using System.Globalization;
using Form;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class AddCharacterToTeamCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new AddCharacterToTeamCmd());

        public override string GetName() => "AddCharacterToTeam";
        public override CmdBase GetNew() => new AddCharacterToTeamCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var manager = PlayManager.instance;
            if (manager != null)
                manager.infoCtrl?.AddCharacterToTeam(prm != null && prm.Length > 0 ? prm[0]?.str : string.Empty);
            return true;
        }
    }

    public class RemoveCharacterFromTeamCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new RemoveCharacterFromTeamCmd());

        public override string GetName() => "RemoveCharacterFromTeam";
        public override CmdBase GetNew() => new RemoveCharacterFromTeamCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var manager = PlayManager.instance;
            if (manager != null)
                manager.infoCtrl?.RemoveCharacterFromTeam(prm != null && prm.Length > 0 ? prm[0]?.str : string.Empty);
            return true;
        }
    }

    public class DeployCharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new DeployCharacterCmd());

        public override string GetName() => "DeployCharacter";
        public override CmdBase GetNew() => new DeployCharacterCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var manager = PlayManager.instance;
            if (manager != null)
                manager.infoCtrl?.DeployCharacter(prm != null && prm.Length > 0 ? prm[0]?.str : string.Empty);
            return true;
        }
    }

    public class UndeployCharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new UndeployCharacterCmd());

        public override string GetName() => "UndeployCharacter";
        public override CmdBase GetNew() => new UndeployCharacterCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var manager = PlayManager.instance;
            if (manager != null)
                manager.infoCtrl?.UndeployCharacter(prm != null && prm.Length > 0 ? prm[0]?.str : string.Empty);
            return true;
        }
    }

    public class SetCurrentCharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new SetCurrentCharacterCmd());

        public override string GetName() => "SetCurrentCharacter";
        public override CmdBase GetNew() => new SetCurrentCharacterCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var manager = PlayManager.instance;
            if (manager != null)
                manager.infoCtrl?.SetCurrentCharacter(prm != null && prm.Length > 0 ? prm[0]?.str : string.Empty);
            return true;
        }
    }

    public class GetDeployedCharactersCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new GetDeployedCharactersCmd());

        public override string GetName() => "GetDeployedCharacters";
        public override CmdBase GetNew() => new GetDeployedCharactersCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new[] { PartyListCommand.CreateList(GameManager.instance.curProgress?.teamActive) };
            return true;
        }
    }

    public class GetTeamCharactersCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new GetTeamCharactersCmd());

        public override string GetName() => "GetTeamCharacters";
        public override CmdBase GetNew() => new GetTeamCharactersCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new[] { PartyListCommand.CreateList(GameManager.instance.curProgress?.team) };
            return true;
        }
    }

    internal static class PartyListCommand
    {
        internal static BoxDataForm.Data CreateList(List<int> members)
        {
            var result = CodeHelper.CreateBox();
            if (members == null)
                return result;

            var seen = new HashSet<int>();
            foreach (int uid in members)
            {
                if (uid <= 0 || !CharacterProductForm.DataByUid.TryGetValue(uid, out var character)
                    || character == null || !seen.Add(uid))
                    continue;

                result.dic[result.dic.Count.ToString(CultureInfo.InvariantCulture)] = CodeHelper.CreateBoxByStr(
                    GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, uid.ToString(CultureInfo.InvariantCulture)));
            }
            return result;
        }
    }
}
