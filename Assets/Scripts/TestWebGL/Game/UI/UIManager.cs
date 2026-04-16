using System;
using System.Collections.Generic;
using UnityEngine;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// UI层管理器
    /// 遵循MVC架构，只监听事件，不直接调用业务逻辑
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance => _instance;

        [Header("UI页面引用")]
        public GameObject MainPanel;
        public GameObject BackpackPanel;
        public GameObject CraftingPanel;
        public GameObject ExplorationPanel;
        public GameObject OrderPanel;
        public GameObject PlayerInfoPanel;

        private Dictionary<Type, UIPanelBase> _panels = new Dictionary<Type, UIPanelBase>();

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializePanels();
            RegisterEventListeners();
            UnityEngine.Debug.Log("[UIManager] UI系统初始化完成");
        }

        private void InitializePanels()
        {
            foreach (var panel in GetComponentsInChildren<UIPanelBase>(true))
            {
                _panels[panel.GetType()] = panel;
                panel.Initialize();
            }
        }

        private void RegisterEventListeners()
        {
            // 监听所有游戏事件，只做UI响应
            var eventBus = GameLoop.Instance.EventBus;

            // 物品事件
            eventBus.Listen<ItemMovedEvent>(OnItemMoved);
            eventBus.Listen<ItemSwappedEvent>(OnItemSwapped);
            eventBus.Listen<MergeCompleteEvent>(OnMergeComplete);
            eventBus.Listen<ItemCraftedEvent>(OnItemCrafted);

            // 探索事件
            eventBus.Listen<ExplorationCompleteEvent>(OnExplorationComplete);

            // 订单事件
            eventBus.Listen<OrderSubmittedEvent>(OnOrderSubmitted);

            // 玩家事件
            eventBus.Listen<LevelUpEvent>(OnLevelUp);
            eventBus.Listen<ExperienceGainedEvent>(OnExperienceGained);
        }

        /// <summary>
        /// 显示指定面板
        /// </summary>
        public void ShowPanel<T>() where T : UIPanelBase
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
            {
                panel.Show();
            }
        }

        /// <summary>
        /// 隐藏指定面板
        /// </summary>
        public void HidePanel<T>() where T : UIPanelBase
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
            {
                panel.Hide();
            }
        }

        /// <summary>
        /// 获取面板实例
        /// </summary>
        public T GetPanel<T>() where T : UIPanelBase
        {
            return _panels.TryGetValue(typeof(T), out var panel) ? (T)panel : null;
        }

        #region 事件响应方法 - 纯UI逻辑
        private void OnItemMoved(ItemMovedEvent e)
        {
            GetPanel<BackpackUI>()?.RefreshCell(e.FromCellId);
            GetPanel<BackpackUI>()?.RefreshCell(e.ToCellId);
        }

        private void OnItemSwapped(ItemSwappedEvent e)
        {
            GetPanel<BackpackUI>()?.RefreshCell(e.CellIdA);
            GetPanel<BackpackUI>()?.RefreshCell(e.CellIdB);
        }

        private void OnMergeComplete(MergeCompleteEvent e)
        {
            GetPanel<BackpackUI>()?.PlayMergeAnimation(e.CellId);
        }

        private void OnItemCrafted(ItemCraftedEvent e)
        {
            GetPanel<CollectionUI>()?.UpdateCollection(e.ItemType);
        }

        private void OnExplorationComplete(ExplorationCompleteEvent e)
        {
            GetPanel<ExplorationUI>()?.ShowExploreResult(e.GeneratedCellIds);
            foreach (var cellId in e.GeneratedCellIds)
            {
                GetPanel<BackpackUI>()?.RefreshCell(cellId);
            }
        }

        private void OnOrderSubmitted(OrderSubmittedEvent e)
        {
            GetPanel<OrderUI>()?.RefreshOrders();
            ShowRewardPopup(e.RewardExp, e.RewardGold);
        }

        private void OnLevelUp(LevelUpEvent e)
        {
            ShowLevelUpAnimation(e.OldLevel, e.NewLevel);
        }

        private void OnExperienceGained(ExperienceGainedEvent e)
        {
            GetPanel<PlayerInfoUI>()?.UpdateExperience();
        }

        private void ShowRewardPopup(int exp, int gold)
        {
            // TODO: 通用奖励弹窗
            UnityEngine.Debug.Log($"获得奖励: 经验+{exp} 金币+{gold}");
        }

        private void ShowLevelUpAnimation(int oldLevel, int newLevel)
        {
            // TODO: 升级动画
            UnityEngine.Debug.Log($"升级! Lv{oldLevel} -> Lv{newLevel}");
        }
        #endregion
    }

    /// <summary>
    /// UI面板基类
    /// </summary>
    public abstract class UIPanelBase : MonoBehaviour
    {
        public virtual void Initialize() { }
        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
        public virtual void Refresh() { }
    }
}
