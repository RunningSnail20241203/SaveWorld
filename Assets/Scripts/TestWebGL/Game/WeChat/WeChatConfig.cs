using UnityEngine;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信小游戏全局配置 (ScriptableObject)
    /// 在 Unity Editor 中通过 Assets/Create/WeChat/Config 创建
    /// 存放于 Resources 目录下，运行时通过 Resources.Load 加载
    ///
    /// 所有广告ID、支付offerId等微信后台配置集中管理于此
    /// </summary>
    [CreateAssetMenu(fileName = "WeChatConfig", menuName = "WeChat/Config", order = 1)]
    public class WeChatConfig : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("微信小游戏 AppID (MP后台获取)")]
        public string appId = "wxb2624b9ca163b59f";

        [Tooltip("游戏分享默认标题")]
        public string shareTitle = "末世生存合成";

        [Tooltip("分享图片 CDN 地址")]
        public string shareImageUrl = "";

        [Header("广告配置")]
        [Tooltip("激励视频广告单元 ID (微信MP后台 → 流量主 → 广告管理)")]
        public string rewardedVideoAdUnitId = "";

        [Tooltip("Banner 广告单元 ID")]
        public string bannerAdUnitId = "";

        [Tooltip("插屏广告单元 ID")]
        public string interstitialAdUnitId = "";

        [Header("支付配置")]
        [Tooltip("米大师 offerId (微信MP后台 → 支付 → 米大师)")]
        public string midasOfferId = "";

        [Tooltip("米大师 zoneId (分区ID，单区游戏填 '1')")]
        public string midasZoneId = "1";

        [Tooltip("支付环境: 0=正式, 1=沙箱")]
        public int midasEnv = 1;

        [Header("云存储")]
        [Tooltip("排行榜云存储 key")]
        public string rankingKey = "game_score";

        [Tooltip("存档云存储 key 前缀")]
        public string saveDataKeyPrefix = "save_data_";

        // ==================== 运行时验证 ====================

        public bool IsAdConfigured()
        {
            return !string.IsNullOrEmpty(rewardedVideoAdUnitId);
        }

        public bool IsPayConfigured()
        {
            return !string.IsNullOrEmpty(midasOfferId);
        }

        public bool IsShareImageConfigured()
        {
            return !string.IsNullOrEmpty(shareImageUrl);
        }

        public string GetConfigSummary()
        {
            return $"AppID: {appId}\n" +
                   $"激励视频广告: {(string.IsNullOrEmpty(rewardedVideoAdUnitId) ? "❌ 未配置" : "✅ 已配置")}\n" +
                   $"Banner广告: {(string.IsNullOrEmpty(bannerAdUnitId) ? "❌ 未配置" : "✅ 已配置")}\n" +
                   $"插屏广告: {(string.IsNullOrEmpty(interstitialAdUnitId) ? "❌ 未配置" : "✅ 已配置")}\n" +
                   $"米大师支付: {(string.IsNullOrEmpty(midasOfferId) ? "❌ 未配置" : $"✅ {midasOfferId}")}\n" +
                   $"分享图片: {(string.IsNullOrEmpty(shareImageUrl) ? "❌ 未配置" : "✅ 已配置")}";
        }
    }
}
