using UnityEngine;
using UnityEngine.UI;
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
        public Text StaminaText;

        [Header("等级")]
        public Text LevelText;
        public Slider ExpSlider;
        public Text ExpText;

        [Header("金币")]
        public Text GoldText;

        private StateMutator _stateMutator;
        private EventBus _eventBus;

        public override void Initialize()
        {
            base.Initialize();

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
            var player = _stateMutator.CurrentState.Player;
            int maxStamina = 20 + player.Level * 5;

            StaminaSlider.maxValue = maxStamina;
            StaminaSlider.value = player.Stamina;
            StaminaText.text = $"{player.Stamina} / {maxStamina}";
        }

        private void RefreshLevel()
        {
            var player = _stateMutator.CurrentState.Player;
            
            LevelText.text = $"Lv {player.Level}";
            
            int currentExp = player.Experience;
            int needExp = GetExpForLevel(player.Level + 1);
            int prevLevelExp = GetExpForLevel(player.Level);

            float progress = (float)(currentExp - prevLevelExp) / (needExp - prevLevelExp);
            
            ExpSlider.value = Mathf.Clamp01(progress);
            ExpText.text = $"{currentExp - prevLevelExp} / {needExp - prevLevelExp}";
        }

        private void RefreshGold()
        {
            var player = _stateMutator.CurrentState.Player;
            GoldText.text = player.Gold.ToString();
        }

        /// <summary>
        /// 获取对应等级所需经验
        /// 经验曲线: 100 + level * 50
        /// </summary>
        public static int GetExpForLevel(int level)
        {
            return 100 + level * 50;
        }

        private void PlayExpAnimation(int amount)
        {
            // 经验增长动画
        }

        private void PlayStaminaAnimation(int amount)
        {
            // 体力恢复动画
        }

        private void PlayLevelUpAnimation()
        {
            // 升级特效动画
        }
    }
}
