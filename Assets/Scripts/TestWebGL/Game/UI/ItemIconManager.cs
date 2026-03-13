using UnityEngine;
using TestWebGL.Game.Items;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// 物品图标管理器
    /// 负责管理所有物品的图标资源
    /// </summary>
    public class ItemIconManager : MonoBehaviour
    {
        private static ItemIconManager s_instance;

        public static ItemIconManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("ItemIconManager");
                    s_instance = go.AddComponent<ItemIconManager>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 图标资源路径配置
        private const string ICON_PATH_BASE = "Icons/Items/";

        /// <summary>
        /// 获取物品图标
        /// </summary>
        public Sprite GetItemIcon(ItemType itemType)
        {
            string iconPath = GetIconPath(itemType);
            Sprite icon = Resources.Load<Sprite>(iconPath);

            if (icon == null)
            {
                Debug.LogWarning($"[ItemIconManager] 物品图标未找到: {iconPath}，使用默认图标");
                // 返回默认图标
                return GetDefaultIcon();
            }

            return icon;
        }

        /// <summary>
        /// 获取物品图标路径
        /// </summary>
        private string GetIconPath(ItemType itemType)
        {
            // 解析物品类型和等级
            int baseTypeIndex = (int)itemType / 10; // 每10个等级一个基础类型
            int level = ((int)itemType % 10) + 1; // 等级从1开始

            string baseTypeName = GetBaseTypeName(baseTypeIndex);
            return $"{ICON_PATH_BASE}{baseTypeName}_L{level}";
        }

        /// <summary>
        /// 获取基础类型名称
        /// </summary>
        private string GetBaseTypeName(int baseTypeIndex)
        {
            return baseTypeIndex switch
            {
                0 => "Water",      // 净水
                1 => "Food",       // 食物
                2 => "Tool",       // 工具
                3 => "Home",       // 居所
                4 => "Medical",    // 医疗
                5 => "Energy",     // 能源
                6 => "Knowledge",  // 知识
                7 => "Hope",       // 希望
                8 => "Explore",    // 探索
                _ => "Unknown"
            };
        }

        /// <summary>
        /// 获取默认图标
        /// </summary>
        private Sprite GetDefaultIcon()
        {
            // 尝试加载默认图标
            Sprite defaultIcon = Resources.Load<Sprite>("Icons/DefaultItem");
            if (defaultIcon == null)
            {
                Debug.LogError("[ItemIconManager] 默认物品图标也未找到!");
            }
            return defaultIcon;
        }

        /// <summary>
        /// 预加载所有图标（可选，用于优化）
        /// </summary>
        public void PreloadAllIcons()
        {
            Debug.Log("[ItemIconManager] 开始预加载物品图标...");

            int loadedCount = 0;
            foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
            {
                if (itemType != ItemType.None)
                {
                    Sprite icon = GetItemIcon(itemType);
                    if (icon != null)
                    {
                        loadedCount++;
                    }
                }
            }

            Debug.Log($"[ItemIconManager] 预加载完成，共加载 {loadedCount} 个图标");
        }
    }
}