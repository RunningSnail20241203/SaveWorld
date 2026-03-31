using System;
using UnityEngine;

namespace TestWebGL.Game.WeChat
{
    /// <summary>
    /// 微信管理器
    /// 统一管理所有微信相关系统
    /// </summary>
    public class WeChatManager : MonoBehaviour
    {
        private static WeChatManager s_instance;
        public static WeChatManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("WeChatManager");
                    s_instance = go.AddComponent<WeChatManager>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 微信系统引用
        private WeChatLoginSystem _loginSystem;
        private WeChatStorageSystem _storageSystem;
        private WeChatShareSystem _shareSystem;
        private WeChatSocialSystem _socialSystem;
        private WeChatPaySystem _paySystem;
        private WeChatAdSystem _adSystem;
        
        // 初始化状态
        private bool _isInitialized = false;

        // 事件
        public event Action<bool, string> OnWeChatInitialized;
        public event Action<bool, string> OnWeChatLoginCompleted;
        public event Action<bool, string> OnWeChatShareCompleted;

        /// <summary>
        /// 初始化微信管理器
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[WeChatManager] 初始化微信管理器...");

            try
            {
                // 初始化微信API基础层
                WeChatAPI.Initialize();

                // 初始化各个微信系统
                InitializeWeChatSystems();

                _isInitialized = true;
                Debug.Log("[WeChatManager] 微信管理器初始化完成");
                OnWeChatInitialized?.Invoke(true, "微信管理器初始化成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WeChatManager] 初始化失败：{ex.Message}");
                OnWeChatInitialized?.Invoke(false, $"初始化失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 初始化微信各个系统
        /// </summary>
        private void InitializeWeChatSystems()
        {
            Debug.Log("[WeChatManager] 初始化微信子系统...");

            // 初始化登录系统
            _loginSystem = WeChatLoginSystem.Instance;
            _loginSystem.Initialize();
            _loginSystem.OnLoginCompleted += HandleLoginCompleted;

            // 初始化存储系统
            _storageSystem = WeChatStorageSystem.Instance;
            _storageSystem.Initialize();

            // 初始化分享系统
            _shareSystem = WeChatShareSystem.Instance;
            _shareSystem.Initialize();
            _shareSystem.OnShareCompleted += HandleShareCompleted;

            // 初始化社交系统
            _socialSystem = WeChatSocialSystem.Instance;
            _socialSystem.Initialize();

            // 初始化支付系统
            _paySystem = WeChatPaySystem.Instance;
            _paySystem.Initialize();

            // 初始化广告系统
            _adSystem = WeChatAdSystem.Instance;
            _adSystem.Initialize();

            Debug.Log("[WeChatManager] 微信子系统初始化完成");
        }

        /// <summary>
        /// 处理登录完成事件
        /// </summary>
        private void HandleLoginCompleted(bool success, string message)
        {
            Debug.Log($"[WeChatManager] 登录完成：{success} - {message}");
            OnWeChatLoginCompleted?.Invoke(success, message);
        }

        /// <summary>
        /// 处理分享完成事件
        /// </summary>
        private void HandleShareCompleted(bool success, string message)
        {
            Debug.Log($"[WeChatManager] 分享完成：{success} - {message}");
            OnWeChatShareCompleted?.Invoke(success, message);
        }

        /// <summary>
        /// 微信登录
        /// </summary>
        public void Login(Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化");
                return;
            }

            _loginSystem.Login(callback);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public void GetUserInfo(Action<WeChatUserInfo> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(null);
                return;
            }

            _loginSystem.GetUserInfo(callback);
        }

        /// <summary>
        /// 保存本地数据
        /// </summary>
        public void SaveLocalData(string key, string data, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化");
                return;
            }

            _storageSystem.SaveLocal(key, data, callback);
        }

        /// <summary>
        /// 加载本地数据
        /// </summary>
        public void LoadLocalData(string key, Action<bool, string, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化", null);
                return;
            }

            _storageSystem.LoadLocal(key, callback);
        }

        /// <summary>
        /// 分享到朋友
        /// </summary>
        public void ShareToFriend(string title, string description = null, string imageUrl = null, string query = null, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化");
                return;
            }

