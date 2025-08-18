using System.Collections.Generic;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace SaveWorld
{
    public class ProcedureGame : ProcedureBase
    {
        private readonly Dictionary<GameMode, GameBase> m_Games = new();
        private GameBase m_CurrentGame;
        private int? m_SettlementFormId;
        private int? m_GameFormId;


        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameMode gameMode = (GameMode)procedureOwner.GetData<VarByte>(Constant.String.CurrentGameMode).Value;
            m_CurrentGame = m_Games[gameMode];
            m_CurrentGame.Initialize();

            m_GameFormId = GameEntry.UI.OpenUIForm(GameEntry.Config.GetInt(Constant.UI.GameForm), this);
        }

        protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);

            m_Games.Add(GameMode.PlaySingle, new PlaySingleGame());
            m_Games.Add(GameMode.PlayMatch, new PlayMatchGame());
            m_Games.Add(GameMode.PlayRoom, new PlayRoomGame());
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (m_CurrentGame != null)
            {
                m_CurrentGame.Update(elapseSeconds, realElapseSeconds);

                if (m_CurrentGame.GameOver && m_SettlementFormId == null)
                {
                    StartSettle();
                }
            }
        }

        protected override void OnDestroy(IFsm<IProcedureManager> procedureOwner)
        {
            m_Games.Clear();
            base.OnDestroy(procedureOwner);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            if (m_CurrentGame != null)
            {
                m_CurrentGame.Shutdown();
                m_CurrentGame = null;
            }

            if (m_GameFormId != null)
            {
                GameEntry.UI.CloseUIForm(m_GameFormId.Value, this);
                m_GameFormId = null;
            }

            if (m_SettlementFormId != null)
            {
                GameEntry.UI.CloseUIForm(m_SettlementFormId.Value, this);
                m_SettlementFormId = null;
            }

            base.OnLeave(procedureOwner, isShutdown);
        }

        public void StartSettle()
        {
            m_SettlementFormId = GameEntry.UI.OpenUIForm(GameEntry.Config.GetInt(Constant.UI.SettlementForm), this);
        }

        public void LeaveGame()
        {
            m_ProcedureOwner.SetData<VarInt32>(Constant.String.NextSceneId,
                GameEntry.Config.GetInt(Constant.Scene.MainScene));
            m_ProcedureOwner.SetData<VarType>(Constant.String.NextProcedureName, typeof(ProcedureMain));
            ChangeState<ProcedureChangeScene>(m_ProcedureOwner);
        }
    }
}