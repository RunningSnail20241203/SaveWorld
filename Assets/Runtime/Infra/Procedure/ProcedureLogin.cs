using GameFramework.Fsm;
using GameFramework.Procedure;

namespace SaveWorld
{
    public class ProcedureLogin : ProcedureBase
    {
        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            
            ChangeState<ProcedureMain>(procedureOwner);
        }
    }
}