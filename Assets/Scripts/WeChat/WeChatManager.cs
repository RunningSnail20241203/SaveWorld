using System;
using SaveWorld.Game.Core;
using WeChatWASM;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信管理器 - 微信小游戏SDK统一接入入口
    /// 使用 WX-WASM-SDK-V2 (WeChatWASM.WX)
    /// AppID: wxb2624b9ca163b59f
    /// </summary>
    public class WeChatManager
    {
        private readonly EventBus _eventBus;
        private bool _isInitialized = false;
        private string _openId;

        public bool IsInitialized => _isInitialized;
        public string OpenId => _openId;

        public WeChatManager(EventBus eventBus)
        {
            _eventBus = eventBus;
            Initialize();
        }

        /// <summary>
        /// 初始化微信SDK，必须在游戏主逻辑之前调用
        /// </summary>
        private void Initialize()
        {
            WX.InitSDK((code) =>
            {
                if (code == 0)
                {
                    _isInitialized = true;

                    // 上报游戏启动
                    WX.ReportGameStart();

                    // 隐藏加载页
                    WX.HideLoadingPage();

                    _eventBus.Publish(new WeChatInitializedEvent
                    {
                        Success = true
                    });

                    UnityEngine.Debug.Log("[WeChatManager] 微信SDK初始化成功");
                }
                else
                {
                    _eventBus.Publish(new WeChatInitializedEvent
                    {
                        Success = false
                    });

                    UnityEngine.Debug.LogError($"[WeChatManager] 微信SDK初始化失败，错误码: {code}");
                }
            });
        }

        /// <summary>
        /// 微信登录
        /// </summary>
        public void Login()
        {
            if (!_isInitialized)
            {
                _eventBus.Publish(new WeChatLoginCompletedEvent { OpenId = null, Success = false });
                return;
            }

            _eventBus.Publish(new WeChatLoginStartedEvent());

            WX.Login(new LoginOption
            {
                success = (res) =>
                {
                    _openId = "pending"; // 需要后端通过 code 换取 openId
                    _eventBus.Publish(new WeChatLoginCompletedEvent
                    {
                        OpenId = res.code,
                        Success = true
                    });
                    UnityEngine.Debug.Log($"[WeChatManager] 登录成功, code: {res.code}");
                },
                fail = (res) =>
                {
                    _eventBus.Publish(new WeChatLoginCompletedEvent
                    {
                        OpenId = null,
                        Success = false
                    });
                    UnityEngine.Debug.LogError($"[WeChatManager] 登录失败: {res.errMsg}");
                }
            });
        }

        /// <summary>
        /// 分享游戏（主动调起分享面板）
        /// </summary>
        public void Share(ShareType type, string title, string imageUrl)
        {
            if (!_isInitialized) return;

            _eventBus.Publish(new WeChatShareStartedEvent { ShareType = type });

            var shareParam = new ShareAppMessageOption
            {
                title = title,
                imageUrl = imageUrl
            };

            WX.ShareAppMessage(shareParam);

            _eventBus.Publish(new WeChatShareCompletedEvent
            {
                ShareType = type,
                Success = true
            });
        }

        /// <summary>
        /// 播放激励视频广告
        /// </summary>
        public void ShowRewardedAd(string adUnitId)
        {
            if (!_isInitialized) return;

            _eventBus.Publish(new RewardedAdStartedEvent());

            var videoAd = WX.CreateRewardedVideoAd(
                new WXCreateRewardedVideoAdParam { adUnitId = adUnitId }
            );

            videoAd.OnLoad((WXADLoadResponse loadRes) =>
            {
                videoAd.Show((res) =>
                {
                    _eventBus.Publish(new RewardedAdCompletedEvent
                    {
                        Success = res != null,
                        RewardGiven = true
                    });
                });
            });

            videoAd.OnError((res) =>
            {
                _eventBus.Publish(new RewardedAdCompletedEvent
                {
                    Success = false,
                    RewardGiven = false
                });
                UnityEngine.Debug.LogError($"[WeChatManager] 激励广告加载失败: {res.errMsg}");
            });
        }

        /// <summary>
        /// 发起米大师支付
        /// </summary>
        public void RequestPayment(string orderId, long amount)
        {
            if (!_isInitialized) return;

            _eventBus.Publish(new PaymentStartedEvent { OrderId = orderId });

            WX.RequestMidasPayment(new RequestMidasPaymentOption
            {
                mode = "game",
                offerId = orderId,
                currencyType = "CNY",
                success = (res) =>
                {
                    _eventBus.Publish(new PaymentCompletedEvent
                    {
                        OrderId = orderId,
                        Success = true
                    });
                },
                fail = (res) =>
                {
                    _eventBus.Publish(new PaymentCompletedEvent
                    {
                        OrderId = orderId,
                        Success = false
                    });
                    UnityEngine.Debug.LogError($"[WeChatManager] 支付失败: {res.errMsg}");
                }
            });
        }

        /// <summary>
        /// 虚拟支付（游戏内货币购买）
        /// </summary>
        public void RequestVirtualPayment(string productId, int quantity)
        {
            if (!_isInitialized) return;

            WX.RequestMidasPayment(new RequestMidasPaymentOption
            {
                mode = "game",
                offerId = productId,
                buyQuantity = quantity,
                currencyType = "CNY",
                success = (res) =>
                {
                    UnityEngine.Debug.Log($"[WeChatManager] 虚拟支付成功: {productId} x{quantity}");
                },
                fail = (res) =>
                {
                    UnityEngine.Debug.LogError($"[WeChatManager] 虚拟支付失败: {res.errMsg}");
                }
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
