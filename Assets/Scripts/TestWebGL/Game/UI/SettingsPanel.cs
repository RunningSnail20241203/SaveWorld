using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// 设置面板 - 游戏设置和选项
    /// 包括音量控制、显示设置、游戏选项等
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("UI组件")]
        public RectTransform panelRect;
        public Image backgroundImage;
        public TextMeshProUGUI titleText;
        public Button closeButton;

        [Header("音量设置")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public TextMeshProUGUI masterVolumeText;
        public TextMeshProUGUI musicVolumeText;
        public TextMeshProUGUI sfxVolumeText;

        [Header("显示设置")]
        public Toggle fullscreenToggle;
        public TextMeshProUGUI fullscreenText;
        public Toggle vsyncToggle;
        public TextMeshProUGUI vsyncText;

        [Header("游戏设置")]
        public Toggle autoSaveToggle;
        public TextMeshProUGUI autoSaveText;
        public Toggle showTipsToggle;
        public TextMeshProUGUI showTipsText;

        [Header("按钮")]
        public Button resetButton;
        public Button applyButton;

        [Header("面板设置")]
        public Vector2 panelSize = new Vector2(400, 500);
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        private bool _isVisible = false;

        /// <summary>
        /// 初始化设置面板
        /// </summary>
        public void Initialize()
        {
            // 从预制件获取RectTransform
            if (panelRect == null)
            {
                panelRect = GetComponent<RectTransform>();
            }

            // 从预制件获取UI组件引用
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            if (titleText == null)
                titleText = transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (closeButton == null)
                closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
            if (masterVolumeSlider == null)
                masterVolumeSlider = transform.Find("VolumeSettings/MasterVolumeContainer/Slider")?.GetComponent<Slider>();
            if (musicVolumeSlider == null)
                musicVolumeSlider = transform.Find("VolumeSettings/MusicVolumeContainer/Slider")?.GetComponent<Slider>();
            if (sfxVolumeSlider == null)
                sfxVolumeSlider = transform.Find("VolumeSettings/SFXVolumeContainer/Slider")?.GetComponent<Slider>();
            if (masterVolumeText == null)
                masterVolumeText = transform.Find("VolumeSettings/MasterVolumeContainer/Value")?.GetComponent<TextMeshProUGUI>();
            if (musicVolumeText == null)
                musicVolumeText = transform.Find("VolumeSettings/MusicVolumeContainer/Value")?.GetComponent<TextMeshProUGUI>();
            if (sfxVolumeText == null)
                sfxVolumeText = transform.Find("VolumeSettings/SFXVolumeContainer/Value")?.GetComponent<TextMeshProUGUI>();
            if (fullscreenToggle == null)
                fullscreenToggle = transform.Find("DisplaySettings/FullscreenContainer/Toggle")?.GetComponent<Toggle>();
            if (vsyncToggle == null)
                vsyncToggle = transform.Find("DisplaySettings/VSyncContainer/Toggle")?.GetComponent<Toggle>();
            if (autoSaveToggle == null)
                autoSaveToggle = transform.Find("GameSettings/AutoSaveContainer/Toggle")?.GetComponent<Toggle>();
            if (showTipsToggle == null)
                showTipsToggle = transform.Find("GameSettings/ShowTipsContainer/Toggle")?.GetComponent<Toggle>();
            if (resetButton == null)
                resetButton = transform.Find("ButtonContainer/ResetButton")?.GetComponent<Button>();
            if (applyButton == null)
                applyButton = transform.Find("ButtonContainer/ApplyButton")?.GetComponent<Button>();

            // 设置按钮事件
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetButtonClicked);
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyButtonClicked);

            // 设置滑块事件
            if (masterVolumeSlider != null && masterVolumeText != null)
                masterVolumeSlider.onValueChanged.AddListener((value) => UpdateSliderText(masterVolumeText, value));
            if (musicVolumeSlider != null && musicVolumeText != null)
                musicVolumeSlider.onValueChanged.AddListener((value) => UpdateSliderText(musicVolumeText, value));
            if (sfxVolumeSlider != null && sfxVolumeText != null)
                sfxVolumeSlider.onValueChanged.AddListener((value) => UpdateSliderText(sfxVolumeText, value));

            LoadSettings();

            // 初始隐藏
            Hide();

            Debug.Log("[SettingsPanel] 设置面板初始化完成");
        }

        /// <summary>
        /// 更新滑块文本
        /// </summary>
        private void UpdateSliderText(TextMeshProUGUI text, float value)
        {
            if (text != null)
            {
                text.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        /// <summary>
        /// 显示设置面板
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            _isVisible = true;
        }

        /// <summary>
        /// 隐藏设置面板
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _isVisible = false;
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            // 从PlayerPrefs加载设置，如果没有则使用默认值
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.9f);

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
            if (vsyncToggle != null)
                vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;

            if (autoSaveToggle != null)
                autoSaveToggle.isOn = PlayerPrefs.GetInt("AutoSave", 1) == 1;
            if (showTipsToggle != null)
                showTipsToggle.isOn = PlayerPrefs.GetInt("ShowTips", 1) == 1;

            UpdateAllSliderTexts();
        }

        /// <summary>
        /// 更新所有滑块文本
        /// </summary>
        private void UpdateAllSliderTexts()
        {
            if (masterVolumeSlider != null && masterVolumeText != null)
                UpdateSliderText(masterVolumeText, masterVolumeSlider.value);
            if (musicVolumeSlider != null && musicVolumeText != null)
                UpdateSliderText(musicVolumeText, musicVolumeSlider.value);
            if (sfxVolumeSlider != null && sfxVolumeText != null)
                UpdateSliderText(sfxVolumeText, sfxVolumeSlider.value);
        }

        /// <summary>
        /// 重置按钮点击事件
        /// </summary>
        private void OnResetButtonClicked()
        {
            LoadSettings();
            Debug.Log("[SettingsPanel] 设置已重置为默认值");
        }

        /// <summary>
        /// 应用按钮点击事件
        /// </summary>
        private void OnApplyButtonClicked()
        {
            SaveSettings();
            ApplySettings();
            Hide();
            Debug.Log("[SettingsPanel] 设置已应用");
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        private void SaveSettings()
        {
            // 保存到PlayerPrefs
            if (masterVolumeSlider != null)
                PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            if (musicVolumeSlider != null)
                PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            if (sfxVolumeSlider != null)
                PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);

            if (fullscreenToggle != null)
                PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
            if (vsyncToggle != null)
                PlayerPrefs.SetInt("VSync", vsyncToggle.isOn ? 1 : 0);

            if (autoSaveToggle != null)
                PlayerPrefs.SetInt("AutoSave", autoSaveToggle.isOn ? 1 : 0);
            if (showTipsToggle != null)
                PlayerPrefs.SetInt("ShowTips", showTipsToggle.isOn ? 1 : 0);

            PlayerPrefs.Save(); // 确保保存到磁盘

            PlayerPrefs.Save();
        }

        /// <summary>
        /// 应用设置
        /// </summary>
        private void ApplySettings()
        {
            // 音量设置
            if (masterVolumeSlider != null)
                AudioListener.volume = masterVolumeSlider.value;

            // 显示设置
            if (fullscreenToggle != null)
                Screen.fullScreen = fullscreenToggle.isOn;
            if (vsyncToggle != null)
                QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;

            // 游戏设置
            // 注意：这些设置可能需要在GameManager中实现相应的方法
            // GameManager.Instance.SetAutoSave(autoSaveToggle.isOn);
            // GameManager.Instance.SetShowTips(showTipsToggle.isOn);

            Debug.Log("[SettingsPanel] 设置已应用到游戏系统");
        }

        public bool IsVisible => _isVisible;
    }
}