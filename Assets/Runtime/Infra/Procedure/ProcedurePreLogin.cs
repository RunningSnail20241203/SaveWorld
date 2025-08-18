using GameFramework.Fsm;
using GameFramework.Procedure;

namespace SaveWorld
{
    public class ProcedurePreLogin : ProcedureBase
    {
        private int? m_LoginFormId;
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_LoginFormId = GameEntry.UI.OpenUIForm(GameEntry.Config.GetInt("UIForm.Login"), this);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            if (m_LoginFormId != null)
            {
                GameEntry.UI.CloseUIForm(m_LoginFormId.Value,this);
                m_LoginFormId = null;
            }
        }

        public void StartLogin()
        {
            ChangeState<ProcedureLogin>(m_ProcedureOwner);
        }

    }
}