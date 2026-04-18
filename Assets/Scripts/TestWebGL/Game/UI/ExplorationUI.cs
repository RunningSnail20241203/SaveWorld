using UnityEngine;
using UnityEngine.UI;
using SaveWorld.Game.Exploration;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 探索UI
    /// </summary>
    public class ExplorationUI : UIPanelBase
    {
        [Header("探索按钮")]
        public Button ExploreButton;
        public Text StaminaCostText;
        public Text RemainingStaminaText;

        [Header("探索结果")]
        public GameObject ResultPopup;
        public Transform ResultItemsContainer;
        public GameObject ResultItemPrefab;

        public override void Initialize()
        {
            ExploreButton.onClick.AddListener(OnExploreClicked);
            StaminaCostText.text = ExplorationSystem.Instance.GetExploreCost().ToString();

            UpdateStaminaDisplay();

            // 监听体力变化
            Player.PlayerManager.Instance.OnStaminaChanged += OnStaminaChanged;
        }

        private void OnExploreClicked()
        {
            bool success = ExplorationSystem.Instance.TryExplore();

            if (!success)
            {
                // 显示失败提示
                UnityEngine.Debug.Log("探索失败");
            }
        }

        /// <summary>
        /// 显示探索结果
        /// </summary>
        public void ShowExploreResult(int[] cellIds)
        {
            ResultPopup.SetActive(true);

            // 清空旧结果
            foreach (Transform child in ResultItemsContainer)
            {
                Destroy(child.gameObject);
            }

            // 显示新获得的物品
            foreach (int cellId in cellIds)
            {
                var cell = Grid.GridManager.Instance.GetCell(cellId);
                if (cell.HasItem)
                {
                    var itemObj = Instantiate(ResultItemPrefab, ResultItemsContainer);
                    var icon = itemObj.GetComponentInChildren<Image>();
                    icon.sprite = Items.ItemIconManager.Instance.GetIcon(cell.ItemType);
                }
            }
        }

        private void OnStaminaChanged(int newStamina, int maxStamina)
        {
            UpdateStaminaDisplay();
        }

        private void UpdateStaminaDisplay()
        {
            int current = Player.PlayerManager.Instance.GetCurrentStamina();
            int cost = ExplorationSystem.Instance.GetExploreCost();

            RemainingStaminaText.text = current.ToString();
            ExploreButton.interactable = current >= cost;
        }

        public void CloseResultPopup()
        {
            ResultPopup.SetActive(false);
        }

        public override void Refresh()
        {
            UpdateStaminaDisplay();
        }
    }
}
