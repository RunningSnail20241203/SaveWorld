using UnityEngine;

namespace TestWebGL.Game.Feedback
{
    /// <summary>
    /// 反馈系统 - 处理游戏中的反馈机制
    /// 包括音效、振动、视觉反馈等
    /// </summary>
    public class FeedbackSystem : MonoBehaviour
    {
        // 音效配置
        public enum SoundType
        {
            CraftSuccess,      // 合成成功 "叮"
            CraftFailure,      // 合成失败 "咚"
            UnlockSuccess,     // 解锁成功 "咔哒" + 升级声
            ExploreClick,      // 探索点击 "嗖"
            OrderComplete,     // 订单完成 "金币声"
            GridFull,          // 满格提示 "咚"
        }

        // 振动配置
        public struct VibrationPattern
        {
            public float duration;     // 持续时间（秒）
            public float intensity;    // 强度（0-1）
        }

        private static FeedbackSystem s_instance;

        public static FeedbackSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("FeedbackSystem");
                    s_instance = go.AddComponent<FeedbackSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 音效字典
        private System.Collections.Generic.Dictionary<SoundType, Enums.AudioClip> _soundMap;

        // 是否启用反馈
        public bool enableVibration = true;
        public bool enableSound = true;
        public bool enableVisualFeedback = true;

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSounds();
        }

        /// <summary>
        /// 初始化音效（预留实现）
        /// 后期需要导入音频assets
        /// </summary>
        private void InitializeSounds()
        {
            _soundMap = new System.Collections.Generic.Dictionary<SoundType, Enums.AudioClip>();
            Debug.Log("[FeedbackSystem] 音效系统已初始化");
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySound(SoundType soundType)
        {
            if (!enableSound)
                return;

            // 后期实现具体的音频播放逻辑
            Debug.Log($"[FeedbackSystem] 播放音效: {soundType}");

            switch (soundType)
            {
                case SoundType.CraftSuccess:
                    // 播放 "叮" 清脆声音
                    if (_soundMap.ContainsKey(soundType))
                        Debug.Log("▶ 播放: 清脆的叮声");
                    break;

                case SoundType.CraftFailure:
                    // 播放 "咚" 低沉短促声音
                    if (_soundMap.ContainsKey(soundType))
                        Debug.Log("▶ 播放: 低沉的咚声");
                    break;

                case SoundType.UnlockSuccess:
                    // 播放 "咔哒" + 升级声
                    if (_soundMap.ContainsKey(soundType))
                        Debug.Log("▶ 播放: 咔哒声 + 升级声");
                    break;

                case SoundType.ExploreClick:
                    // 播放 "嗖" 轻快声音
                    if (_soundMap.ContainsKey(soundType))
                        Debug.Log("▶ 播放: 轻快的嗖声");
                    break;

                case SoundType.OrderComplete:
                    // 播放 "金币声"
                    if (_soundMap.ContainsKey(soundType))
                        Debug.Log("▶ 播放: 欢快的金币声");
                    break;

                case SoundType.GridFull:
                    // 播放 "咚" 与失败相同
                    if (_soundMap.ContainsKey(soundType))
                        Debug.Log("▶ 播放: 警告的咚声");
                    break;
            }
        }

        /// <summary>
        /// 触发振动
        /// </summary>
        public void TriggerVibration(float duration = 0.1f, float intensity = 0.5f)
        {
            if (!enableVibration)
                return;

            // WebGL/小游戏平台需要调用特定API
            Debug.Log($"[FeedbackSystem] 振动: {duration}秒, 强度{intensity:P0}");

            // 后期需要接入微信 API 的震动接口
            // wx.vibrateLong() 或 wx.vibrateShort()
        }

        /// <summary>
        /// 轻微振动（用于失败/满格）
        /// </summary>
        public void LightVibration()
        {
            TriggerVibration(0.05f, 0.3f);
        }

        /// <summary>
        /// 中等振动（用于解锁成功）
        /// </summary>
        public void MediumVibration()
        {
            TriggerVibration(0.1f, 0.6f);
        }

        /// <summary>
        /// 强烈振动（用于重要事件）
        /// </summary>
        public void StrongVibration()
        {
            TriggerVibration(0.2f, 1.0f);
        }

        /// <summary>
        /// 触发视觉反馈 - 满格闪烁
        /// 屏幕边缘闪烁红色（0.2秒）
        /// 后期与UI系统集成
        /// </summary>
        public void FullGridFlash()
        {
            if (!enableVisualFeedback)
                return;

            Debug.Log("[FeedbackSystem] 触发满格闪烁反馈");
            // 后期实现与Canvas连接，闪烁屏幕边缘
            PlaySound(SoundType.GridFull);
            LightVibration();
        }

        /// <summary>
        /// 合成成功反馈
        /// </summary>
        public void CraftSuccessFeedback()
        {
            PlaySound(SoundType.CraftSuccess);
            // 可以添加粒子效果、动画等
            Debug.Log("[FeedbackSystem] 合成成功反馈");
        }

        /// <summary>
        /// 合成失败反馈
        /// </summary>
        public void CraftFailureFeedback()
        {
            PlaySound(SoundType.CraftFailure);
            LightVibration();
            Debug.Log("[FeedbackSystem] 合成失败反馈");
        }

        /// <summary>
        /// 解锁成功反馈
        /// </summary>
        public void UnlockSuccessFeedback()
        {
            PlaySound(SoundType.UnlockSuccess);
            MediumVibration();
            Debug.Log("[FeedbackSystem] 解锁成功反馈");
        }

        /// <summary>
        /// 探索成功反馈
        /// </summary>
        public void ExploreSuccessFeedback()
        {
            PlaySound(SoundType.ExploreClick);
            LightVibration();
            Debug.Log("[FeedbackSystem] 探索成功反馈");
        }

        /// <summary>
        /// 订单完成反馈
        /// </summary>
        public void OrderCompleteFeedback()
        {
            PlaySound(SoundType.OrderComplete);
            StrongVibration();
            Debug.Log("[FeedbackSystem] 订单完成反馈");
        }
    }

    /// <summary>
    /// 枚举辅助类（暂时避免AudioClip依赖）
    /// </summary>
    public class Enums
    {
        public class AudioClip { }
    }
}
