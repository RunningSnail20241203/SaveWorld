using System;
using UnityEngine;
using WeChatWASM;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信广告系统 - 激励视频、Banner、插屏广告
    /// 基于 WX.CreateRewardedVideoAd / WX.CreateBannerAd / WX.CreateInterstitialAd
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

        private WXRewardedVideoAd _rewardedVideoAd;
        private WXBannerAd _bannerAd;
        private WXInterstitialAd _interstitialAd;

        private string _rewardedVideoAdUnitId = "";
        private string _bannerAdUnitId = "";
        private string _interstitialAdUnitId = "";

        public event Action<bool, int> OnRewardedVideoCompleted;
        public event Action<bool, string> OnAdError;

        /// <summary>
        /// 从 WeChatConfigManager 加载广告单元 ID
        /// </summary>
        public void LoadConfig()
        {
            _rewardedVideoAdUnitId = WeChatConfigManager.RewardedVideoAdUnitId;
            _bannerAdUnitId = WeChatConfigManager.BannerAdUnitId;
            _interstitialAdUnitId = WeChatConfigManager.InterstitialAdUnitId;
            Debug.Log($"[WeChatAd] 配置加载完成: {GetAdInfo()}");
        }

        /// <summary>
        /// 设置广告单元ID（需要在微信后台获取后配置）
        /// </summary>
        public void SetAdUnitIds(string rewardedVideoId, string bannerId = null, string interstitialId = null)
        {
            _rewardedVideoAdUnitId = rewardedVideoId;
            _bannerAdUnitId = bannerId ?? "";
            _interstitialAdUnitId = interstitialId ?? "";
            Debug.Log("[WeChatAd] 广告单元ID已设置");
        }

        /// <summary>
        /// 显示激励视频广告
        /// </summary>
        public void ShowRewardedVideoAd(Action<bool, int> callback = null)
        {
            if (string.IsNullOrEmpty(_rewardedVideoAdUnitId))
            {
                Debug.LogWarning("[WeChatAd] 激励视频广告ID未设置");
                callback?.Invoke(false, 0);
                return;
            }

            Debug.Log("[WeChatAd] 创建激励视频广告...");

            _rewardedVideoAd = WX.CreateRewardedVideoAd(
                new WXCreateRewardedVideoAdParam
                {
                    adUnitId = _rewardedVideoAdUnitId
                }
            );

            _rewardedVideoAd.OnLoad((WXADLoadResponse loadRes) =>
            {
                Debug.Log("[WeChatAd] 激励视频广告加载成功");
                _rewardedVideoAd.Show((res) =>
                {
                    if (res != null)
                    {
                        Debug.Log("[WeChatAd] 激励视频广告观看完成，发放奖励");
                        OnRewardedVideoCompleted?.Invoke(true, 1);
                        callback?.Invoke(true, 1);
                    }
                    else
                    {
                        OnAdError?.Invoke(false, "用户未看完广告");
                        callback?.Invoke(false, 0);
                    }
                });
            });

            _rewardedVideoAd.OnError((res) =>
            {
                Debug.LogError($"[WeChatAd] 激励视频广告错误: {res.errMsg}");
                OnAdError?.Invoke(false, res.errMsg);
                callback?.Invoke(false, 0);
            });

            _rewardedVideoAd.OnClose((res) =>
            {
                if (res != null && res.isEnded)
                {
                    Debug.Log("[WeChatAd] 激励视频广告正常结束");
                }
            });
        }

        /// <summary>
        /// 显示底部 Banner 广告
        /// </summary>
        public void ShowBannerAd()
        {
            if (string.IsNullOrEmpty(_bannerAdUnitId))
            {
                Debug.LogWarning("[WeChatAd] Banner广告ID未设置");
                return;
            }

            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            _bannerAd = WX.CreateBannerAd(new WXCreateBannerAdParam
            {
                adUnitId = _bannerAdUnitId,
                adIntervals = 30,
                style = new Style
                {
                    left = 0,
                    top = screenHeight - 100,
                    width = screenWidth,
                    height = 100
                }
            });

            _bannerAd.OnLoad((WXADLoadResponse loadRes) =>
            {
                Debug.Log("[WeChatAd] Banner广告加载成功");
                _bannerAd.Show();
            });

            _bannerAd.OnError((res) =>
            {
                Debug.LogError($"[WeChatAd] Banner广告错误: {res.errMsg}");
                OnAdError?.Invoke(false, res.errMsg);
            });

            _bannerAd.OnResize((res) =>
            {
                _bannerAd.style.top = (int)(Screen.height - res.height);
            });
        }

        /// <summary>
        /// 隐藏 Banner 广告
        /// </summary>
        public void HideBannerAd()
        {
            _bannerAd?.Hide();
        }

        /// <summary>
        /// 显示插屏广告
        /// </summary>
        public void ShowInterstitialAd(Action<bool> callback = null)
        {
            if (string.IsNullOrEmpty(_interstitialAdUnitId))
            {
                Debug.LogWarning("[WeChatAd] 插屏广告ID未设置");
                callback?.Invoke(false);
                return;
            }

            _interstitialAd = WX.CreateInterstitialAd(new WXCreateInterstitialAdParam
            {
                adUnitId = _interstitialAdUnitId
            });

            _interstitialAd.OnLoad((WXADLoadResponse loadRes) =>
            {
                _interstitialAd.Show();
                callback?.Invoke(true);
            });

            _interstitialAd.OnError((res) =>
            {
                Debug.LogError($"[WeChatAd] 插屏广告错误: {res.errMsg}");
                OnAdError?.Invoke(false, res.errMsg);
                callback?.Invoke(false);
            });
        }

        public string GetAdInfo()
        {
            return $"激励视频ID: {(_rewardedVideoAdUnitId.Length > 0 ? "已设置" : "未设置")}, " +
                   $"Banner ID: {(_bannerAdUnitId.Length > 0 ? "已设置" : "未设置")}, " +
                   $"插屏ID: {(_interstitialAdUnitId.Length > 0 ? "已设置" : "未设置")}";
        }
    }
}
