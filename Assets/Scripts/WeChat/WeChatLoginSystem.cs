using System;
using UnityEngine;
using WeChatWASM;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信登录系统 - 负责登录、授权、获取用户信息
    /// 基于 WX-WASM-SDK-V2
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

        private bool _isLoggedIn = false;
        private WeChatUserInfo _userInfo = null;
        private string _loginCode = "";

        public event Action<bool, string> OnLoginCompleted;
        public event Action<WeChatUserInfo> OnUserInfoReceived;

        /// <summary>
        /// 微信登录 - 调用 wx.login 获取临时 code
        /// </summary>
        public void Login(Action<bool, string> callback = null)
        {
            if (_isLoggedIn)
            {
                Debug.Log("[WeChatLogin] 已登录");
                callback?.Invoke(true, "已登录");
                return;
            }

            Debug.Log("[WeChatLogin] 开始微信登录...");

            WX.Login(new LoginOption
            {
                success = (res) =>
                {
                    _loginCode = res.code;
                    _isLoggedIn = true;

                    Debug.Log($"[WeChatLogin] 登录成功, code: {_loginCode}");
                    OnLoginCompleted?.Invoke(true, "登录成功");
                    callback?.Invoke(true, "登录成功");

                    // 登录成功后获取用户信息
                    GetUserInfo();
                },
                fail = (res) =>
                {
                    Debug.LogError($"[WeChatLogin] 登录失败: {res.errMsg}");
                    OnLoginCompleted?.Invoke(false, res.errMsg);
                    callback?.Invoke(false, res.errMsg);
                }
            });
        }

        /// <summary>
        /// 获取用户信息（头像、昵称等）
        /// </summary>
        public void GetUserInfo(Action<WeChatUserInfo> callback = null)
        {
            Debug.Log("[WeChatLogin] 获取用户信息...");

            WX.GetSetting(new GetSettingOption
            {
                success = (res) =>
                {
                    if (res.authSetting.ContainsKey("scope.userInfo") && res.authSetting["scope.userInfo"])
                    {
                        // 已授权，直接获取
                        FetchUserInfo(callback);
                    }
                    else
                    {
                        // 未授权，通过透明按钮获取
                        Debug.Log("[WeChatLogin] 未授权，创建授权按钮");
                        RequestUserInfoViaButton(callback);
                    }
                },
                fail = (res) =>
                {
                    Debug.LogWarning($"[WeChatLogin] 获取设置失败: {res.errMsg}");
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// 通过透明授权按钮获取用户信息
        /// </summary>
        private void RequestUserInfoViaButton(Action<WeChatUserInfo> callback)
        {
            // 计算屏幕中心位置
            int x = Screen.width / 2 - 100;
            int y = Screen.height / 2 - 25;
            int width = 200;
            int height = 50;

            var button = WX.CreateUserInfoButton(x, y, width, height, "zh_CN", true);

            button.OnTap((res) =>
            {
                if (!string.IsNullOrEmpty(res.userInfo.nickName))
                {
                    _userInfo = new WeChatUserInfo
                    {
                        nickName = res.userInfo.nickName,
                        avatarUrl = res.userInfo.avatarUrl,
                        gender = (int)res.userInfo.gender,
                        city = res.userInfo.city ?? "",
                        province = res.userInfo.province ?? "",
                        country = res.userInfo.country ?? "",
                        language = res.userInfo.language ?? "zh_CN"
                    };

                    Debug.Log($"[WeChatLogin] 用户信息获取成功: {_userInfo.nickName}");
                    OnUserInfoReceived?.Invoke(_userInfo);
                    callback?.Invoke(_userInfo);
                }
                else
                {
                    Debug.LogWarning("[WeChatLogin] 用户拒绝授权或未获取到信息");
                    callback?.Invoke(null);
                }

                button.Destroy();
            });
        }

        /// <summary>
        /// 已授权情况下直接获取用户信息
        /// </summary>
        private void FetchUserInfo(Action<WeChatUserInfo> callback)
        {
            WX.GetUserInfo(new GetUserInfoOption
            {
                success = (res) =>
                {
                    _userInfo = new WeChatUserInfo
                    {
                        nickName = res.userInfo.nickName,
                        avatarUrl = res.userInfo.avatarUrl,
                        gender = (int)res.userInfo.gender,
                        city = res.userInfo.city ?? "",
                        province = res.userInfo.province ?? "",
                        country = res.userInfo.country ?? "",
                        language = res.userInfo.language ?? "zh_CN"
                    };

                    Debug.Log($"[WeChatLogin] 用户信息获取成功: {_userInfo.nickName}");
                    OnUserInfoReceived?.Invoke(_userInfo);
                    callback?.Invoke(_userInfo);
                },
                fail = (res) =>
                {
                    Debug.LogWarning($"[WeChatLogin] 获取用户信息失败: {res.errMsg}");
                    callback?.Invoke(null);
                }
            });
        }

        public bool IsLoggedIn() => _isLoggedIn;
        public string GetLoginCode() => _loginCode;
        public WeChatUserInfo GetCurrentUserInfo() => _userInfo;

        public void Logout()
        {
            _isLoggedIn = false;
            _loginCode = "";
            _userInfo = null;
            Debug.Log("[WeChatLogin] 已退出登录");
        }

        public string GetLoginInfo()
        {
            return $"登录状态：{(_isLoggedIn ? "已登录" : "未登录")}, " +
                   $"用户：{(_userInfo?.nickName ?? "未知")}";
        }
    }

    [Serializable]
    public class WeChatUserInfo
    {
        public string nickName;
        public string avatarUrl;
        public int gender;
        public string city;
        public string province;
        public string country;
        public string language;
    }
}
