using UnityEngine;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信配置管理器
    /// 优先从 Resources/WeChatConfig 加载 ScriptableObject，
    /// 如果未创建 .asset 文件则使用内置默认值。
    /// </summary>
    public static class WeChatConfigManager
    {
        private static WeChatConfig _config;
        private static bool _initialized = false;

        /// <summary>
        /// 获取配置实例（懒加载，优先从 Resources 读取）
        /// </summary>
        public static WeChatConfig Config
        {
            get
            {
                if (!_initialized)
                {
                    Initialize();
                }
                return _config;
            }
        }

        private static void Initialize()
        {
            _initialized = true;

            // 尝试从 Resources 加载 ScriptableObject
            _config = Resources.Load<WeChatConfig>("WeChatConfig");

            if (_config == null)
            {
                // 回退：在代码中创建默认配置实例
                Debug.LogWarning("[WeChatConfig] 未找到 Resources/WeChatConfig.asset，" +
                               "使用内置默认值。请在 Unity Editor 中通过 " +
                               "Assets/Create/WeChat/Config 创建配置文件并放入 Resources 目录。");
                _config = ScriptableObject.CreateInstance<WeChatConfig>();
            }
            else
            {
                Debug.Log($"[WeChatConfig] 配置加载成功\n{_config.GetConfigSummary()}");
            }
        }

        // ==================== 便捷属性 ====================

        public static string AppId => Config.appId;
        public static string ShareTitle => Config.shareTitle;
        public static string ShareImageUrl => Config.shareImageUrl;
        public static string RewardedVideoAdUnitId => Config.rewardedVideoAdUnitId;
        public static string BannerAdUnitId => Config.bannerAdUnitId;
        public static string InterstitialAdUnitId => Config.interstitialAdUnitId;
        public static string MidasOfferId => Config.midasOfferId;
        public static string MidasZoneId => Config.midasZoneId;
        public static int MidasEnv => Config.midasEnv;
        public static string RankingKey => Config.rankingKey;
        public static string SaveDataKeyPrefix => Config.saveDataKeyPrefix;

        /// <summary>
        /// 重新加载配置（用于热更新场景）
        /// </summary>
        public static void Reload()
        {
            _initialized = false;
            _config = null;
        }

        /// <summary>
        /// 获取配置摘要字符串（调试用）
        /// </summary>
        public static string GetSummary()
        {
            return Config.GetConfigSummary();
        }
    }
}
