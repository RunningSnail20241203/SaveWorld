using System;
using System.Collections.Generic;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using Runtime.Logic.Event;
using UnityGameFramework.Runtime;

namespace SaveWorld
{
    public class ProcedureGame : ProcedureBase
    {
        public override bool UseNativeDialog => false;
        private readonly Dictionary<GameMode, GameBase> m_Games = new();
        private GameBase m_CurrentGame;
        private IFsm<IProcedureManager>  m_ProcedureManager = null;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameMode gameMode = (GameMode)procedureOwner.GetData<VarByte>("GameMode").Value;
            m_CurrentGame = m_Games[gameMode];
            m_CurrentGame.Initialize();
            
            GameEntry.Event.Subscribe(LeaveGameEventArgs.EventId, OnReceiveLeaveGameEvent);
        }

        protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            m_ProcedureManager = procedureOwner;
            base.OnInit(procedureOwner);

            m_Games.Add(GameMode.Survival, new SurvivalGame());
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            GameEntry.Event.Unsubscribe(LeaveGameEventArgs.EventId, OnReceiveLeaveGameEvent);
            
            if (m_CurrentGame != null)
            {
                m_CurrentGame.Shutdown();
                m_CurrentGame = null;
            }

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (m_CurrentGame != null)
            {
                m_CurrentGame.Update(elapseSeconds, realElapseSeconds);
            }
        }

        protected override void OnDestroy(IFsm<IProcedureManager> procedureOwner)
        {
            m_Games.Clear();
            base.OnDestroy(procedureOwner);
        }

        private void OnReceiveLeaveGameEvent(object sender, GameEventArgs e)
        {
            if (e is not LeaveGameEventArgs ne) return;
            LeaveGame(m_ProcedureManager, ne.NextSceneName, ne.NextProcedureType);
        }

        private void LeaveGame(IFsm<IProcedureManager> procedureOwner, string sceneName, Type procedureType)
        {
            procedureOwner.SetData<VarString>(Constant.String.NextSceneName, sceneName);
            procedureOwner.SetData<VarType>(Constant.String.NextProcedureName, procedureType); 
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }
    }
}