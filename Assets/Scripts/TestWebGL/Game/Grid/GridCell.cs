using TestWebGL.Game.Items;

namespace TestWebGL.Game.Grid
{
    /// <summary>
    /// 单个格子的数据模型
    /// 表示9×7网格中的一个格子状态
    /// </summary>
    [System.Serializable]
    public class GridCell
    {
        // 格子位置
        public int row;
        public int column;

        // 锁定状态
        [UnityEngine.SerializeField]
        private bool _isLocked;

        // 当格子被锁定时，显示要解锁的物品信息
        [UnityEngine.SerializeField]
        private ItemType _lockedItemType;

        [UnityEngine.SerializeField]
        private int _lockedItemLevel;

        // 已解锁格子中的物品
        [UnityEngine.SerializeField]
        private ItemType _currentItemType;

        [UnityEngine.SerializeField]
        private int _itemCount;  // 堆叠数量，1-99

        // 属性 - 只读访问
        public bool IsLocked => _isLocked;
        public ItemType LockedItemType => _lockedItemType;
        public int LockedItemLevel => _lockedItemLevel;
        public ItemType CurrentItemType => _currentItemType;
        public int ItemCount => _itemCount;
        public bool HasItem => _currentItemType != ItemType.None && _itemCount > 0;

        /// <summary>
        /// 创建锁定格子
        /// </summary>
        public static GridCell CreateLockedCell(int row, int column, ItemType lockedItemType, int level)
        {
            return new GridCell
            {
                row = row,
                column = column,
                _isLocked = true,
                _lockedItemType = lockedItemType,
                _lockedItemLevel = level,
                _currentItemType = ItemType.None,
                _itemCount = 0
            };
        }

        /// <summary>
        /// 创建已解锁空格子
        /// </summary>
        public static GridCell CreateUnlockedCell(int row, int column)
        {
            return new GridCell
            {
                row = row,
                column = column,
                _isLocked = false,
                _lockedItemType = ItemType.None,
                _lockedItemLevel = 0,
                _currentItemType = ItemType.None,
                _itemCount = 0
            };
        }

        /// <summary>
        /// 创建已解锁有物品的格子
        /// </summary>
        public static GridCell CreateFilledCell(int row, int column, ItemType itemType, int count = 1)
        {
            return new GridCell
            {
                row = row,
                column = column,
                _isLocked = false,
                _lockedItemType = ItemType.None,
                _lockedItemLevel = 0,
                _currentItemType = itemType,
                _itemCount = count > 99 ? 99 : count
            };
        }

        /// <summary>
        /// 放置物品到该格子（格子必须已解锁且为空）
        /// </summary>
        public bool TryPlaceItem(ItemType itemType, int count = 1)
        {
            if (_isLocked || _currentItemType != ItemType.None)
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
            if (_isLocked || _currentItemType != itemType)
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
            if (_isLocked || _currentItemType == ItemType.None)
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
        /// 清空格子中的物品（不可锁定格子）
        /// </summary>
        public void ClearItem()
        {
            if (!_isLocked)
            {
                _currentItemType = ItemType.None;
                _itemCount = 0;
            }
        }

        /// <summary>
        /// 解锁格子（从锁定状态变为已解锁空格）
        /// </summary>
        public void Unlock()
        {
            _isLocked = false;
            _lockedItemType = ItemType.None;
            _lockedItemLevel = 0;
        }

        /// <summary>
        /// 获取该格子的UI显示名称
        /// </summary>
        public string GetDisplayName()
        {
            if (_isLocked)
            {
                return ItemConfig.GetItemName(_lockedItemType);
            }

            if (_currentItemType != ItemType.None)
            {
                return ItemConfig.GetItemName(_currentItemType);
            }

            return "空";
        }
    }
}
