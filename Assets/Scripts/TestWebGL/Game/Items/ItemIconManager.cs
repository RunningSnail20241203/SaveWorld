using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld.Game.Items
{
    /// <summary>
    /// 物品图标管理器
    /// 负责管理所有物品图标的加载和缓存
    /// 支持同步预加载（兼容旧代码）和分帧异步预加载（推荐）
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

        // 图标缓存
        private Dictionary<string, Sprite> _iconCache = new Dictionary<string, Sprite>();
        
        // 默认图标
        private Sprite _defaultIcon;
        
        // 初始化状态
        private bool _isInitialized = false;

        // 异步预加载每帧最大加载数量
        public int IconsPerFrame = 8;

        /// <summary>
        /// 初始化图标管理器
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[ItemIconManager] 初始化物品图标管理器...");

            // 加载默认图标
            _defaultIcon = Resources.Load<Sprite>("Icons/DefaultItem");
            if (_defaultIcon == null)
            {
                Debug.LogWarning("[ItemIconManager] 默认图标未找到");
            }

            _isInitialized = true;
            Debug.Log("[ItemIconManager] 物品图标管理器初始化完成");
        }

        /// <summary>
        /// 获取物品图标
        /// </summary>
        public Sprite GetItemIcon(ItemType itemType)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            string iconName = GetIconName(itemType);

            // 检查缓存
            if (_iconCache.TryGetValue(iconName, out Sprite cachedIcon))
            {
                return cachedIcon;
            }

            // 加载图标
            Sprite icon = LoadItemIcon(iconName);
            if (icon != null)
            {
                _iconCache[iconName] = icon;
                return icon;
            }

            // 返回默认图标
            Debug.LogWarning($"[ItemIconManager] 图标未找到：{iconName}，使用默认图标");
            return _defaultIcon;
        }

        /// <summary>
        /// 获取图标名称
        /// </summary>
        private string GetIconName(ItemType itemType)
        {
            string typeName = itemType.ToString();
            
            // 处理特殊类型
            if (typeName.Contains("_L"))
            {
                return typeName;
            }
            
            // 处理跨线合成物品
            if (typeName.StartsWith("Cross_"))
            {
                return typeName;
            }
            
            return typeName;
        }

        /// <summary>
        /// 加载物品图标
        /// </summary>
        private Sprite LoadItemIcon(string iconName)
        {
            try
            {
                // 尝试从Resources加载
                Sprite icon = Resources.Load<Sprite>($"Icons/Items/{iconName}");
                if (icon != null)
                {
                    return icon;
                }

                // 尝试从Resources根目录加载
                icon = Resources.Load<Sprite>($"Icons/{iconName}");
                if (icon != null)
                {
                    return icon;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemIconManager] 加载图标失败：{iconName} - {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 预加载所有图标（同步，兼容旧调用）
        /// </summary>
        public void PreloadAllIcons()
        {
            Debug.Log("[ItemIconManager] 预加载所有物品图标（同步模式）...");

            // 预加载L1图标
            PreloadL1Icons();

            // 预加载L2-L10图标（如果存在）
            PreloadLevelIcons();

            Debug.Log($"[ItemIconManager] 图标预加载完成，缓存数量：{_iconCache.Count}");
        }

        /// <summary>
        /// 异步分帧预加载所有图标（推荐用于启动流程）
        /// 每帧加载 IconsPerFrame 个图标，避免主线程阻塞
        /// </summary>
        /// <param name="onProgress">进度回调 (当前数, 总数, 进度0~1)</param>
        /// <param name="onComplete">完成回调</param>
        public IEnumerator PreloadAllIconsCoroutine(
            Action<int, int, float> onProgress = null,
            Action onComplete = null)
        {
            Debug.Log($"[ItemIconManager] 开始异步预加载物品图标，每帧{IconsPerFrame}个...");

            var allIconNames = GetAllIconNames();
            int total = allIconNames.Count;
            int loaded = 0;

            for (int i = 0; i < allIconNames.Count; i++)
            {
                string iconName = allIconNames[i];
                if (!_iconCache.ContainsKey(iconName))
                {
                    Sprite icon = Resources.Load<Sprite>($"Icons/Items/{iconName}");
                    if (icon != null)
                        _iconCache[iconName] = icon;
                }
                loaded++;

                float progress = (float)loaded / total;
                onProgress?.Invoke(loaded, total, progress);

                // 分帧：每 IconsPerFrame 个图标让出一帧
                if ((i + 1) % IconsPerFrame == 0 && i < allIconNames.Count - 1)
                    yield return null;
            }

            Debug.Log($"[ItemIconManager] 图标异步预加载完成，缓存数量：{_iconCache.Count}");
            onComplete?.Invoke();
        }

        /// <summary>
        /// 获取所有需要预加载的图标名称列表
        /// </summary>
        private List<string> GetAllIconNames()
        {
            var list = new List<string>();
            
            string[] l1Icons = { "Water_L1", "Food_L1", "Tool_L1", "Home_L1",
                "Medical_L1", "Energy_L1", "Knowledge_L1", "Hope_L1", "Explore_L1" };
            foreach (var n in l1Icons) list.Add(n);

            string[] itemTypes = { "Water", "Food", "Tool", "Home", "Medical", "Energy", "Knowledge", "Hope", "Explore" };
            for (int level = 2; level <= 10; level++)
            {
                foreach (string itemType in itemTypes)
                    list.Add($"{itemType}_L{level}");
            }

            return list;
        }

        /// <summary>
        /// 预加载L1图标
        /// </summary>
        private void PreloadL1Icons()
        {
            string[] l1Icons = {
                "Water_L1", "Food_L1", "Tool_L1", "Home_L1",
                "Medical_L1", "Energy_L1", "Knowledge_L1", "Hope_L1", "Explore_L1"
            };

            foreach (string iconName in l1Icons)
            {
                Sprite icon = Resources.Load<Sprite>($"Icons/Items/{iconName}");
                if (icon != null)
                {
                    _iconCache[iconName] = icon;
                }
            }
        }

        /// <summary>
        /// 预加载等级图标
        /// </summary>
        private void PreloadLevelIcons()
        {
            string[] itemTypes = { "Water", "Food", "Tool", "Home", "Medical", "Energy", "Knowledge", "Hope", "Explore" };

            foreach (string itemType in itemTypes)
            {
                for (int level = 2; level <= 10; level++)
                {
                    string iconName = $"{itemType}_L{level}";
                    Sprite icon = Resources.Load<Sprite>($"Icons/Items/{iconName}");
                    if (icon != null)
                    {
                        _iconCache[iconName] = icon;
                    }
                }
            }
        }

        /// <summary>
        /// 清除图标缓存
        /// </summary>
        public void ClearCache()
        {
            _iconCache.Clear();
            Debug.Log("[ItemIconManager] 图标缓存已清除");
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public string GetCacheInfo()
        {
            return $"图标缓存：{_iconCache.Count}个";
        }

        /// <summary>
        /// 检查图标是否存在
        /// </summary>
        public bool HasIcon(ItemType itemType)
        {
            string iconName = GetIconName(itemType);
            return _iconCache.ContainsKey(iconName) || Resources.Load<Sprite>($"Icons/Items/{iconName}") != null;
        }

        /// <summary>
        /// 获取缺失的图标列表
        /// </summary>
        public List<string> GetMissingIcons()
        {
            List<string> missingIcons = new List<string>();

            // 检查所有物品类型
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType == ItemType.None) continue;

                string iconName = GetIconName(itemType);
                if (!_iconCache.ContainsKey(iconName) && Resources.Load<Sprite>($"Icons/Items/{iconName}") == null)
                {
                    missingIcons.Add(iconName);
                }
            }

            return missingIcons;
        }
    }
}