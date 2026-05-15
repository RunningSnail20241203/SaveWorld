using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SaveWorld.Game.WeChat;

namespace SaveWorld.Game.Core
{
    /// <summary>
    /// 最小化启动脚本 - 空微信小游戏验证构建管道
    /// 仅做 WX SDK 初始化 + Canvas 状态文本显示，不加载任何游戏业务系统
    /// </summary>
    public sealed class EmptyBootstrap : MonoBehaviour
    {
        private EventBus _eventBus;
        private WeChatManager _weChatManager;
        private Text _statusText;
        private bool _initialized;

        private void Awake()
        {
            // 创建 WXSDKManagerHandler（JS-to-C# SendMessage 通信必需）
            var wxHandler = new GameObject("WXSDKManagerHandler");
            wxHandler.AddComponent<WXSDKHandler>();
            DontDestroyOnLoad(wxHandler);

            // 初始化 EventBus + WeChatManager（构造函数自动调用 WX.InitSDK）
            _eventBus = new EventBus();
            _weChatManager = new WeChatManager(_eventBus);

            // 监听微信 SDK 事件
            _eventBus.Listen<WeChatInitializedEvent>(OnWeChatInitialized);
            _eventBus.Listen<WeChatLoginCompletedEvent>(OnWeChatLoginCompleted);

            // 超时兜底：3秒后若未收到回调则进入离线模式
            StartCoroutine(TimeoutFallback());

            // 设置状态文本
            SetupStatusText();
            UpdateStatus("SaveWorld\nEmpty Minigame v0.1\n\n等待微信 SDK 初始化...");

            Debug.Log("[EmptyBootstrap] Awake 完成，等待微信SDK初始化...");
        }

        private void SetupStatusText()
        {
            // 查找场景中已有的 Canvas（由 CreateEmptyScene 创建），否则新建
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            var textGo = new GameObject("StatusText");
            textGo.transform.SetParent(canvas.transform, false);
            _statusText = textGo.AddComponent<Text>();
            _statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusText.fontSize = 28;
            _statusText.color = Color.white;
            _statusText.alignment = TextAnchor.MiddleCenter;

            var rect = _statusText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.05f);
            rect.anchorMax = new Vector2(0.95f, 0.95f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            // 添加背景遮罩便于阅读
            var bg = new GameObject("StatusBg");
            bg.transform.SetParent(canvas.transform, false);
            bg.transform.SetSiblingIndex(0);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.12f, 1f);

            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
        }

        private IEnumerator TimeoutFallback()
        {
            yield return new WaitForSeconds(3f);
            if (!_initialized)
            {
                _initialized = true;
                UpdateStatus("SaveWorld\nEmpty Minigame v0.1\n\n微信 SDK 初始化超时\n离线模式（Editor 环境正常）");
                Debug.LogWarning("[EmptyBootstrap] SDK 初始化超时（3秒），离线模式");
            }
        }

        private void OnWeChatInitialized(WeChatInitializedEvent evt)
        {
            if (evt.Success)
            {
                Debug.Log("[EmptyBootstrap] 微信SDK初始化成功，发起登录...");
                _weChatManager.Login();
            }
            else
            {
                _initialized = true;
                UpdateStatus("SaveWorld\nEmpty Minigame v0.1\n\n微信 SDK 初始化失败\n离线模式");
                Debug.LogWarning("[EmptyBootstrap] SDK初始化失败，离线模式");
            }
        }

        private void OnWeChatLoginCompleted(WeChatLoginCompletedEvent evt)
        {
            _initialized = true;
            if (evt.Success)
            {
                var openId = _weChatManager.OpenId ?? "N/A";
                UpdateStatus($"SaveWorld\nEmpty Minigame v0.1\n\nSDK 初始化成功\n登录成功\nOpenID: {openId}");
            }
            else
            {
                UpdateStatus("SaveWorld\nEmpty Minigame v0.1\n\nSDK 初始化成功\n登录失败（离线模式）");
            }
        }

        private void UpdateStatus(string msg)
        {
            if (_statusText != null)
                _statusText.text = msg;
        }

        private void Update()
        {
            // 驱动事件处理
            _eventBus?.ProcessEvents();
        }
    }
}
