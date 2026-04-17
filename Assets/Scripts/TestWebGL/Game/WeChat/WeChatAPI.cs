using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信API基础封装类
    /// 提供微信小游戏SDK的基础功能封装
    /// </summary>
    public static class WeChatAPI
    {
        // 微信SDK是否可用
        private static bool _isWeChatAvailable = false;
        
        // 初始化状态
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化微信API
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                // 检查是否在微信环境中
                _isWeChatAvailable = CheckWeChatEnvironment();
                
                if (_isWeChatAvailable)
                {
                    Debug.Log("[WeChatAPI] 微信环境检测成功");
                    InitializeWeChatSDK();
                }
                else
                {
                    Debug.LogWarning("[WeChatAPI] 非微信环境，部分功能不可用");
                }

                _isInitialized = true;
                Debug.Log("[WeChatAPI] 微信API初始化完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WeChatAPI] 初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查微信环境
        /// </summary>
        private static bool CheckWeChatEnvironment()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            // 在微信小游戏中
            return true;
            #else
            // 在编辑器或其他环境中
            return false;
            #endif
        }

        /// <summary>
        /// 初始化微信SDK
        /// </summary>
        private static void InitializeWeChatSDK()
        {
            // 调用微信SDK初始化
            // 这里会调用WX-WASM-SDK-V2的初始化方法
            Debug.Log("[WeChatAPI] 微信SDK初始化中...");
        }

        /// <summary>
        /// 检查微信API是否可用
        /// </summary>
        public static bool IsAvailable()
        {
            return _isInitialized && _isWeChatAvailable;
        }

        /// <summary>
        /// 获取微信环境信息
        /// </summary>
        public static WeChatEnvironmentInfo GetEnvironmentInfo()
        {
            return new WeChatEnvironmentInfo
            {
                isWeChat = _isWeChatAvailable,
                isInitialized = _isInitialized,
                platform = Application.platform.ToString(),
                unityVersion = Application.unityVersion
            };
        }

        #region 微信API调用封装

        /// <summary>
        /// 调用微信API（通用方法）
        /// </summary>
        public static void CallWeChatAPI(string apiName, string parameters = null, Action<string> callback = null)
        {
            if (!IsAvailable())
            {
                Debug.LogWarning($"[WeChatAPI] API不可用: {apiName}");
                callback?.Invoke(null);
                return;
            }

            try
            {
                // 这里实现实际的API调用
                // 通过WX-WASM-SDK-V2调用微信API
                Debug.Log($"[WeChatAPI] 调用API: {apiName}");
                
                // 模拟API调用
                SimulateAPICall(apiName, parameters, callback);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WeChatAPI] API调用失败: {apiName} - {ex.Message}");
                callback?.Invoke(null);
            }
        }

        /// <summary>
        /// 模拟API调用（用于开发测试）
        /// </summary>
        private static void SimulateAPICall(string apiName, string parameters, Action<string> callback)
        {
            // 在编辑器中模拟API调用
            #if UNITY_EDITOR
            Debug.Log($"[WeChatAPI] 模拟调用: {apiName}({parameters})");
            
            // 延迟回调模拟异步操作
            UnityEditor.EditorApplication.delayCall += () =>
            {
                callback?.Invoke($"{{\"success\": true, \"api\": \"{apiName}\"}}");
            };
            #else
            // 在实际微信环境中调用真实API
            // 这里会调用WX-WASM-SDK-V2的具体方法
            #endif
        }

        #endregion

        #region 微信API方法声明

        // 登录相关
        [DllImport("__Internal")]
        private static extern void wxLogin(string callbackId);

        [DllImport("__Internal")]
        private static extern void wxGetUserInfo(string callbackId);

        [DllImport("__Internal")]
        private static extern void wxGetSetting(string callbackId);

        // 存储相关
        [DllImport("__Internal")]
        private static extern void wxSetStorage(string key, string data, string callbackId);

        [DllImport("__Internal")]
        private static extern void wxGetStorage(string key, string callbackId);

        [DllImport("__Internal")]
        private static extern void wxRemoveStorage(string key, string callbackId);

        // 分享相关
        [DllImport("__Internal")]
        private static extern void wxShareAppMessage(string title, string imageUrl, string query, string callbackId);

        // 震动相关
        [DllImport("__Internal")]
        private static extern void wxVibrateShort(string callbackId);

        [DllImport("__Internal")]
        private static extern void wxVibrateLong(string callbackId);

        // 系统信息
        [DllImport("__Internal")]
        private static extern void wxGetSystemInfo(string callbackId);

        #endregion
    }

    /// <summary>
    /// 微信环境信息
    /// </summary>
    [System.Serializable]
    public class WeChatEnvironmentInfo
    {
        public bool isWeChat;
        public bool isInitialized;
        public string platform;
        public string unityVersion;
    }

    /// <summary>
    /// 微信API回调数据
    /// </summary>
    [System.Serializable]
    public class WeChatCallbackData
    {
        public bool success;
        public string data;
        public string error;
        public int errCode;
    }
}