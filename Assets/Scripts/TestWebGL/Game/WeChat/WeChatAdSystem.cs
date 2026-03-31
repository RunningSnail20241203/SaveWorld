using System;
using UnityEngine;

namespace TestWebGL.Game.WeChat
{
    /// <summary>
    /// 微信广告系统
    /// 负责微信广告、激励视频、横幅广告等功能
    /// </summary>
    public class WeChatAdSystem : MonoBehaviour
    {
        private static WeChatAdSystem s_instance;
        public static WeChatAdSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("WeChatAdSystem");
                    s_instance = go.AddComponent<WeChatAdSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 广告配置
        private const string BANNER_AD_ID = "your_banner_ad_id";
        private const string REWARDED_VIDEO_AD_ID = "your_rewarded_video_ad_id";
        private const string INTERSTITIAL_AD_ID = "your_interstitial_ad_id";
        
        // 初始化状态
        private bool _isInitialized = false;
        
        // 广告状态
        private bool _isBannerLoaded = false;
        private bool _isRewardedVideoLoaded = false;
        private bool _isInterstitialLoaded = false;
        
        // 广告实例
        private WeChatBannerAd _bannerAd = null;
        private WeChatRewardedVideoAd _rewardedVideoAd = null;
        private WeChatInterstitialAd _interstitialAd = null;

        // 事件
        public event Action<bool, string> OnBannerAdLoaded;
        public event Action<bool, string> OnRewardedVideoAdLoaded;
        public event Action<bool, string> OnInterstitialAdLoaded;
        public event Action<bool, string> OnAdShown;
        public event Action<bool, string> OnAdClosed;
        public event Action<bool, int> OnRewardedVideoCompleted;

        /// <summary>
        /// 初始化微信广告系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[WeChatAd] 初始化微信广告系统...");

            // 检查微信环境
            if (!WeChatAPI.IsAvailable())
            {
                Debug.LogWarning("[WeChatAd] 微信环境不可用，广告功能受限");
            }

            // 预加载广告
            PreloadAds();

            _isInitialized = true;
            Debug.Log("[WeChatAd] 微信广告系统初始化完成");
        }

        /// <summary>
        /// 预加载广告
        /// </summary>
        private void PreloadAds()
        {
            Debug.Log("[WeChatAd] 预加载广告...");

            // 预加载横幅广告
            LoadBannerAd();

            // 预加载激励视频广告
            LoadRewardedVideoAd();

            // 预加载插屏广告
            LoadInterstitialAd();
        }

        /// <summary>
        /// 加载横幅广告
        /// </summary>
        public void LoadBannerAd(Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatAd] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            Debug.Log("[WeChatAd] 加载横幅广告...");

            // 构建横幅广告参数
            string adParams = BuildBannerAdParams();

