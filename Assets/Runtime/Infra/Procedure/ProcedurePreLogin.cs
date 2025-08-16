using GameFramework.Fsm;
using GameFramework.Procedure;

namespace SaveWorld
{
    public class ProcedurePreLogin : ProcedureBase
    {
        public override bool UseNativeDialog => false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.UI.OpenUIForm(UIFormId.LoginForm);
        }
    }
}