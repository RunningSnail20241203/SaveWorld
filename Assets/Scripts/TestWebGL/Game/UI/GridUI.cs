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
        public Image backgroundImage;

        [Header("布局参数")]
        public Vector2 cellSize = new Vector2(UIThemeConfig.GridCellSize, UIThemeConfig.GridCellSize);
        public Vector2 spacing = new Vector2(UIThemeConfig.GridCellSpacing, UIThemeConfig.GridCellSpacing);
        public int rows = UIThemeConfig.GridRows;
        public int cols = UIThemeConfig.GridColumns;

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

                // 动态创建缺失的格子
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        if (_cellUIs[row, col] == null && cellPrefab != null && gridContainer != null)
                        {
                            GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                            GridCellUI cellUI = cellObj.GetComponent<GridCellUI>();
                            if (cellUI != null)
                            {
                                cellUI.Initialize(row, col);
                                _cellUIs[row, col] = cellUI;
                            }
                        }
                    }
                }
            }

            // 应用主题样式
            ApplyTheme();
            
            // 自动适配屏幕
            AutoFitToScreen();

            Refresh();

            // 订阅网格变化事件
            _gridManager.OnCellChanged += OnGridCellChanged;
            _gridManager.OnGridUnlocked += OnGridUnlocked;

            Debug.Log("[GridUI] 网格UI初始化完成");
        }

        /// <summary>
        /// 应用主题样式
        /// </summary>
        private void ApplyTheme()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = UIThemeConfig.BackgroundCard;
            }

            if (gridLayout != null)
            {
                gridLayout.cellSize = cellSize;
                gridLayout.spacing = spacing;
                gridLayout.padding = new RectOffset(8, 8, 8, 8);
            }
        }

        /// <summary>
        /// 自动适配屏幕尺寸
        /// </summary>
        private void AutoFitToScreen()
        {
            // ✅ 布局由预制件处理，CanvasScaler已经负责屏幕适配
            // 运行时不再动态修改布局，避免覆盖预制件正确设置
            Debug.Log("[GridUI] 使用预制件原生布局，跳过动态适配");
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