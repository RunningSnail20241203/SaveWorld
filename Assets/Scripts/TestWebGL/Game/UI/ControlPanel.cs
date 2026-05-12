using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 控制面板 - 底部按钮栏
    /// </summary>
    public class ControlPanel : MonoBehaviour
    {
        public Button exploreButton;
        public Button settingsButton;
        public Button ordersButton;
        public Button achievementsButton;
        public Button saveButton;

        public TextMeshProUGUI exploreButtonText;
        public TextMeshProUGUI settingsButtonText;
        public TextMeshProUGUI ordersButtonText;
        public TextMeshProUGUI achievementsButtonText;
        public TextMeshProUGUI saveButtonText;

        private EventBus _eventBus;

        void Start()
        {
            _eventBus = GameLoop.Instance.EventBus;

            if (exploreButton != null)
                exploreButton.onClick.AddListener(OnExploreClick);

            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveClick);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClick);

            if (ordersButton != null)
                ordersButton.onClick.AddListener(OnOrdersClick);

            if (achievementsButton != null)
                achievementsButton.onClick.AddListener(OnAchievementsClick);
        }

        private void OnExploreClick()
        {
            Debug.Log("[ControlPanel] 探索按钮点击");
            _eventBus.Publish(new ExplorationRequestEvent { StaminaCost = 5 });
        }

        private void OnSaveClick()
        {
            Debug.Log("[ControlPanel] 保存按钮点击");
            var mutator = GameLoop.Instance.StateMutator;
            if (mutator != null) mutator.SaveCurrentState();
        }

        private void OnSettingsClick()
        {
            Debug.Log("[ControlPanel] 设置按钮点击");
        }

        private void OnOrdersClick()
        {
            Debug.Log("[ControlPanel] 订单按钮点击");
        }

        private void OnAchievementsClick()
        {
            Debug.Log("[ControlPanel] 成就按钮点击");
        }
    }
}
