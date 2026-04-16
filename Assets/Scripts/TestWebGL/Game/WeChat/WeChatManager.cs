using System;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信管理器
    /// 微信API统一接入入口
    /// </summary>
    public class WeChatManager
    {
        private readonly EventBus _eventBus;
        private bool _isInitialized = false;
        private string _openId;

        public WeChatManager(EventBus eventBus)
        {
            _eventBus = eventBus;
            Initialize();
        }

        private void Initialize()
        {
            // TODO: 微信SDK初始化
            
            _isInitialized = true;
            
            _eventBus.Publish(new WeChatInitializedEvent
            {
                Success = true
            });
        }

        /// <summary>
        /// 微信登录
        /// </summary>
        public void Login()
        {
            _eventBus.Publish(new WeChatLoginStartedEvent());
            
            // TODO: 微信登录流程
            
            _openId = "user_openid_placeholder";
            
            _eventBus.Publish(new WeChatLoginCompletedEvent
            {
                OpenId = _openId,
                Success = true
            });
        }

        /// <summary>
        /// 分享游戏
        /// </summary>
        public void Share(ShareType type, string title, string imageUrl)
        {
            _eventBus.Publish(new WeChatShareStartedEvent { ShareType = type });
            
            // TODO: 微信分享API
            
            _eventBus.Publish(new WeChatShareCompletedEvent
            {
                ShareType = type,
                Success = true
            });
        }

        /// <summary>
        /// 播放激励广告
        /// </summary>
        public void ShowRewardedAd(string adUnitId)
        {
            _eventBus.Publish(new RewardedAdStartedEvent());
            
            // TODO: 微信激励广告
            
            _eventBus.Publish(new RewardedAdCompletedEvent
            {
                Success = true,
                RewardGiven = true
            });
        }

        /// <summary>
        /// 发起支付
        /// </summary>
        public void RequestPayment(string orderId, long amount)
        {
            _eventBus.Publish(new PaymentStartedEvent { OrderId = orderId });
            
            // TODO: 微信支付API
            
            _eventBus.Publish(new PaymentCompletedEvent
            {
                OrderId = orderId,
                Success = true
            });
        }
    }

    #region 类型定义

    public enum ShareType
    {
        Friends = 1,
        Moments = 2,
        Favorite = 3
    }

    #endregion

    #region 事件定义

    public class WeChatInitializedEvent : GameEvent
    {
        public bool Success;
    }

    public class WeChatLoginStartedEvent : GameEvent
    {
    }

    public class WeChatLoginCompletedEvent : GameEvent
    {
        public string OpenId;
        public bool Success;
    }

    public class WeChatShareStartedEvent : GameEvent
    {
        public ShareType ShareType;
    }

    public class WeChatShareCompletedEvent : GameEvent
    {
        public ShareType ShareType;
        public bool Success;
    }

    public class RewardedAdStartedEvent : GameEvent
    {
    }

    public class RewardedAdCompletedEvent : GameEvent
    {
        public bool Success;
        public bool RewardGiven;
    }

    public class PaymentStartedEvent : GameEvent
    {
        public string OrderId;
    }

    public class PaymentCompletedEvent : GameEvent
    {
        public string OrderId;
        public bool Success;
    }

    #endregion
}