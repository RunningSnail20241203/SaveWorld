using System;
using UnityEngine;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信登录系统
    /// 负责微信用户登录、授权、获取用户信息等功能
    /// </summary>
    public class WeChatLoginSystem : MonoBehaviour
    {
        private static WeChatLoginSystem s_instance;
        public static WeChatLoginSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("WeChatLoginSystem");
                    s_instance = go.AddComponent<WeChatLoginSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 登录状态
        private bool _isLoggingIn = false;
        private bool _isLoggedIn = false;
        
        // 用户信息
        private WeChatUserInfo _userInfo = null;
        private string _loginCode = "";
        
        // 初始化状态
        private bool _isInitialized = false;

        // 事件
        public event Action<bool, string> OnLoginCompleted;
        public event Action<WeChatUserInfo> OnUserInfoReceived;
        public event Action<bool, string> OnAuthCompleted;

        /// <summary>
        /// 初始化微信登录系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[WeChatLogin] 初始化微信登录系统...");

            // 检查微信环境
            if (!WeChatAPI.IsAvailable())
            {
                Debug.LogWarning("[WeChatLogin] 微信环境不可用，登录功能受限");
            }

            _isInitialized = true;
            Debug.Log("[WeChatLogin] 微信登录系统初始化完成");
        }

        /// <summary>
        /// 微信登录
        /// </summary>
        public void Login(Action<bool, string> callback = null)
        {
            if (_isLoggingIn)
            {
                Debug.LogWarning("[WeChatLogin] 正在登录中...");
                callback?.Invoke(false, "正在登录中");
                return;
            }

            if (_isLoggedIn)
            {
                Debug.Log("[WeChatLogin] 已登录");
                callback?.Invoke(true, "已登录");
                return;
            }

            _isLoggingIn = true;
            Debug.Log("[WeChatLogin] 开始微信登录...");

            // 调用微信登录API
            WeChatAPI.CallWeChatAPI("login", null, (result) =>
            {
                _isLoggingIn = false;

                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogError("[WeChatLogin] 登录失败：无返回数据");
                    OnLoginCompleted?.Invoke(false, "登录失败");
                    callback?.Invoke(false, "登录失败");
                    return;
                }

                try
                {
                    var loginData = JsonUtility.FromJson<WeChatLoginResult>(result);
                    
                    if (loginData.success && !string.IsNullOrEmpty(loginData.code))
                    {
                        _loginCode = loginData.code;
                        _isLoggedIn = true;
                        
                        Debug.Log($"[WeChatLogin] 登录成功，code: {_loginCode}");
                        OnLoginCompleted?.Invoke(true, "登录成功");
                        callback?.Invoke(true, "登录成功");
                        
                        // 自动获取用户信息
                        GetUserInfo();
                    }
                    else
                    {
                        Debug.LogError($"[WeChatLogin] 登录失败：{loginData.error}");
                        OnLoginCompleted?.Invoke(false, loginData.error);
                        callback?.Invoke(false, loginData.error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatLogin] 登录结果解析失败：{ex.Message}");
                    OnLoginCompleted?.Invoke(false, "登录结果解析失败");
                    callback?.Invoke(false, "登录结果解析失败");
                }
            });
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public void GetUserInfo(Action<WeChatUserInfo> callback = null)
        {
            if (!_isLoggedIn)
            {
                Debug.LogWarning("[WeChatLogin] 未登录，无法获取用户信息");
                callback?.Invoke(null);
                return;
            }

            Debug.Log("[WeChatLogin] 获取用户信息...");

            // 先检查授权状态
            CheckAuthStatus((isAuthorized) =>
            {
                if (isAuthorized)
                {
                    // 已授权，直接获取用户信息
                    FetchUserInfo(callback);
                }
                else
                {
                    // 未授权，请求授权
                    RequestAuth((authSuccess) =>
                    {
                        if (authSuccess)
                        {
                            FetchUserInfo(callback);
                        }
                        else
                        {
                            Debug.LogWarning("[WeChatLogin] 用户拒绝授权");
                            callback?.Invoke(null);
                        }
                    });
                }
            });
        }

        /// <summary>
        /// 检查授权状态
        /// </summary>
        private void CheckAuthStatus(Action<bool> callback)
        {
            Debug.Log("[WeChatLogin] 检查授权状态...");

            WeChatAPI.CallWeChatAPI("getSetting", null, (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    callback?.Invoke(false);
                    return;
                }

                try
                {
                    var settingData = JsonUtility.FromJson<WeChatSettingResult>(result);
                    bool isAuthorized = settingData.authSetting != null && 
                                       settingData.authSetting.ContainsKey("scope.userInfo") &&
                                       settingData.authSetting["scope.userInfo"];
                    
                    Debug.Log($"[WeChatLogin] 授权状态：{isAuthorized}");
                    callback?.Invoke(isAuthorized);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatLogin] 授权状态解析失败：{ex.Message}");
                    callback?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// 请求授权
        /// </summary>
        private void RequestAuth(Action<bool> callback)
        {
            Debug.Log("[WeChatLogin] 请求用户授权...");

            WeChatAPI.CallWeChatAPI("authorize", "{\"scope\":\"scope.userInfo\"}", (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                Debug.Log($"[WeChatLogin] 授权结果：{success}");
                OnAuthCompleted?.Invoke(success, success ? "授权成功" : "授权失败");
                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// 获取用户信息（实际调用）
        /// </summary>
        private void FetchUserInfo(Action<WeChatUserInfo> callback)
        {
            Debug.Log("[WeChatLogin] 获取用户信息...");

            WeChatAPI.CallWeChatAPI("getUserInfo", null, (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogError("[WeChatLogin] 获取用户信息失败");
                    callback?.Invoke(null);
                    return;
                }

                try
                {
                    var userInfo = JsonUtility.FromJson<WeChatUserInfo>(result);
                    _userInfo = userInfo;
                    
                    Debug.Log($"[WeChatLogin] 用户信息获取成功：{userInfo.nickName}");
                    OnUserInfoReceived?.Invoke(userInfo);
                    callback?.Invoke(userInfo);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatLogin] 用户信息解析失败：{ex.Message}");
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// 检查登录状态
        /// </summary>
        public bool IsLoggedIn()
        {
            return _isLoggedIn;
        }

        /// <summary>
        /// 获取登录凭证
        /// </summary>
        public string GetLoginCode()
        {
            return _loginCode;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public WeChatUserInfo GetUserInfo()
        {
            return _userInfo;
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public void Logout()
        {
            _isLoggedIn = false;
            _loginCode = "";
            _userInfo = null;
            Debug.Log("[WeChatLogin] 已退出登录");
        }

        /// <summary>
        /// 获取登录统计信息
        /// </summary>
        public string GetLoginInfo()
        {
            return $"登录状态：{(_isLoggedIn ? "已登录" : "未登录")}, " +
                   $"用户：{(_userInfo?.nickName ?? "未知")}, " +
                   $"授权：{(_userInfo != null ? "已授权" : "未授权")}";
        }
    }

    /// <summary>
    /// 微信登录结果
    /// </summary>
    [System.Serializable]
    public class WeChatLoginResult
    {
        public bool success;
        public string code;
        public string error;
    }

    /// <summary>
    /// 微信设置结果
    /// </summary>
    [System.Serializable]
    public class WeChatSettingResult
    {
        public System.Collections.Generic.Dictionary<string, bool> authSetting;
    }

    /// <summary>
    /// 微信用户信息
    /// </summary>
    [System.Serializable]
    public class WeChatUserInfo
    {
        public string nickName;
        public string avatarUrl;
        public int gender; // 0：未知，1：男，2：女
        public string city;
        public string province;
        public string country;
        public string language;
    }
}