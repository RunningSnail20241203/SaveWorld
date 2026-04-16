using System;
using System.Collections.Generic;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.Social
{
    /// <summary>
    /// 社交系统
    /// 好友、体力赠送、排行榜、分享
    /// </summary>
    public class SocialSystem
    {
        private readonly EventBus _eventBus;
        private readonly StateMutator _stateMutator;

        public SocialSystem(EventBus eventBus, StateMutator stateMutator)
        {
            _eventBus = eventBus;
            _stateMutator = stateMutator;
            
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            _eventBus.Listen<SendStaminaRequestEvent>(OnSendStaminaRequest);
            _eventBus.Listen<ClaimStaminaRequestEvent>(OnClaimStaminaRequest);
            _eventBus.Listen<ShareGameRequestEvent>(OnShareGameRequest);
            _eventBus.Listen<UpdateRankingRequestEvent>(OnUpdateRankingRequest);
        }

        private void OnSendStaminaRequest(SendStaminaRequestEvent e)
        {
            LogSocialEvent("stamina_sent", new Dictionary<string, object>
            {
                {"friend_id", e.FriendId},
                {"amount", e.Amount}
            });
            
            _eventBus.Publish(new StaminaSentEvent
            {
                FriendId = e.FriendId,
                Amount = e.Amount,
                Success = true
            });
        }

        private void OnClaimStaminaRequest(ClaimStaminaRequestEvent e)
        {
            var state = _stateMutator.CurrentState;
            int claimAmount = 5;
            
            var newPlayer = state.Player;
            newPlayer.Stamina = Math.Min(newPlayer.MaxStamina, newPlayer.Stamina + claimAmount);
            
            _stateMutator.UpdateState(state.Cells, newPlayer);
            
            LogSocialEvent("stamina_claimed", new Dictionary<string, object> {{"amount", claimAmount}});
            
            _eventBus.Publish(new StaminaClaimedEvent
            {
                Amount = claimAmount,
                NewStamina = newPlayer.Stamina
            });
        }

        private void OnShareGameRequest(ShareGameRequestEvent e)
        {
            LogSocialEvent("game_shared", new Dictionary<string, object>
            {
                {"share_type", e.ShareType.ToString()}
            });
            
            // 分享奖励
            var state = _stateMutator.CurrentState;
            var newPlayer = state.Player;
            newPlayer.Coins += 100;
            
            _stateMutator.UpdateState(state.Cells, newPlayer);
            
            _eventBus.Publish(new GameSharedEvent
            {
                ShareType = e.ShareType,
                CoinReward = 100
            });
        }

        private void OnUpdateRankingRequest(UpdateRankingRequestEvent e)
        {
            LogSocialEvent("ranking_updated");
        }

        private void LogSocialEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            _eventBus.Publish(new SocialActionEvent
            {
                ActionType = eventName,
                Parameters = parameters ?? new Dictionary<string, object>()
            });
        }

        /// <summary>
        /// 领取每日赠送体力
        /// </summary>
        public void ClaimDailyStamina()
        {
            _eventBus.Publish(new ClaimStaminaRequestEvent());
        }

        /// <summary>
        /// 分享游戏
        /// </summary>
        public void ShareGame(ShareType type)
        {
            _eventBus.Publish(new ShareGameRequestEvent { ShareType = type });
        }
    }

    #region 类型定义

    public enum ShareType
    {
        Moments = 1,
        Friends = 2,
        Group = 3
    }

    #endregion

    #region 事件定义

    public class SendStaminaRequestEvent : GameEvent
    {
        public string FriendId;
        public int Amount;
    }

    public class StaminaSentEvent : GameEvent
    {
        public string FriendId;
        public int Amount;
        public bool Success;
    }

    public class ClaimStaminaRequestEvent : GameEvent
    {
    }

    public class StaminaClaimedEvent : GameEvent
    {
        public int Amount;
        public int NewStamina;
    }

    public class ShareGameRequestEvent : GameEvent
    {
        public ShareType ShareType;
    }

    public class GameSharedEvent : GameEvent
    {
        public ShareType ShareType;
        public int CoinReward;
    }

    public class UpdateRankingRequestEvent : GameEvent
    {
        public long Score;
    }

    public class SocialActionEvent : GameEvent
    {
        public string ActionType;
        public Dictionary<string, object> Parameters;
    }

    #endregion
}