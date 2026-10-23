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
}
