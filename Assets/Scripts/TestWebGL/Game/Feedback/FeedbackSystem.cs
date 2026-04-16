using UnityEngine;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.Feedback
{
    /// <summary>
    /// 反馈系统 - 音效与振动
    /// 全局唯一 自动监听所有事件
    /// 零侵入 无耦合 可单独开关
    /// </summary>
    public static class FeedbackSystem
    {
        /// <summary>
        /// 反馈开关
        /// </summary>
        public static bool SoundEnabled = true;
        public static bool VibrationEnabled = true;

        /// <summary>
        /// 初始化并绑定所有事件
        /// </summary>
        public static void Initialize(EventBus eventBus)
        {
            // 背包交互
            eventBus.Listen<CellClickEvent>(OnCellClick);
            eventBus.Listen<CellDoubleClickEvent>(OnCellDoubleClick);
            eventBus.Listen<MergeCompleteEvent>(OnMergeComplete);
            eventBus.Listen<ItemMovedEvent>(OnItemMoved);
            eventBus.Listen<ItemSwappedEvent>(OnItemSwapped);

            // 探索系统
            eventBus.Listen<ExplorationCompleteEvent>(OnExplorationComplete);

            // 订单系统
            eventBus.Listen<OrderSubmittedEvent>(OnOrderSubmitted);

            // 玩家状态
            eventBus.Listen<LevelUpEvent>(OnLevelUp);
            eventBus.Listen<StaminaRecoverEvent>(OnStaminaRecover);
        }

        private static void OnCellClick(CellClickEvent e)
        {
            PlaySound(SoundType.Click);
        }

        private static void OnCellDoubleClick(CellDoubleClickEvent e)
        {
            PlaySound(SoundType.DoubleClick);
            PlayVibration(VibrationType.Light);
        }

        private static void OnMergeComplete(MergeCompleteEvent e)
        {
            PlaySound(SoundType.Merge);
            PlayVibration(VibrationType.Medium);
        }

        private static void OnItemMoved(ItemMovedEvent e)
        {
            PlaySound(SoundType.DragDrop);
        }

        private static void OnItemSwapped(ItemSwappedEvent e)
        {
            PlaySound(SoundType.Swap);
            PlayVibration(VibrationType.Light);
        }

        private static void OnExplorationComplete(ExplorationCompleteEvent e)
        {
            PlaySound(SoundType.Explore);
            PlayVibration(VibrationType.Medium);
        }

        private static void OnOrderSubmitted(OrderSubmittedEvent e)
        {
            PlaySound(SoundType.Reward);
            PlayVibration(VibrationType.Heavy);
        }

        private static void OnLevelUp(LevelUpEvent e)
        {
            PlaySound(SoundType.LevelUp);
            PlayVibration(VibrationType.Heavy);
        }

        private static void OnStaminaRecover(StaminaRecoverEvent e)
        {
            PlaySound(SoundType.Recover);
        }

        private static void PlaySound(SoundType type)
        {
            if (!SoundEnabled) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            // 微信小游戏音频接口
            WX.Audio.PlaySound((int)type);
#else
            // 编辑器/其他平台
            Debug.Log($"[Sound] {type}");
#endif
        }

        private static void PlayVibration(VibrationType type)
        {
            if (!VibrationEnabled) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            // 微信小游戏振动接口
            WX.Vibrate(type);
#else
            // 编辑器/其他平台
            Handheld.Vibrate();
            Debug.Log($"[Vibration] {type}");
#endif
        }
    }

    public enum SoundType
    {
        Click = 0,
        DoubleClick = 1,
        Merge = 2,
        DragDrop = 3,
        Swap = 4,
        Explore = 5,
        Reward = 6,
        LevelUp = 7,
        Recover = 8,
        Error = 9
    }

    public enum VibrationType
    {
        Light = 15,
        Medium = 30,
        Heavy = 50
    }
}
