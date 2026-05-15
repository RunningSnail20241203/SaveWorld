using UnityEngine;

namespace SaveWorld.Game.Core
{
    /// <summary>
    /// WXSDK 消息接收器
    /// 挂载在 WXSDKManagerHandler GameObject 上，用于接收 HTML Mock 的 SendMessage 调用
    /// </summary>
    public class WXSDKHandler : MonoBehaviour
    {
        /// <summary>
        /// 被 HTML 中的 WXWASMSDK.WXInitializeSDK() 调用
        /// </summary>
        public void Inited(string code)
        {
            Debug.Log($"[WXSDKHandler] Inited called with code: {code}");
            // WeChatManager 会通过 EventBus 监听 WeChatInitializedEvent
            // 此处不需要做额外处理，消息接收本身证明了 GameObject 存在即可
        }

        /// <summary>
        /// 其他可能的回调方法
        /// </summary>
        public void LoginSuccess(string jsonData)
        {
            Debug.Log($"[WXSDKHandler] LoginSuccess: {jsonData}");
        }

        public void ShareSuccess()
        {
            Debug.Log("[WXSDKHandler] ShareSuccess");
        }

        public void OnError(string errorInfo)
        {
            Debug.LogWarning($"[WXSDKHandler] Error: {errorInfo}");
        }
    }
}
