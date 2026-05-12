using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 玩家状态UI面板
    /// 显示体力 等级 经验 金币
    /// 自动刷新 动画反馈
    /// </summary>
    public class PlayerStatusUI : UIPanelBase
    {
        [Header("体力")]
        public Slider StaminaSlider;
        public TextMeshProUGUI StaminaText;

        [Header("等级")]
        public TextMeshProUGUI LevelText;
        public Slider ExpSlider;
        public TextMeshProUGUI ExpText;

        [Header("金币")]
        public TextMeshProUGUI GoldText;

        private StateMutator _stateMutator;
        private EventBus _eventBus;
        private bool _initialized = false;

        void Awake()
        {
            var panel = GetComponent<PlayerInfoPanel>();
            if (panel != null)
            {
                if (StaminaSlider == null) StaminaSlider = panel.staminaSlider;
                if (StaminaText == null) StaminaText = panel.staminaText;
                if (LevelText == null) LevelText = panel.levelText;
                if (ExpSlider == null) ExpSlider = panel.experienceSlider;
                if (ExpText == null) ExpText = panel.experienceText;
                if (GoldText == null && panel.playTimeText != null)
                {
                    // 如果GoldText没设置，创建一个简单的TMP文本
                }
            }
        }

        public override void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _stateMutator = GameLoop.Instance.StateMutator;
            _eventBus = GameLoop.Instance.EventBus;

            // 绑定事件监听
            _eventBus.Listen<ExplorationCompleteEvent>(OnExplorationComplete);
            _eventBus.Listen<OrderSubmittedEvent>(OnOrderSubmitted);
            _eventBus.Listen<ExperienceGainedEvent>(OnExperienceGained);
            _eventBus.Listen<StaminaRecoverEvent>(OnStaminaRecover);
            _eventBus.Listen<LevelUpEvent>(OnLevelUp);

            RefreshAll();
        }

        private void OnExplorationComplete(ExplorationCompleteEvent e)
        {
            RefreshStamina();
            RefreshGold();
        }

        private void OnOrderSubmitted(OrderSubmittedEvent e)
        {
            RefreshGold();
        }

        private void OnExperienceGained(ExperienceGainedEvent e)
        {
            RefreshLevel();
            PlayExpAnimation(e.Amount);
        }

        private void OnStaminaRecover(StaminaRecoverEvent e)
        {
            RefreshStamina();
            PlayStaminaAnimation(e.Amount);
        }

        private void OnLevelUp(LevelUpEvent e)
        {
            RefreshAll();
            PlayLevelUpAnimation();
        }

        public override void Refresh()
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshStamina();
            RefreshLevel();
            RefreshGold();
        }

        private void RefreshStamina()
        {
            if (StaminaSlider == null || StaminaText == null) return;
            var player = _stateMutator.CurrentState.Player;
            int maxStamina = 20 + player.Level * 5;

            StaminaSlider.maxValue = maxStamina;
            StaminaSlider.value = player.Stamina;
            StaminaText.text = $"{player.Stamina} / {maxStamina}";
        }

        private void RefreshLevel()
        {
            if (LevelText == null) return;
            var player = _stateMutator.CurrentState.Player;

            LevelText.text = $"Lv\u00A0{player.Level}";

            if (ExpSlider != null && ExpText != null)
            {
                int currentExp = player.Experience;
                int needExp = GetExpForLevel(player.Level + 1);
                int prevLevelExp = GetExpForLevel(player.Level);

                float progress = (float)(currentExp - prevLevelExp) / (needExp - prevLevelExp);
                ExpSlider.value = Mathf.Clamp01(progress);
                ExpText.text = $"{currentExp - prevLevelExp} / {needExp - prevLevelExp}";
            }
        }

        private void RefreshGold()
        {
            if (GoldText == null) return;
            var player = _stateMutator.CurrentState.Player;
            GoldText.text = player.Gold.ToString();
        }

        public static int GetExpForLevel(int level)
        {
            return 100 + level * 50;
        }

        private void PlayExpAnimation(int amount)
        {
        }

        private void PlayStaminaAnimation(int amount)
        {
        }

        private void PlayLevelUpAnimation()
        {
        }
    }
}