            // 调用微信横幅广告API
            WeChatAPI.CallWeChatAPI("createBannerAd", adParams, (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning("[WeChatAd] 横幅广告加载失败");
                    OnBannerAdLoaded?.Invoke(false, "横幅广告加载失败");
                    callback?.Invoke(false, "横幅广告加载失败");
                    return;
                }

                try
                {
                    var adResult = JsonUtility.FromJson<WeChatAdResult>(result);
                    
                    if (adResult.success)
                    {
                        _isBannerLoaded = true;
                        _bannerAd = adResult.bannerAd;
                        
                        Debug.Log("[WeChatAd] 横幅广告加载成功");
                        OnBannerAdLoaded?.Invoke(true, "横幅广告加载成功");
                        callback?.Invoke(true, "横幅广告加载成功");
                    }
                    else
                    {
                        Debug.LogWarning($"[WeChatAd] 横幅广告加载失败：{adResult.error}");
                        OnBannerAdLoaded?.Invoke(false, adResult.error);
                        callback?.Invoke(false, adResult.error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatAd] 横幅广告结果解析失败：{ex.Message}");
                    OnBannerAdLoaded?.Invoke(false, "横幅广告结果解析失败");
                    callback?.Invoke(false, "横幅广告结果解析失败");
                }
            });
        }

        /// <summary>
        /// 显示横幅广告
        /// </summary>
        public void ShowBannerAd(Action<bool, string> callback = null)
        {
            if (!_isBannerLoaded || _bannerAd == null)
            {
                Debug.LogWarning("[WeChatAd] 横幅广告未加载");
                callback?.Invoke(false, "横幅广告未加载");
                return;
            }

            Debug.Log("[WeChatAd] 显示横幅广告...");

            // 调用微信横幅广告显示API
            WeChatAPI.CallWeChatAPI("showBannerAd", $"{{\"adId\":\"{_bannerAd.adId}\"}}", (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                Debug.Log($"[WeChatAd] 横幅广告显示：{(success ? "成功" : "失败")}");
                OnAdShown?.Invoke(success, success ? "横幅广告显示成功" : "横幅广告显示失败");
                callback?.Invoke(success, success ? "横幅广告显示成功" : "横幅广告显示失败");
            });
        }

        /// <summary>
        /// 隐藏横幅广告
        /// </summary>
        public void HideBannerAd(Action<bool, string> callback = null)
        {
            if (_bannerAd == null)
            {
                Debug.LogWarning("[WeChatAd] 横幅广告未创建");
                callback?.Invoke(false, "横幅广告未创建");
                return;
            }

            Debug.Log("[WeChatAd] 隐藏横幅广告...");

            // 调用微信横幅广告隐藏API
            WeChatAPI.CallWeChatAPI("hideBannerAd", $"{{\"adId\":\"{_bannerAd.adId}\"}}", (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                Debug.Log($"[WeChatAd] 横幅广告隐藏：{(success ? "成功" : "失败")}");
                callback?.Invoke(success, success ? "横幅广告隐藏成功" : "横幅广告隐藏失败");
            });
        }

        /// <summary>
        /// 加载激励视频广告
        /// </summary>
        public void LoadRewardedVideoAd(Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatAd] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            Debug.Log("[WeChatAd] 加载激励视频广告...");

            // 构建激励视频广告参数
            string adParams = BuildRewardedVideoAdParams();

            // 调用微信激励视频广告API
            WeChatAPI.CallWeChatAPI("createRewardedVideoAd", adParams, (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning("[WeChatAd] 激励视频广告加载失败");
                    OnRewardedVideoAdLoaded?.Invoke(false, "激励视频广告加载失败");
                    callback?.Invoke(false, "激励视频广告加载失败");
                    return;
                }

                try
                {
                    var adResult = JsonUtility.FromJson<WeChatAdResult>(result);
                    
                    if (adResult.success)
                    {
                        _isRewardedVideoLoaded = true;
                        _rewardedVideoAd = adResult.rewardedVideoAd;
                        
                        Debug.Log("[WeChatAd] 激励视频广告加载成功");
                        OnRewardedVideoAdLoaded?.Invoke(true, "激励视频广告加载成功");
                        callback?.Invoke(true, "激励视频广告加载成功");
                    }
                    else
                    {
                        Debug.LogWarning($"[WeChatAd] 激励视频广告加载失败：{adResult.error}");
                        OnRewardedVideoAdLoaded?.Invoke(false, adResult.error);
                        callback?.Invoke(false, adResult.error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatAd] 激励视频广告结果解析失败：{ex.Message}");
                    OnRewardedVideoAdLoaded?.Invoke(false, "激励视频广告结果解析失败");
                    callback?.Invoke(false, "激励视频广告结果解析失败");
                }
            });
        }

        /// <summary>
        /// 显示激励视频广告
        /// </summary>
        public void ShowRewardedVideoAd(Action<bool, int> callback = null)
        {
            if (!_isRewardedVideoLoaded || _rewardedVideoAd == null)
            {
                Debug.LogWarning("[WeChatAd] 激励视频广告未加载");
                callback?.Invoke(false, 0);
                return;
            }

            Debug.Log("[WeChatAd] 显示激励视频广告...");

            // 调用微信激励视频广告显示API
            WeChatAPI.CallWeChatAPI("showRewardedVideoAd", $"{{\"adId\":\"{_rewardedVideoAd.adId}\"}}", (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning("[WeChatAd] 激励视频广告显示失败");
                    OnAdShown?.Invoke(false, "激励视频广告显示失败");
                    callback?.Invoke(false, 0);
                    return;
                }

                try
                {
                    var adResult = JsonUtility.FromJson<WeChatRewardedVideoResult>(result);
                    
                    if (adResult.success)
                    {
                        int rewardAmount = adResult.rewardAmount;
                        
                        Debug.Log($"[WeChatAd] 激励视频广告观看完成，奖励：{rewardAmount}");
                        OnRewardedVideoCompleted?.Invoke(true, rewardAmount);
                        OnAdShown?.Invoke(true, "激励视频广告显示成功");
                        callback?.Invoke(true, rewardAmount);
                    }
                    else
                    {
                        Debug.LogWarning($"[WeChatAd] 激励视频广告显示失败：{adResult.error}");
                        OnAdShown?.Invoke(false, adResult.error);
                        callback?.Invoke(false, 0);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatAd] 激励视频广告结果解析失败：{ex.Message}");
                    OnAdShown?.Invoke(false, "激励视频广告结果解析失败");
                    callback?.Invoke(false, 0);
                }
            });
        }

        /// <summary>
        /// 加载插屏广告
        /// </summary>
        public void LoadInterstitialAd(Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatAd] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            Debug.Log("[WeChatAd] 加载插屏广告...");

            // 构建插屏广告参数
            string adParams = BuildInterstitialAdParams();

            // 调用微信插屏广告API
            WeChatAPI.CallWeChatAPI("createInterstitialAd", adParams, (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning("[WeChatAd] 插屏广告加载失败");
                    OnInterstitialAdLoaded?.Invoke(false, "插屏广告加载失败");
                    callback?.Invoke(false, "插屏广告加载失败");
                    return;
                }

                try
                {
                    var adResult = JsonUtility.FromJson<WeChatAdResult>(result);
                    
                    if (adResult.success)
                    {
                        _isInterstitialLoaded = true;
                        _interstitialAd = adResult.interstitialAd;
                        
                        Debug.Log("[WeChatAd] 插屏广告加载成功");
                        OnInterstitialAdLoaded?.Invoke(true, "插屏广告加载成功");
                        callback?.Invoke(true, "插屏广告加载成功");
                    }
                    else
                    {
                        Debug.LogWarning($"[WeChatAd] 插屏广告加载失败：{adResult.error}");
                        OnInterstitialAdLoaded?.Invoke(false, adResult.error);
                        callback?.Invoke(false, adResult.error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatAd] 插屏广告结果解析失败：{ex.Message}");
                    OnInterstitialAdLoaded?.Invoke(false, "插屏广告结果解析失败");
                    callback?.Invoke(false, "插屏广告结果解析失败");
                }
            });
        }

        /// <summary>
        /// 显示插屏广告
        /// </summary>
        public void ShowInterstitialAd(Action<bool, string> callback = null)
        {
            if (!_isInterstitialLoaded || _interstitialAd == null)
            {
                Debug.LogWarning("[WeChatAd] 插屏广告未加载");
                callback?.Invoke(false, "插屏广告未加载");
                return;
            }

            Debug.Log("[WeChatAd] 显示插屏广告...");

            // 调用微信插屏广告显示API
            WeChatAPI.CallWeChatAPI("showInterstitialAd", $"{{\"adId\":\"{_interstitialAd.adId}\"}}", (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                Debug.Log($"[WeChatAd] 插屏广告显示：{(success ? "成功" : "失败")}");
                OnAdShown?.Invoke(success, success ? "插屏广告显示成功" : "插屏广告显示失败");
                callback?.Invoke(success, success ? "插屏广告显示成功" : "插屏广告显示失败");
            });
        }

        /// <summary>
        /// 构建横幅广告参数
        /// </summary>
        private string BuildBannerAdParams()
        {
            var bannerData = new WeChatBannerAdData
            {
                adId = BANNER_AD_ID,
                style = new WeChatAdStyle
                {
                    left = 0,
                    top = 0,
                    width = 300,
                    height = 100
                }
            };

            return JsonUtility.ToJson(bannerData);
        }

        /// <summary>
        /// 构建激励视频广告参数
        /// </summary>
        private string BuildRewardedVideoAdParams()
        {
            var rewardedVideoData = new WeChatRewardedVideoAdData
            {
                adId = REWARDED_VIDEO_AD_ID
            };

            return JsonUtility.ToJson(rewardedVideoData);
        }

        /// <summary>
        /// 构建插屏广告参数
        /// </summary>
        private string BuildInterstitialAdParams()
        {
            var interstitialData = new WeChatInterstitialAdData
            {
                adId = INTERSTITIAL_AD_ID
            };

            return JsonUtility.ToJson(interstitialData);
        }

        /// <summary>
        /// 检查横幅广告是否已加载
        /// </summary>
        public bool IsBannerLoaded()
        {
            return _isBannerLoaded;
        }

        /// <summary>
        /// 检查激励视频广告是否已加载
        /// </summary>
        public bool IsRewardedVideoLoaded()
        {
            return _isRewardedVideoLoaded;
        }

        /// <summary>
        /// 检查插屏广告是否已加载
        /// </summary>
        public bool IsInterstitialLoaded()
        {
            return _isInterstitialLoaded;
        }

        /// <summary>
        /// 获取广告统计信息
        /// </summary>
        public string GetAdInfo()
        {
            return $"横幅广告：{(_isBannerLoaded ? "已加载" : "未加载")}, " +
                   $"激励视频：{(_isRewardedVideoLoaded ? "已加载" : "未加载")}, " +
                   $"插屏广告：{(_isInterstitialLoaded ? "已加载" : "未加载")}, " +
                   $"微信环境：{(WeChatAPI.IsAvailable() ? "可用" : "不可用")}";
        }

        /// <summary>
        /// 清除广告数据
        /// </summary>
        public void ClearAdData()
        {
            _isBannerLoaded = false;
            _isRewardedVideoLoaded = false;
            _isInterstitialLoaded = false;
            _bannerAd = null;
            _rewardedVideoAd = null;
            _interstitialAd = null;
            Debug.Log("[WeChatAd] 广告数据已清除");
        }
    }

    /// <summary>
    /// 微信广告结果
    /// </summary>
    [System.Serializable]
    public class WeChatAdResult
    {
        public bool success;
        public WeChatBannerAd bannerAd;
        public WeChatRewardedVideoAd rewardedVideoAd;
        public WeChatInterstitialAd interstitialAd;
        public string error;
    }

    /// <summary>
    /// 微信横幅广告数据
    /// </summary>
    [System.Serializable]
    public class WeChatBannerAdData
    {
        public string adId;
        public WeChatAdStyle style;
    }

    /// <summary>
    /// 微信激励视频广告数据
    /// </summary>
    [System.Serializable]
    public class WeChatRewardedVideoAdData
    {
        public string adId;
    }

    /// <summary>
    /// 微信插屏广告数据
    /// </summary>
    [System.Serializable]
    public class WeChatInterstitialAdData
    {
        public string adId;
    }

    /// <summary>
    /// 微信广告样式
    /// </summary>
    [System.Serializable]
    public class WeChatAdStyle
    {
        public int left;
        public int top;
        public int width;
        public int height;
    }

    /// <summary>
    /// 微信横幅广告
    /// </summary>
    [System.Serializable]
    public class WeChatBannerAd
    {
        public string adId;
        public WeChatAdStyle style;
    }

    /// <summary>
    /// 微信激励视频广告
    /// </summary>
    [System.Serializable]
    public class WeChatRewardedVideoAd
    {
        public string adId;
    }

    /// <summary>
    /// 微信插屏广告
    /// </summary>
    [System.Serializable]
    public class WeChatInterstitialAd
    {
        public string adId;
    }

    /// <summary>
    /// 微信激励视频结果
    /// </summary>
    [System.Serializable]
    public class WeChatRewardedVideoResult
    {
        public bool success;
        public int rewardAmount;
        public string error;
    }
}