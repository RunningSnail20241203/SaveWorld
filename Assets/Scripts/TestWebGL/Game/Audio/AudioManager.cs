using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestWebGL.Game.Audio
{
    /// <summary>
    /// 音频管理器
    /// 负责游戏音效和背景音乐的播放控制
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager s_instance;
        public static AudioManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("AudioManager");
                    s_instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        [Header("音频源")]
        public AudioSource musicSource;      // 背景音乐
        public AudioSource sfxSource;        // 音效

        [Header("音频设置")]
        public float masterVolume = 1.0f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1.0f;
        public bool musicEnabled = true;
        public bool sfxEnabled = true;

        // 音频资源字典
        private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
        
        // 当前播放的背景音乐
        private string _currentMusic = null;
        
        // 初始化状态
        private bool _isInitialized = false;

        /// <summary>
        /// 初始化音频管理器
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[AudioManager] 初始化音频管理器...");

            // 创建音频源
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxGO = new GameObject("SFXSource");
                sfxGO.transform.SetParent(transform);
                sfxSource = sfxGO.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            // 预加载音效
            PreloadAudioClips();

            _isInitialized = true;
            Debug.Log("[AudioManager] 音频管理器初始化完成");
        }

        /// <summary>
        /// 预加载音效资源
        /// </summary>
        private void PreloadAudioClips()
        {
            // 从Resources加载音效
            // 这里可以根据实际资源路径加载
            Debug.Log("[AudioManager] 预加载音效资源...");
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayMusic(string musicName, bool loop = true)
        {
            if (!musicEnabled || musicSource == null) return;

            // 如果正在播放相同的音乐，不重复播放
            if (_currentMusic == musicName && musicSource.isPlaying) return;

            AudioClip clip = GetAudioClip(musicName);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 找不到背景音乐: {musicName}");
                return;
            }

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();

            _currentMusic = musicName;
            Debug.Log($"[AudioManager] 播放背景音乐: {musicName}");
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
                _currentMusic = null;
                Debug.Log("[AudioManager] 停止背景音乐");
            }
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Pause();
                Debug.Log("[AudioManager] 暂停背景音乐");
            }
        }

        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        public void ResumeMusic()
        {
            if (musicSource != null && !musicSource.isPlaying && _currentMusic != null)
            {
                musicSource.UnPause();
                Debug.Log("[AudioManager] 恢复背景音乐");
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(string sfxName)
        {
            if (!sfxEnabled || sfxSource == null) return;

            AudioClip clip = GetAudioClip(sfxName);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 找不到音效: {sfxName}");
                return;
            }

            sfxSource.volume = sfxVolume * masterVolume;
            sfxSource.PlayOneShot(clip);

            Debug.Log($"[AudioManager] 播放音效: {sfxName}");
        }

        /// <summary>
        /// 播放音效（带音量控制）
        /// </summary>
        public void PlaySFX(string sfxName, float volumeScale)
        {
            if (!sfxEnabled || sfxSource == null) return;

            AudioClip clip = GetAudioClip(sfxName);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 找不到音效: {sfxName}");
                return;
            }

            sfxSource.volume = sfxVolume * masterVolume * volumeScale;
            sfxSource.PlayOneShot(clip);

            Debug.Log($"[AudioManager] 播放音效: {sfxName}, 音量: {volumeScale}");
        }

        /// <summary>
        /// 获取音效资源
        /// </summary>
        private AudioClip GetAudioClip(string clipName)
        {
            if (_audioClips.TryGetValue(clipName, out AudioClip clip))
            {
                return clip;
            }

            // 尝试从Resources加载
            clip = Resources.Load<AudioClip>($"Audio/{clipName}");
            if (clip != null)
            {
                _audioClips[clipName] = clip;
                return clip;
            }

            Debug.LogWarning($"[AudioManager] 无法加载音效: {clipName}");
            return null;
        }

        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolume();
            Debug.Log($"[AudioManager] 主音量设置为: {masterVolume}");
        }

        /// <summary>
        /// 设置音乐音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolume();
            Debug.Log($"[AudioManager] 音乐音量设置为: {musicVolume}");
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] 音效音量设置为: {sfxVolume}");
        }

        /// <summary>
        /// 更新音量
        /// </summary>
        private void UpdateVolume()
        {
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }
        }

        /// <summary>
        /// 启用/禁用音乐
        /// </summary>
        public void SetMusicEnabled(bool enabled)
        {
            musicEnabled = enabled;
            if (!enabled)
            {
                StopMusic();
            }
            Debug.Log($"[AudioManager] 音乐已{(enabled ? "启用" : "禁用")}");
        }

        /// <summary>
        /// 启用/禁用音效
        /// </summary>
        public void SetSFXEnabled(bool enabled)
        {
            sfxEnabled = enabled;
            Debug.Log($"[AudioManager] 音效已{(enabled ? "启用" : "禁用")}");
        }

        /// <summary>
        /// 检查音乐是否正在播放
        /// </summary>
        public bool IsMusicPlaying()
        {
            return musicSource != null && musicSource.isPlaying;
        }

        /// <summary>
        /// 获取当前播放的音乐名称
        /// </summary>
        public string GetCurrentMusic()
        {
            return _currentMusic;
        }

        /// <summary>
        /// 获取音频设置信息
        /// </summary>
        public string GetAudioInfo()
        {
            return $"音乐: {(musicEnabled ? "开启" : "关闭")}, 音效: {(sfxEnabled ? "开启" : "关闭")}, " +
                   $"主音量: {masterVolume:F1}, 音乐音量: {musicVolume:F1}, 音效音量: {sfxVolume:F1}";
        }

        #region 预定义音效播放方法

        /// <summary>
        /// 播放合成成功音效
        /// </summary>
        public void PlayCraftSuccess()
        {
            PlaySFX("craft_success");
        }

        /// <summary>
        /// 播放合成失败音效
        /// </summary>
        public void PlayCraftFailure()
        {
            PlaySFX("craft_failure");
        }

        /// <summary>
        /// 播放解锁成功音效
        /// </summary>
        public void PlayUnlockSuccess()
        {
            PlaySFX("unlock_success");
        }

        /// <summary>
        /// 播放探索点击音效
        /// </summary>
        public void PlayExploreClick()
        {
            PlaySFX("explore_click");
        }

        /// <summary>
        /// 播放订单完成音效
        /// </summary>
        public void PlayOrderComplete()
        {
            PlaySFX("order_complete");
        }

        /// <summary>
        /// 播放满格提示音效
        /// </summary>
        public void PlayGridFull()
        {
            PlaySFX("grid_full");
        }

        /// <summary>
        /// 播放按钮点击音效
        /// </summary>
        public void PlayButtonClick()
        {
            PlaySFX("button_click");
        }

        /// <summary>
        /// 播放成就解锁音效
        /// </summary>
        public void PlayAchievementUnlock()
        {
            PlaySFX("achievement_unlock");
        }

        #endregion
    }
}