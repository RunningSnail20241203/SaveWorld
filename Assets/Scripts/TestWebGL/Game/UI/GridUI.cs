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
        public Vector2 cellSize = new Vector2(100, 100);
        public Vector2 spacing = new Vector2(5, 5);
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

            if (gridContainer == null)
            {
                CreateGridContainer();
            }

            if (gridLayout == null)
            {
                SetupGridLayout();
            }

            CreateCellUIs();
            Refresh();

            // 订阅网格变化事件
            _gridManager.OnCellChanged += OnGridCellChanged;
            _gridManager.OnGridUnlocked += OnGridUnlocked;

            Debug.Log("[GridUI] 网格UI初始化完成");
        }

        /// <summary>
        /// 创建网格容器
        /// </summary>
        private void CreateGridContainer()
        {
            GameObject containerGO = new GameObject("GridContainer");
            containerGO.transform.SetParent(transform, false);
            gridContainer = containerGO.AddComponent<RectTransform>();

            // 设置容器位置和大小
            gridContainer.anchorMin = new Vector2(0.5f, 0.5f);
            gridContainer.anchorMax = new Vector2(0.5f, 0.5f);
            gridContainer.pivot = new Vector2(0.5f, 0.5f);
            gridContainer.anchoredPosition = Vector2.zero;
            gridContainer.sizeDelta = new Vector2(800, 1000);
        }

        /// <summary>
        /// 设置网格布局
        /// </summary>
        private void SetupGridLayout()
        {
            gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = cellSize;
            gridLayout.spacing = spacing;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = cols;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
        }

        /// <summary>
        /// 创建所有格子UI
        /// </summary>
        private void CreateCellUIs()
        {
            _cellUIs = new GridCellUI[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    CreateCellUI(row, col);
                }
            }
        }

        /// <summary>
        /// 创建单个格子UI
        /// </summary>
        private void CreateCellUI(int row, int col)
        {
            GameObject cellGO = Instantiate(cellPrefab, gridContainer);
            cellGO.name = $"Cell_{row}_{col}";

            GridCellUI cellUI = cellGO.GetComponent<GridCellUI>();
            if (cellUI == null)
            {
                cellUI = cellGO.AddComponent<GridCellUI>();
            }

            cellUI.Initialize(row, col);
            _cellUIs[row, col] = cellUI;
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