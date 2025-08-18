using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace SaveWorld
{
    public class ProcedureMain : ProcedureBase
    {
        private int? m_MainFormId;

        public void Logout()
        {
            m_ProcedureOwner.SetData<VarInt32>(Constant.String.NextSceneId,
                GameEntry.Config.GetInt(Constant.Scene.LoginScene));
            m_ProcedureOwner.SetData<VarType>(Constant.String.NextProcedureName, typeof(ProcedurePreLogin));
            ChangeState<ProcedureChangeScene>(m_ProcedureOwner);
        }

        public void StartGame(GameMode gameMode)
        {
            m_ProcedureOwner.SetData<VarInt32>(Constant.String.NextSceneId,
                GameEntry.Config.GetInt(Constant.Scene.GameScene));
            m_ProcedureOwner.SetData<VarType>(Constant.String.NextProcedureName, typeof(ProcedureGame));
            m_ProcedureOwner.SetData<VarByte>(Constant.String.CurrentGameMode, (byte)gameMode);
            ChangeState<ProcedureChangeScene>(m_ProcedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_MainFormId = GameEntry.UI.OpenUIForm(GameEntry.Config.GetInt("UIForm.Main"), this);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            if (m_MainFormId != null)
            {
                GameEntry.UI.CloseUIForm(m_MainFormId.Value, this);
                m_MainFormId = null;
            }
        }
    }
}