            _shareSystem.ShareToFriend(title, description, imageUrl, query, callback);
        }

        /// <summary>
        /// 分享游戏成就
        /// </summary>
        public void ShareAchievement(string achievementTitle, string achievementDescription, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化");
                return;
            }

            _shareSystem.ShareAchievement(achievementTitle, achievementDescription, callback);
        }

        /// <summary>
        /// 获取好友数据
        /// </summary>
        public void GetFriends(Action<bool, string, System.Collections.Generic.List<WeChatFriendData>> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化", null);
                return;
            }

            _socialSystem.GetFriends(callback);
        }

        /// <summary>
        /// 获取排行榜
        /// </summary>
        public void GetRanking(Action<bool, string, System.Collections.Generic.List<WeChatRankingData>> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化", null);
                return;
            }

            _socialSystem.GetRanking(callback);
        }

        /// <summary>
        /// 发起支付
        /// </summary>
        public void Pay(string productId, string orderId = null, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化");
                return;
            }

            _paySystem.Pay(productId, orderId, callback);
        }

        /// <summary>
        /// 显示激励视频广告
        /// </summary>
        public void ShowRewardedVideoAd(Action<bool, int> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, 0);
                return;
            }

            _adSystem.ShowRewardedVideoAd(callback);
        }

        /// <summary>
        /// 显示横幅广告
        /// </summary>
        public void ShowBannerAd(Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化");
                return;
            }

            _adSystem.ShowBannerAd(callback);
        }

        /// <summary>
        /// 隐藏横幅广告
        /// </summary>
        public void HideBannerAd(Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatManager] 微信管理器未初始化");
                callback?.Invoke(false, "微信管理器未初始化");
                return;
            }

            _adSystem.HideBannerAd(callback);
        }

        /// <summary>
        /// 检查是否已登录
        /// </summary>
        public bool IsLoggedIn()
        {
            return _isInitialized && _loginSystem.IsLoggedIn();
        }

        /// <summary>
        /// 获取登录用户信息
        /// </summary>
        public WeChatUserInfo GetLoginUserInfo()
        {
            return _isInitialized ? _loginSystem.GetUserInfo() : null;
        }

        /// <summary>
        /// 获取微信环境信息
        /// </summary>
        public WeChatEnvironmentInfo GetEnvironmentInfo()
        {
            return WeChatAPI.GetEnvironmentInfo();
        }

        /// <summary>
        /// 获取所有系统统计信息
        /// </summary>
        public string GetAllSystemsInfo()
        {
            if (!_isInitialized)
            {
                return "微信管理器未初始化";
            }

            return $"登录：{_loginSystem.GetLoginInfo()}\n" +
                   $"存储：{_storageSystem.GetStorageInfo()}\n" +
                   $"分享：{_shareSystem.GetShareInfo()}\n" +
                   $"社交：{_socialSystem.GetSocialInfo()}\n" +
                   $"支付：{_paySystem.GetPayInfo()}\n" +
                   $"广告：{_adSystem.GetAdInfo()}";
        }

        /// <summary>
        /// 检查微信环境是否可用
        /// </summary>
        public bool IsWeChatAvailable()
        {
            return WeChatAPI.IsAvailable();
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public void Logout()
        {
            if (_isInitialized)
            {
                _loginSystem.Logout();
                Debug.Log("[WeChatManager] 已退出登录");
            }
        }

        /// <summary>
        /// 清除所有微信数据
        /// </summary>
        public void ClearAllWeChatData()
        {
            if (_isInitialized)
            {
                _socialSystem.ClearSocialData();
                _paySystem.ClearPayData();
                _adSystem.ClearAdData();
                Debug.Log("[WeChatManager] 所有微信数据已清除");
            }
        }

        private void OnDestroy()
        {
            // 清理事件订阅
            if (_loginSystem != null)
            {
                _loginSystem.OnLoginCompleted -= HandleLoginCompleted;
            }

            if (_shareSystem != null)
            {
                _shareSystem.OnShareCompleted -= HandleShareCompleted;
            }
        }
    }
}