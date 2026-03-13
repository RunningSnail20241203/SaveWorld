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
            if (panelRect == null)
            {
                CreatePanel();
            }

            CreateUIComponents();
            SetupButtonEvents();
            LoadSettings();

            // 初始隐藏
            Hide();

            Debug.Log("[SettingsPanel] 设置面板初始化完成");
        }

        /// <summary>
        /// 创建面板
        /// </summary>
        private void CreatePanel()
        {
            panelRect = GetComponent<RectTransform>();
            if (panelRect == null)
            {
                panelRect = gameObject.AddComponent<RectTransform>();
            }

            // 设置面板位置和大小（屏幕中央）
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = panelSize;

            // 添加背景
            backgroundImage = gameObject.AddComponent<Image>();
            backgroundImage.color = backgroundColor;

            // 添加垂直布局
            VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.UpperCenter;
        }

        /// <summary>
        /// 创建UI组件
        /// </summary>
        private void CreateUIComponents()
        {
            // 标题
            if (titleText == null)
            {
                GameObject titleGO = new GameObject("Title");
                titleGO.transform.SetParent(transform, false);

                titleText = titleGO.AddComponent<TextMeshProUGUI>();
                titleText.text = "游戏设置";
                titleText.fontSize = 24;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = Color.white;

                RectTransform titleRect = titleGO.GetComponent<RectTransform>();
                titleRect.sizeDelta = new Vector2(panelSize.x - 40, 40);
            }

            // 关闭按钮
            if (closeButton == null)
            {
                GameObject closeGO = new GameObject("CloseButton");
                closeGO.transform.SetParent(transform, false);

                closeButton = closeGO.AddComponent<Button>();
                closeButton.onClick.AddListener(Hide);

                // 设置关闭按钮样式
                Image closeImage = closeGO.AddComponent<Image>();
                closeImage.color = new Color(0.8f, 0.2f, 0.2f);

                // 关闭按钮文本
                GameObject closeTextGO = new GameObject("Text");
                closeTextGO.transform.SetParent(closeGO.transform, false);

                TextMeshProUGUI closeText = closeTextGO.AddComponent<TextMeshProUGUI>();
                closeText.text = "×";
                closeText.fontSize = 24;
                closeText.alignment = TextAlignmentOptions.Center;
                closeText.color = Color.white;

                // 设置关闭按钮位置（右上角）
                RectTransform closeRect = closeGO.GetComponent<RectTransform>();
                closeRect.anchorMin = new Vector2(1f, 1f);
                closeRect.anchorMax = new Vector2(1f, 1f);
                closeRect.pivot = new Vector2(1f, 1f);
                closeRect.anchoredPosition = new Vector2(-10, -10);
                closeRect.sizeDelta = new Vector2(40, 40);

                RectTransform closeTextRect = closeTextGO.GetComponent<RectTransform>();
                closeTextRect.anchorMin = Vector2.zero;
                closeTextRect.anchorMax = Vector2.one;
                closeTextRect.offsetMin = Vector2.zero;
                closeTextRect.offsetMax = Vector2.zero;
            }

            // 音量设置区域
            CreateVolumeSettings();

            // 显示设置区域
            CreateDisplaySettings();

            // 游戏设置区域
            CreateGameSettings();

            // 底部按钮区域
            CreateBottomButtons();
        }

        /// <summary>
        /// 创建音量设置
        /// </summary>
        private void CreateVolumeSettings()
        {
            // 音量设置容器
            GameObject volumeContainer = new GameObject("VolumeSettings");
            volumeContainer.transform.SetParent(transform, false);

            VerticalLayoutGroup volumeLayout = volumeContainer.AddComponent<VerticalLayoutGroup>();
            volumeLayout.spacing = 10;
            volumeLayout.childAlignment = TextAnchor.UpperLeft;

            RectTransform volumeRect = volumeContainer.GetComponent<RectTransform>();
            volumeRect.sizeDelta = new Vector2(panelSize.x - 40, 120);

            // 音量标题
            GameObject volumeTitleGO = new GameObject("VolumeTitle");
            volumeTitleGO.transform.SetParent(volumeContainer.transform, false);

            TextMeshProUGUI volumeTitle = volumeTitleGO.AddComponent<TextMeshProUGUI>();
            volumeTitle.text = "音量设置";
            volumeTitle.fontSize = 18;
            volumeTitle.color = Color.white;

            RectTransform volumeTitleRect = volumeTitleGO.GetComponent<RectTransform>();
            volumeTitleRect.sizeDelta = new Vector2(panelSize.x - 40, 25);

            // 主音量
            CreateSliderWithText(volumeContainer, "MasterVolume", "主音量", ref masterVolumeSlider, masterVolumeText);

            // 音乐音量
            CreateSliderWithText(volumeContainer, "MusicVolume", "音乐", ref musicVolumeSlider, musicVolumeText);

            // 音效音量
            CreateSliderWithText(volumeContainer, "SFXVolume", "音效", ref sfxVolumeSlider, sfxVolumeText);
        }

        /// <summary>
        /// 创建显示设置
        /// </summary>
        private void CreateDisplaySettings()
        {
            // 显示设置容器
            GameObject displayContainer = new GameObject("DisplaySettings");
            displayContainer.transform.SetParent(transform, false);

            VerticalLayoutGroup displayLayout = displayContainer.AddComponent<VerticalLayoutGroup>();
            displayLayout.spacing = 10;
            displayLayout.childAlignment = TextAnchor.UpperLeft;

            RectTransform displayRect = displayContainer.GetComponent<RectTransform>();
            displayRect.sizeDelta = new Vector2(panelSize.x - 40, 80);

            // 显示标题
            GameObject displayTitleGO = new GameObject("DisplayTitle");
            displayTitleGO.transform.SetParent(displayContainer.transform, false);

            TextMeshProUGUI displayTitle = displayTitleGO.AddComponent<TextMeshProUGUI>();
            displayTitle.text = "显示设置";
            displayTitle.fontSize = 18;
            displayTitle.color = Color.white;

            RectTransform displayTitleRect = displayTitleGO.GetComponent<RectTransform>();
            displayTitleRect.sizeDelta = new Vector2(panelSize.x - 40, 25);

            // 全屏切换
            CreateToggleWithText(displayContainer, "Fullscreen", "全屏模式", ref fullscreenToggle, ref fullscreenText);

            // 垂直同步
            CreateToggleWithText(displayContainer, "VSync", "垂直同步", ref vsyncToggle, ref vsyncText);
        }

        /// <summary>
        /// 创建游戏设置
        /// </summary>
        private void CreateGameSettings()
        {
            // 游戏设置容器
            GameObject gameContainer = new GameObject("GameSettings");
            gameContainer.transform.SetParent(transform, false);

            VerticalLayoutGroup gameLayout = gameContainer.AddComponent<VerticalLayoutGroup>();
            gameLayout.spacing = 10;
            gameLayout.childAlignment = TextAnchor.UpperLeft;

            RectTransform gameRect = gameContainer.GetComponent<RectTransform>();
            gameRect.sizeDelta = new Vector2(panelSize.x - 40, 80);

            // 游戏标题
            GameObject gameTitleGO = new GameObject("GameTitle");
            gameTitleGO.transform.SetParent(gameContainer.transform, false);

            TextMeshProUGUI gameTitle = gameTitleGO.AddComponent<TextMeshProUGUI>();
            gameTitle.text = "游戏设置";
            gameTitle.fontSize = 18;
            gameTitle.color = Color.white;

            RectTransform gameTitleRect = gameTitleGO.GetComponent<RectTransform>();
            gameTitleRect.sizeDelta = new Vector2(panelSize.x - 40, 25);

            // 自动保存
            CreateToggleWithText(gameContainer, "AutoSave", "自动保存", ref autoSaveToggle, ref autoSaveText);

            // 显示提示
            CreateToggleWithText(gameContainer, "ShowTips", "显示提示", ref showTipsToggle, ref showTipsText);
        }

        /// <summary>
        /// 创建底部按钮
        /// </summary>
        private void CreateBottomButtons()
        {
            // 按钮容器
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(transform, false);

            HorizontalLayoutGroup buttonLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 20;
            buttonLayout.childAlignment = TextAnchor.MiddleCenter;

            RectTransform buttonRect = buttonContainer.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(panelSize.x - 40, 50);

            // 重置按钮
            if (resetButton == null)
            {
                resetButton = CreateButton(buttonContainer, "ResetButton", "重置", OnResetButtonClicked);
            }

            // 应用按钮
            if (applyButton == null)
            {
                applyButton = CreateButton(buttonContainer, "ApplyButton", "应用", OnApplyButtonClicked);
            }
        }

        /// <summary>
        /// 创建带文本的滑块
        /// </summary>
        private void CreateSliderWithText(GameObject parent, string name, string label, ref Slider slider, TextMeshProUGUI text)
        {
            GameObject container = new GameObject(name + "Container");
            container.transform.SetParent(parent.transform, false);

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(panelSize.x - 40, 30);

            // 标签
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(container.transform, false);

            TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 14;
            labelText.color = Color.white;

            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(80, 30);

            // 滑块
            GameObject sliderGO = new GameObject("Slider");
            sliderGO.transform.SetParent(container.transform, false);

            slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;

            RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(150, 20);

            // 数值文本
            GameObject textGO = new GameObject("Value");
            textGO.transform.SetParent(container.transform, false);

            text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "50%";
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Right;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(40, 30);

            // 设置滑块事件
            slider.onValueChanged.AddListener((value) => UpdateSliderText(text, value));
        }

        /// <summary>
        /// 创建带文本的切换开关
        /// </summary>
        private void CreateToggleWithText(GameObject parent, string name, string label, ref Toggle toggle, ref TextMeshProUGUI text)
        {
            GameObject container = new GameObject(name + "Container");
            container.transform.SetParent(parent.transform, false);

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(panelSize.x - 40, 30);

            // 切换开关
            GameObject toggleGO = new GameObject("Toggle");
            toggleGO.transform.SetParent(container.transform, false);

            toggle = toggleGO.AddComponent<Toggle>();

            RectTransform toggleRect = toggleGO.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(30, 30);

            // 标签
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(container.transform, false);

            text = labelGO.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 14;
            text.color = Color.white;

            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(200, 30);
        }

        /// <summary>
        /// 创建按钮
        /// </summary>
        private Button CreateButton(GameObject parent, string name, string text, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent.transform, false);

            Button button = buttonGO.AddComponent<Button>();
            button.onClick.AddListener(action);

            // 设置按钮样式
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.6f, 0.3f);

            // 按钮文本
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 16;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;

            // 设置按钮大小
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100, 40);

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            // 滑块事件已在CreateSliderWithText中设置
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