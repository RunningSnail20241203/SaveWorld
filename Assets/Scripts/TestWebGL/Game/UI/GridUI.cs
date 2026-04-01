using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestWebGL.Game.Grid;
using TestWebGL.Game.Items;
using TestWebGL.Game.Core;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// 网格UI - 显示9×7的游戏网格
    /// 每个格子显示物品信息，支持点击交互
    /// </summary>
    public class GridUI : MonoBehaviour
    {
        [Header("网格设置")]
        public GridLayoutGroup gridLayout;
        public GameObject cellPrefab;
        public RectTransform gridContainer;

        [Header("布局参数")]
        public Vector2 cellSize = new Vector2(120, 120);
        public Vector2 spacing = new Vector2(8, 8);
        public int rows = 9;
        public int cols = 7;

        // 格子UI组件数组
        private GridCellUI[,] _cellUIs;
        private GridManager _gridManager;

        /// <summary>
        /// 初始化网格UI
        /// </summary>
        public void Initialize()
        {
            _gridManager = GameManager.Instance.GetGridManager();

            // 如果cellPrefab未赋值，尝试自动加载
            if (cellPrefab == null)
            {
                // 尝试从Resources加载
                cellPrefab = Resources.Load<GameObject>("Prefabs/UI/GridCellUI");
                
                if (cellPrefab == null)
                {
                    Debug.LogError("[GridUI] 无法加载GridCellUI预制体，请在Inspector中手动赋值cellPrefab");
                    return;
                }
                Debug.Log("[GridUI] 自动加载GridCellUI预制体成功");
            }

            // 从预制件获取gridContainer和gridLayout
            if (gridContainer == null)
            {
                gridContainer = transform.Find("GridContainer")?.GetComponent<RectTransform>();
            }

            if (gridLayout == null && gridContainer != null)
            {
                gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
            }

            // 初始化格子UI数组
            if (_cellUIs == null)
            {
                _cellUIs = new GridCellUI[rows, cols];
                
                // 从GridContainer获取所有已存在的GridCellUI
                if (gridContainer != null)
                {
                    for (int i = 0; i < gridContainer.childCount && i < rows * cols; i++)
                    {
                        Transform child = gridContainer.GetChild(i);
                        GridCellUI cellUI = child.GetComponent<GridCellUI>();
                        if (cellUI != null)
                        {
                            int row = i / cols;
                            int col = i % cols;
                            cellUI.Initialize(row, col);
                            _cellUIs[row, col] = cellUI;
                        }
                    }
                }
            }

            Refresh();

            // 订阅网格变化事件
            _gridManager.OnCellChanged += OnGridCellChanged;
            _gridManager.OnGridUnlocked += OnGridUnlocked;

            Debug.Log("[GridUI] 网格UI初始化完成");
        }

        /// <summary>
        /// 刷新所有格子显示
        /// </summary>
        public void Refresh()
        {
            if (_cellUIs == null) return;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (_cellUIs[row, col] != null)
                    {
                        GridCell cell = _gridManager.GetCell(row, col);
                        _cellUIs[row, col].UpdateDisplay(cell);
                    }
                }
            }
        }

        /// <summary>
        /// 网格格子变化事件处理
        /// </summary>
        private void OnGridCellChanged(int row, int col, GridCell cell)
        {
            if (_cellUIs != null && row >= 0 && row < rows && col >= 0 && col < cols)
            {
                _cellUIs[row, col].UpdateDisplay(cell);
            }
        }

        /// <summary>
        /// 网格解锁事件处理
        /// </summary>
        private void OnGridUnlocked(int row, int col, GridCell cell)
        {
            OnGridCellChanged(row, col, cell);
            // 可以添加解锁动画效果
        }

        /// <summary>
        /// 获取格子UI
        /// </summary>
        public GridCellUI GetCellUI(int row, int col)
        {
            if (row >= 0 && row < rows && col >= 0 && col < cols)
            {
                return _cellUIs[row, col];
            }
            return null;
        }

        private void OnDestroy()
        {
            if (_gridManager != null)
            {
                _gridManager.OnCellChanged -= OnGridCellChanged;
                _gridManager.OnGridUnlocked -= OnGridUnlocked;
            }
        }
    }
}