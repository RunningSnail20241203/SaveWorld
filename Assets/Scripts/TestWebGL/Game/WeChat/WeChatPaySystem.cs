using System;
using UnityEngine;

namespace TestWebGL.Game.WeChat
{
    /// <summary>
    /// 微信支付系统
    /// 负责微信支付、虚拟支付等功能
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

        // 支付配置
        private const string PAYMENT_KEY = "game_payment";
        private const string PRODUCT_KEY = "game_product";
        
        // 初始化状态
        private bool _isInitialized = false;
        
        // 支付状态
        private bool _isPaying = false;
        
        // 商品列表
        private WeChatProductList _productList = null;

        // 事件
        public event Action<bool, string> OnPayCompleted;
        public event Action<bool, string, WeChatProductList> OnProductsLoaded;
        public event Action<WeChatPayResult> OnPayResult;
        public event Action<bool, string> OnWalletChecked;

        /// <summary>
        /// 初始化微信支付系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[WeChatPay] 初始化微信支付系统...");

            // 检查微信环境
            if (!WeChatAPI.IsAvailable())
            {
                Debug.LogWarning("[WeChatPay] 微信环境不可用，支付功能受限");
            }

            _isInitialized = true;
            Debug.Log("[WeChatPay] 微信支付系统初始化完成");
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        public void GetProducts(Action<bool, string, WeChatProductList> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatPay] 系统未初始化");
                callback?.Invoke(false, "系统未初始化", null);
                return;
            }

            Debug.Log("[WeChatPay] 获取商品列表...");

            // 调用微信支付API获取商品列表
            WeChatAPI.CallWeChatAPI("getAvailableWallets", null, (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning("[WeChatPay] 获取商品列表失败");
                    OnProductsLoaded?.Invoke(false, "获取商品列表失败", null);
                    callback?.Invoke(false, "获取商品列表失败", null);
                    return;
                }

                try
                {
                    var productResult = JsonUtility.FromJson<WeChatProductResult>(result);
                    
                    if (productResult.success && productResult.products != null)
                    {
                        _productList = productResult.products;
                        
                        Debug.Log($"[WeChatPay] 商品列表获取成功：{_productList.products.Count}个商品");
                        OnProductsLoaded?.Invoke(true, "获取成功", _productList);
                        callback?.Invoke(true, "获取成功", _productList);
                    }
                    else
                    {
                        Debug.LogWarning("[WeChatPay] 商品列表为空");
                        OnProductsLoaded?.Invoke(false, "商品列表为空", null);
                        callback?.Invoke(false, "商品列表为空", null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatPay] 商品列表解析失败：{ex.Message}");
                    OnProductsLoaded?.Invoke(false, "商品列表解析失败", null);
                    callback?.Invoke(false, "商品列表解析失败", null);
                }
            });
        }

        /// <summary>
        /// 发起支付
        /// </summary>
        public void Pay(string productId, string orderId = null, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatPay] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            if (_isPaying)
            {
                Debug.LogWarning("[WeChatPay] 正在支付中...");
                callback?.Invoke(false, "正在支付中");
                return;
            }

            _isPaying = true;
            Debug.Log($"[WeChatPay] 发起支付：{productId}");

            // 构建支付参数
            string payParams = BuildPayParams(productId, orderId);

            // 调用微信支付API
            WeChatAPI.CallWeChatAPI("requestPayment", payParams, (result) =>
            {
                _isPaying = false;

                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogError("[WeChatPay] 支付失败：无返回数据");
                    OnPayCompleted?.Invoke(false, "支付失败");
                    callback?.Invoke(false, "支付失败");
                    return;
                }

                try
                {
                    var payResult = JsonUtility.FromJson<WeChatPayResult>(result);
                    
                    if (payResult.success)
                    {
                        Debug.Log($"[WeChatPay] 支付成功：{productId}");
                        OnPayCompleted?.Invoke(true, "支付成功");
                        OnPayResult?.Invoke(payResult);
                        callback?.Invoke(true, "支付成功");
                    }
                    else
                    {
                        Debug.LogError($"[WeChatPay] 支付失败：{payResult.error}");
                        OnPayCompleted?.Invoke(false, payResult.error);
                        callback?.Invoke(false, payResult.error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatPay] 支付结果解析失败：{ex.Message}");
                    OnPayCompleted?.Invoke(false, "支付结果解析失败");
                    callback?.Invoke(false, "支付结果解析失败");
                }
            });
        }

        /// <summary>
        /// 虚拟支付（游戏内货币）
        /// </summary>
        public void VirtualPay(string productId, int quantity, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatPay] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            Debug.Log($"[WeChatPay] 虚拟支付：{productId} x{quantity}");

            // 构建虚拟支付参数
            string payParams = BuildVirtualPayParams(productId, quantity);

            // 调用微信虚拟支付API
            WeChatAPI.CallWeChatAPI("requestVirtualPayment", payParams, (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogError("[WeChatPay] 虚拟支付失败：无返回数据");
                    callback?.Invoke(false, "虚拟支付失败");
                    return;
                }

                try
                {
                    var payResult = JsonUtility.FromJson<WeChatPayResult>(result);
                    
                    if (payResult.success)
                    {
                        Debug.Log($"[WeChatPay] 虚拟支付成功：{productId}");
                        callback?.Invoke(true, "虚拟支付成功");
                    }
                    else
                    {
                        Debug.LogError($"[WeChatPay] 虚拟支付失败：{payResult.error}");
                        callback?.Invoke(false, payResult.error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatPay] 虚拟支付结果解析失败：{ex.Message}");
                    callback?.Invoke(false, "虚拟支付结果解析失败");
                }
            });
        }

        /// <summary>
        /// 检查钱包状态
        /// </summary>
        public void CheckWallet(Action<bool, string> callback = null)
        {
            Debug.Log("[WeChatPay] 检查钱包状态...");

            WeChatAPI.CallWeChatAPI("getAvailableWallets", null, (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                Debug.Log($"[WeChatPay] 钱包状态：{(success ? "可用" : "不可用")}");
                OnWalletChecked?.Invoke(success, success ? "钱包可用" : "钱包不可用");
                callback?.Invoke(success, success ? "钱包可用" : "钱包不可用");
            });
        }

        /// <summary>
        /// 构建支付参数
        /// </summary>
        private string BuildPayParams(string productId, string orderId)
        {
            var payData = new WeChatPayData
            {
                productId = productId,
                orderId = orderId ?? GenerateOrderId(),
                timeStamp = GetTimeStamp(),
                nonceStr = GenerateNonceStr(),
                package = "Sign=WXPay",
                signType = "MD5"
            };

            return JsonUtility.ToJson(payData);
        }

        /// <summary>
        /// 构建虚拟支付参数
        /// </summary>
        private string BuildVirtualPayParams(string productId, int quantity)
        {
            var virtualPayData = new WeChatVirtualPayData
            {
                productId = productId,
                quantity = quantity,
                timeStamp = GetTimeStamp(),
                nonceStr = GenerateNonceStr()
            };

            return JsonUtility.ToJson(virtualPayData);
        }

        /// <summary>
        /// 生成订单号
        /// </summary>
        private string GenerateOrderId()
        {
            return $"ORDER_{DateTime.Now:yyyyMMddHHmmss}_{UnityEngine.Random.Range(1000, 9999)}";
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        private string GenerateNonceStr()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] nonce = new char[16];
            for (int i = 0; i < nonce.Length; i++)
            {
                nonce[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
            }
            return new string(nonce);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        private string GetTimeStamp()
        {
            return ((int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }

        /// <summary>
        /// 获取商品信息
        /// </summary>
        public WeChatProduct GetProduct(string productId)
        {
            if (_productList == null || _productList.products == null) return null;
            
            return _productList.products.Find(p => p.productId == productId);
        }

        /// <summary>
        /// 获取所有商品
        /// </summary>
        public WeChatProductList GetProductList()
        {
            return _productList;
        }

        /// <summary>
        /// 检查是否正在支付
        /// </summary>
        public bool IsPaying()
        {
            return _isPaying;
        }

        /// <summary>
        /// 获取支付统计信息
        /// </summary>
        public string GetPayInfo()
        {
            return $"商品数量：{(_productList?.products?.Count ?? 0)}个, " +
                   $"支付状态：{(_isPaying ? "支付中" : "空闲")}, " +
                   $"微信环境：{(WeChatAPI.IsAvailable() ? "可用" : "不可用")}";
        }

        /// <summary>
        /// 清除支付数据
        /// </summary>
        public void ClearPayData()
        {
            _productList = null;
            _isPaying = false;
            Debug.Log("[WeChatPay] 支付数据已清除");
        }
    }

    /// <summary>
    /// 微信支付数据
    /// </summary>
    [System.Serializable]
    public class WeChatPayData
    {
        public string productId;
        public string orderId;
        public string timeStamp;
        public string nonceStr;
        public string package;
        public string signType;
    }

    /// <summary>
    /// 微信虚拟支付数据
    /// </summary>
    [System.Serializable]
    public class WeChatVirtualPayData
    {
        public string productId;
        public int quantity;
        public string timeStamp;
        public string nonceStr;
    }

    /// <summary>
    /// 微信支付结果
    /// </summary>
    [System.Serializable]
    public class WeChatPayResult
    {
        public bool success;
        public string orderId;
        public string transactionId;
        public string error;
        public int errCode;
    }

    /// <summary>
    /// 微信商品结果
    /// </summary>
    [System.Serializable]
    public class WeChatProductResult
    {
        public bool success;
        public WeChatProductList products;
        public string error;
    }

    /// <summary>
    /// 微信商品列表
    /// </summary>
    [System.Serializable]
    public class WeChatProductList
    {
        public System.Collections.Generic.List<WeChatProduct> products;
    }

    /// <summary>
    /// 微信商品
    /// </summary>
    [System.Serializable]
    public class WeChatProduct
    {
        public string productId;
        public string productName;
        public string description;
        public int price; // 价格（分）
        public string currency;
        public string iconUrl;
    }
}