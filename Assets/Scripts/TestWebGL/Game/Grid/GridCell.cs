

using SaveWorld.Game.Items;

namespace SaveWorld.Game.Grid
{
    /// <summary>
    /// 单个格子的数据模型
    /// 表示9×7网格中的一个格子状态
    /// 
    /// 【重构 v2.0】
    /// 本类只负责持有物品数据，不再包含锁定状态
    /// 锁定是外部正交机制，由 GridManager 统一管理
    /// </summary>
    [System.Serializable]
    public class GridCell
    {
        // 格子位置
        public int row;
        public int column;

        // 格子中的物品
        [UnityEngine.SerializeField]
        private ItemType _currentItemType;

        [UnityEngine.SerializeField]
        private int _itemCount;  // 堆叠数量，1-99

        // 属性 - 只读访问
        public ItemType CurrentItemType => _currentItemType;
        public int ItemCount => _itemCount;
        public bool HasItem => _currentItemType != ItemType.None && _itemCount > 0;

        /// <summary>
        /// 创建空格子
        /// </summary>
        public static GridCell CreateEmpty(int row, int column)
        {
            return new GridCell
            {
                row = row,
                column = column,
                _currentItemType = ItemType.None,
                _itemCount = 0
            };
        }

        /// <summary>
        /// 创建有物品的格子
        /// </summary>
        public static GridCell CreateFilled(int row, int column, ItemType itemType, int count = 1)
        {
            return new GridCell
            {
                row = row,
                column = column,
                _currentItemType = itemType,
                _itemCount = count > 99 ? 99 : count
            };
        }

        /// <summary>
        /// 放置物品到该格子（格子必须为空）
        /// </summary>
        public bool TryPlaceItem(ItemType itemType, int count = 1)
        {
            if (_currentItemType != ItemType.None)
                return false;

            _currentItemType = itemType;
            _itemCount = count > 99 ? 99 : count;
            return true;
        }

        /// <summary>
        /// 堆叠相同的物品到该格子
        /// </summary>
        public bool TryStackItem(ItemType itemType, int addCount = 1)
        {
            if (_currentItemType != itemType)
                return false;

            int newCount = _itemCount + addCount;
            _itemCount = newCount > 99 ? 99 : newCount;
            return true;
        }

        /// <summary>
        /// 移除物品
        /// </summary>
        public bool TryRemoveItem(int removeCount)
        {
            if (_currentItemType == ItemType.None)
                return false;

            _itemCount -= removeCount;
            if (_itemCount <= 0)
            {
                _currentItemType = ItemType.None;
                _itemCount = 0;
            }
            return true;
        }

        /// <summary>
        /// 清空格子中的物品
        /// </summary>
        public void ClearItem()
        {
            _currentItemType = ItemType.None;
            _itemCount = 0;
        }

        /// <summary>
        /// 获取该格子的UI显示名称
        /// </summary>
        public string GetDisplayName()
        {
            if (_currentItemType != ItemType.None)
            {
                return ItemConfig.GetItemName(_currentItemType);
            }

            return "空";
        }
    }
}