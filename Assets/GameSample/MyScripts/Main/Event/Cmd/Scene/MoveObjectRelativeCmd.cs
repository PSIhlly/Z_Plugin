namespace Z_Code
{
    // Source compatibility only. Registration and implementation are shared by the scene command.
    public class MoveObjectRelativeCmd : MoveRelativeCmd
    {
        protected override bool UsesDuration => true;
        protected override bool RunInBackground => false;
        public override CmdBase GetNew() => new MoveObjectRelativeCmd();
    }
}
