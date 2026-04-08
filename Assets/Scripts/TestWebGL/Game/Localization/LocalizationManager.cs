using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TestWebGL.Game.Items;

namespace TestWebGL.Game
{
    /// <summary>
    /// 本地化管理器
    /// 统一管理游戏所有文本资源的本地化访问
    /// </summary>
    public static class LocalizationManager
    {
        public const string TABLE_ITEMS = "Items";
        public const string TABLE_MESSAGES = "Messages";
        public const string TABLE_UI = "UI";
        public const string TABLE_ORDERS = "Orders";
        public const string TABLE_ERRORS = "Errors";

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        public static string GetString(string tableName, string key)
        {
            var table = LocalizationSettings.StringDatabase.GetTable(tableName);
            var entry = table.GetEntry(key);
            return entry != null ? entry.GetLocalizedString() : $"#{key}";
        }

        /// <summary>
        /// 获取物品名称
        /// </summary>
        public static string GetItemName(ItemType itemType)
        {
            return GetString(TABLE_ITEMS, itemType.ToString());
        }

        /// <summary>
        /// 获取消息文本
        /// </summary>
        public static string GetMessage(string key)
        {
            return GetString(TABLE_MESSAGES, key);
        }

        /// <summary>
        /// 获取UI文本
        /// </summary>
        public static string GetUIText(string key)
        {
            return GetString(TABLE_UI, key);
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        public static string GetError(string key)
        {
            return GetString(TABLE_ERRORS, key);
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        public static void SetLanguage(Locale locale)
        {
            LocalizationSettings.SelectedLocale = locale;
        }

        /// <summary>
        /// 获取当前语言
        /// </summary>
        public static Locale GetCurrentLanguage()
        {
            return LocalizationSettings.SelectedLocale;
        }
    }
}