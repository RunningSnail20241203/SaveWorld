using System;
using UnityEngine;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.Audio
{
    /// <summary>
    /// 音频管理器
    /// 负责背景音乐与音效播放、音量控制
    /// </summary>
    public class AudioManager
    {
        private readonly EventBus _eventBus;
        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private AudioClip[] _audioClips;

        private float _masterVolume = 1.0f;
        private float _musicVolume = 0.8f;
        private float _sfxVolume = 1.0f;
        private bool _musicEnabled = true;
        private bool _sfxEnabled = true;

        public AudioManager(EventBus eventBus)
        {
            _eventBus = eventBus;
            
            InitializeAudioSources();
            RegisterEventHandlers();
            LoadAudioClips();
        }

        private void InitializeAudioSources()
        {
            var musicObject = new GameObject("MusicSource");
            _musicSource = musicObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;

            var sfxObject = new GameObject("SFXSource");
            _sfxSource = sfxObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;

            UpdateVolumes();
        }

        private void RegisterEventHandlers()
        {
            // 监听游戏事件播放对应音效
            _eventBus.Listen<MergeCompleteEvent>(_ => PlaySFX(SoundType.CraftSuccess));
            _eventBus.Listen<ItemMovedEvent>(_ => PlaySFX(SoundType.ItemMove));
            _eventBus.Listen<ExplorationCompleteEvent>(_ => PlaySFX(SoundType.Explore));
            _eventBus.Listen<OrderSubmittedEvent>(_ => PlaySFX(SoundType.OrderComplete));
            _eventBus.Listen<AchievementUnlockedEvent>(_ => PlaySFX(SoundType.AchievementUnlock));
            _eventBus.Listen<LevelUpEvent>(_ => PlaySFX(SoundType.LevelUp));
            
            // 音量控制事件
            _eventBus.Listen<SetMasterVolumeEvent>(e => SetMasterVolume(e.Volume));
            _eventBus.Listen<SetMusicVolumeEvent>(e => SetMusicVolume(e.Volume));
            _eventBus.Listen<SetSFXVolumeEvent>(e => SetSFXVolume(e.Volume));
            _eventBus.Listen<ToggleMusicEvent>(e => ToggleMusic(e.Enabled));
            _eventBus.Listen<ToggleSFXEvent>(e => ToggleSFX(e.Enabled));
        }

        private void LoadAudioClips()
        {
            // TODO: 从Resources加载实际音频文件
            // 目前为占位实现
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayMusic(MusicType type)
        {
            if (!_musicEnabled) return;
            
            // _musicSource.clip = GetMusicClip(type);
            // _musicSource.Play();
            
            _eventBus.Publish(new MusicPlayedEvent { MusicType = type });
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopMusic()
        {
            _musicSource.Stop();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(SoundType type)
        {
            if (!_sfxEnabled) return;
            
            // var clip = GetSFXClip(type);
            // _sfxSource.PlayOneShot(clip, _sfxVolume * _masterVolume);
            
            _eventBus.Publish(new SFXPlayedEvent { SoundType = type });
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
            _eventBus.Publish(new VolumeChangedEvent { VolumeType = VolumeType.Master, Volume = _masterVolume });
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
            _eventBus.Publish(new VolumeChangedEvent { VolumeType = VolumeType.Music, Volume = _musicVolume });
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
            _eventBus.Publish(new VolumeChangedEvent { VolumeType = VolumeType.SFX, Volume = _sfxVolume });
        }

        public void ToggleMusic(bool enabled)
        {
            _musicEnabled = enabled;
            if (!_musicEnabled)
            {
                StopMusic();
            }
            _eventBus.Publish(new MusicToggledEvent { Enabled = _musicEnabled });
        }

        public void ToggleSFX(bool enabled)
        {
            _sfxEnabled = enabled;
            _eventBus.Publish(new SFXToggledEvent { Enabled = _sfxEnabled });
        }

        private void UpdateVolumes()
        {
            _musicSource.volume = _musicVolume * _masterVolume;
            _sfxSource.volume = _sfxVolume * _masterVolume;
        }
    }

    #region 音频类型定义

    public enum SoundType
    {
        ButtonClick = 1,
        CraftSuccess = 2,
        CraftFailure = 3,
        ItemMove = 4,
        Explore = 5,
        OrderComplete = 6,
        AchievementUnlock = 7,
        LevelUp = 8,
        GridFull = 9,
        Error = 10
    }

    public enum MusicType
    {
        MainTheme = 1,
        Exploration = 2,
        Menu = 3
    }

    public enum VolumeType
    {
        Master,
        Music,
        SFX
    }

    #endregion

    #region 事件定义

    public class SetMasterVolumeEvent : GameEvent
    {
        public float Volume;
    }

    public class SetMusicVolumeEvent : GameEvent
    {
        public float Volume;
    }

    public class SetSFXVolumeEvent : GameEvent
    {
        public float Volume;
    }

    public class ToggleMusicEvent : GameEvent
    {
        public bool Enabled;
    }

    public class ToggleSFXEvent : GameEvent
    {
        public bool Enabled;
    }

    public class VolumeChangedEvent : GameEvent
    {
        public VolumeType VolumeType;
        public float Volume;
    }

    public class MusicPlayedEvent : GameEvent
    {
        public MusicType MusicType;
    }

    public class SFXPlayedEvent : GameEvent
    {
        public SoundType SoundType;
    }

    public class MusicToggledEvent : GameEvent
    {
        public bool Enabled;
    }

    public class SFXToggledEvent : GameEvent
    {
        public bool Enabled;
    }

    #endregion
}