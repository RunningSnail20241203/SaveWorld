using System;
using UnityEngine;
using WeChatWASM;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信支付系统 - 米大师支付 + 虚拟支付
    /// 基于 WX.RequestMidasPayment / WX.RequestVirtualPayment
    /// </summary>
    public class WeChatPaySystem : MonoBehaviour
    {
        private static WeChatPaySystem s_instance;
        public static WeChatPaySystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("WeChatPaySystem");
                    s_instance = go.AddComponent<WeChatPaySystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        private bool _isPaying = false;
        private string _offerId = "";

        public event Action<bool, string, string> OnPayCompleted;

        /// <summary>
        /// 设置米大师 offerId
        /// </summary>
        public void SetOfferId(string offerId)
        {
            _offerId = offerId;
        }

        /// <summary>
        /// 从 WeChatConfigManager 加载支付配置
        /// </summary>
        public void LoadConfig()
        {
            _offerId = WeChatConfigManager.MidasOfferId;
            Debug.Log($"[WeChatPay] 配置加载完成: offerId={(_offerId.Length > 0 ? _offerId : "未配置")}");
        }

        /// <summary>
        /// 发起米大师支付（购买道具）
        /// signData 需要通过后端签名生成，这里只做客户端调用
        /// </summary>
        public void Pay(string productId, string signData, string paySig, string signature, Action<bool, string> callback = null)
        {
            if (_isPaying)
            {
                Debug.LogWarning("[WeChatPay] 正在支付中...");
                callback?.Invoke(false, "正在支付中");
                return;
            }

            _isPaying = true;

            string orderId = GenerateOrderId();
            Debug.Log($"[WeChatPay] 发起支付, 订单: {orderId}");

            WX.RequestMidasPaymentGameItem(new RequestMidasPaymentGameItemOption
            {
                signData = signData,
                paySig = paySig,
                signature = signature,
                success = (res) =>
                {
                    _isPaying = false;
                    Debug.Log($"[WeChatPay] 支付成功: {productId}");
                    OnPayCompleted?.Invoke(true, orderId, "支付成功");
                    callback?.Invoke(true, "支付成功");
                },
                fail = (res) =>
                {
                    _isPaying = false;
                    Debug.LogError($"[WeChatPay] 支付失败: {res.errMsg}");
                    OnPayCompleted?.Invoke(false, orderId, res.errMsg);
                    callback?.Invoke(false, res.errMsg);
                }
            });
        }

        /// <summary>
        /// 虚拟支付（游戏内货币购买）
        /// </summary>
        public void VirtualPay(string offerId, int quantity, string signData, string paySig, string signature, Action<bool, string> callback = null)
        {
            Debug.Log($"[WeChatPay] 虚拟支付: {offerId} x{quantity}");

            WX.RequestVirtualPayment(new RequestVirtualPaymentOption
            {
                mode = "goods",
                signData = signData,
                paySig = paySig,
                signature = signature,
                success = (res) =>
                {
                    Debug.Log($"[WeChatPay] 虚拟支付成功: {offerId}");
                    callback?.Invoke(true, "虚拟支付成功");
                },
                fail = (res) =>
                {
                    Debug.LogError($"[WeChatPay] 虚拟支付失败: {res.errMsg}");
                    callback?.Invoke(false, res.errMsg);
                }
            });
        }

        public bool IsPaying() => _isPaying;

        private string GenerateOrderId()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int random = UnityEngine.Random.Range(10000, 99999);
            return $"ORDER_{timestamp}_{random}";
        }

        public string GetPayInfo()
        {
            return $"支付状态: {(_isPaying ? "支付中" : "空闲")}, offerId: {(_offerId.Length > 0 ? _offerId : "未设置")}";
        }
    }
}
