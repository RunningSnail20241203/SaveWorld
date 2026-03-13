using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;

namespace TestWebGL.Editor
{
    /// <summary>
    /// UI预制件创建工具
    /// 在Unity Editor中运行以自动创建基础UI预制件
    /// </summary>
    public class UIPrefabCreator
    {
        private const string PREFAB_PATH = "Assets/Prefabs/UI";
        private const string RESOURCE_PATH = "Assets/Resources";

        [MenuItem("Tools/TestWebGL/Create UI Prefabs")]
        public static void CreateAllUIPrefabs()
        {
            // 确保文件夹存在
            EnsureDirectoriesExist();

            // 创建各个UI预制件
            CreateGridUIPrefab();
            CreateGridCellUIPrefab();
            CreatePlayerInfoPanelPrefab();
            CreateControlPanelPrefab();
            CreateItemDetailPopupPrefab();
            CreateSettingsPanelPrefab();
            CreateOrdersPanelPrefab();
            CreateAchievementPanelPrefab();

            Debug.Log("UI预制件创建完成！");
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/TestWebGL/Create Resource Folders")]
        public static void CreateResourceFolders()
        {
            EnsureDirectoriesExist();
            Debug.Log("资源文件夹创建完成！");
        }

        private static void EnsureDirectoriesExist()
        {
            // 创建预制件文件夹
            if (!Directory.Exists(PREFAB_PATH))
                Directory.CreateDirectory(PREFAB_PATH);

            // 创建资源文件夹
            string[] resourceFolders = {
                $"{RESOURCE_PATH}/Icons/Items",
                $"{RESOURCE_PATH}/UI",
                $"{RESOURCE_PATH}/Audio",
                $"{RESOURCE_PATH}/Fonts"
            };

            foreach (var folder in resourceFolders)
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
        }

        private static void CreateGridUIPrefab()
        {
            GameObject gridUI = new GameObject("GridUI");
            gridUI.AddComponent<CanvasRenderer>();

            // 添加背景
            Image background = gridUI.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // 添加网格容器
            GameObject container = new GameObject("GridContainer");
            container.transform.SetParent(gridUI.transform);
            GridLayoutGroup gridLayout = container.AddComponent<GridLayoutGroup>();
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 7; // 7列
            gridLayout.cellSize = new Vector2(60, 60);
            gridLayout.spacing = new Vector2(2, 2);

            // 设置RectTransform
            RectTransform rect = gridUI.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(450, 570); // 7*60 + 6*2 = 450, 9*60 + 8*2 = 570

            // 添加脚本组件
            gridUI.AddComponent<TestWebGL.Game.UI.GridUI>();

            // 创建预制件
            PrefabUtility.SaveAsPrefabAsset(gridUI, $"{PREFAB_PATH}/GridUI.prefab");
            Object.DestroyImmediate(gridUI);
        }

        private static void CreateGridCellUIPrefab()
        {
            GameObject cell = new GameObject("GridCellUI");

            // 背景
            Image background = cell.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // 物品图标
            GameObject iconGO = new GameObject("ItemIcon");
            iconGO.transform.SetParent(cell.transform);
            Image icon = iconGO.AddComponent<Image>();
            icon.color = Color.white;

            // 物品数量文本
            GameObject countGO = new GameObject("ItemCount");
            countGO.transform.SetParent(cell.transform);
            TextMeshProUGUI countText = countGO.AddComponent<TextMeshProUGUI>();
            countText.font = GetDefaultFontAsset();
            countText.text = "";
            countText.fontSize = 12;
            countText.color = Color.white;
            countText.alignment = TextAlignmentOptions.BottomRight;

            // 设置RectTransforms
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            cellRect.sizeDelta = new Vector2(60, 60);

            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(5, 5);
            iconRect.offsetMax = new Vector2(-5, -5);

            RectTransform countRect = countGO.GetComponent<RectTransform>();
            countRect.anchorMin = Vector2.zero;
            countRect.anchorMax = Vector2.one;
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;

            // 添加脚本
            cell.AddComponent<TestWebGL.Game.UI.GridCellUI>();

            PrefabUtility.SaveAsPrefabAsset(cell, $"{PREFAB_PATH}/GridCellUI.prefab");
            Object.DestroyImmediate(cell);
        }

        private static void CreatePlayerInfoPanelPrefab()
        {
            GameObject panel = CreateBasePanel("PlayerInfoPanel", new Vector2(300, 200));

            // 标题
            GameObject titleGO = CreateTextElement("PlayerName", "玩家信息", panel.transform);
            titleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);

            // 等级文本
            GameObject levelGO = CreateTextElement("LevelText", "Lv. 1", panel.transform);
            levelGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);

            // 经验条
            GameObject expBar = CreateSlider("ExperienceBar", panel.transform);
            expBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            // 体力条
            GameObject staminaBar = CreateSlider("StaminaBar", panel.transform);
            staminaBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);

            panel.AddComponent<TestWebGL.Game.UI.PlayerInfoPanel>();

            PrefabUtility.SaveAsPrefabAsset(panel, $"{PREFAB_PATH}/PlayerInfoPanel.prefab");
            Object.DestroyImmediate(panel);
        }

        private static void CreateControlPanelPrefab()
        {
            GameObject panel = CreateBasePanel("ControlPanel", new Vector2(400, 80));

            // 按钮容器
            GameObject container = new GameObject("ButtonContainer");
            container.transform.SetParent(panel.transform);
            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = new Vector2(10, 10);
            containerRect.offsetMax = new Vector2(-10, -10);

            // 创建按钮
            CreateButton("ExploreButton", "探索", container.transform);
            CreateButton("SettingsButton", "设置", container.transform);
            CreateButton("OrdersButton", "订单", container.transform);
            CreateButton("AchievementsButton", "成就", container.transform);
            CreateButton("SaveButton", "保存", container.transform);

            panel.AddComponent<TestWebGL.Game.UI.ControlPanel>();

            PrefabUtility.SaveAsPrefabAsset(panel, $"{PREFAB_PATH}/ControlPanel.prefab");
            Object.DestroyImmediate(panel);
        }

        private static void CreateItemDetailPopupPrefab()
        {
            GameObject popup = CreateBasePanel("ItemDetailPopup", new Vector2(350, 400));

            // 物品图标
            GameObject iconGO = new GameObject("ItemIcon");
            iconGO.transform.SetParent(popup.transform);
            Image icon = iconGO.AddComponent<Image>();
            icon.color = Color.white;

            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(80, 80);
            iconRect.anchoredPosition = new Vector2(0, 120);

            // 物品名称
            GameObject nameGO = CreateTextElement("ItemName", "物品名称", popup.transform);
            nameGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 60);

            // 物品描述
            GameObject descGO = CreateTextElement("ItemDescription", "物品描述", popup.transform);
            descGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            descGO.GetComponent<TextMeshProUGUI>().fontSize = 14;

            // 操作按钮
            GameObject actionBtn = CreateButton("ActionButton", "操作", popup.transform);
            actionBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);

            // 关闭按钮
            GameObject closeBtn = CreateButton("CloseButton", "关闭", popup.transform);
            closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -120);

            popup.AddComponent<TestWebGL.Game.UI.ItemDetailPopup>();

            PrefabUtility.SaveAsPrefabAsset(popup, $"{PREFAB_PATH}/ItemDetailPopup.prefab");
            Object.DestroyImmediate(popup);
        }

        private static void CreateSettingsPanelPrefab()
        {
            GameObject panel = CreateBasePanel("SettingsPanel", new Vector2(400, 500));

            // 标题
            GameObject titleGO = CreateTextElement("Title", "游戏设置", panel.transform);
            titleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 220);

            // 设置容器
            GameObject container = new GameObject("SettingsContainer", typeof(RectTransform));
            container.transform.SetParent(panel.transform, false);

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(350, 350);
            containerRect.anchoredPosition = new Vector2(0, 20);

            // 应用按钮
            GameObject applyBtn = CreateButton("ApplyButton", "应用", panel.transform);
            applyBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80, -200);

            // 重置按钮
            GameObject resetBtn = CreateButton("ResetButton", "重置", panel.transform);
            resetBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);

            // 关闭按钮
            GameObject closeBtn = CreateButton("CloseButton", "关闭", panel.transform);
            closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(80, -200);

            panel.AddComponent<TestWebGL.Game.UI.SettingsPanel>();

            PrefabUtility.SaveAsPrefabAsset(panel, $"{PREFAB_PATH}/SettingsPanel.prefab");
            Object.DestroyImmediate(panel);
        }

        private static void CreateOrdersPanelPrefab()
        {
            GameObject panel = CreateBasePanel("OrdersPanel", new Vector2(400, 500));

            // 标题
            GameObject titleGO = CreateTextElement("Title", "订单面板", panel.transform);
            titleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 220);

            // 订单容器
            GameObject container = new GameObject("OrdersContainer", typeof(RectTransform));
            container.transform.SetParent(panel.transform, false);

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(350, 350);
            containerRect.anchoredPosition = new Vector2(0, 20);

            // 刷新按钮
            GameObject refreshBtn = CreateButton("RefreshButton", "刷新", panel.transform);
            refreshBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80, -200);

            // 关闭按钮
            GameObject closeBtn = CreateButton("CloseButton", "关闭", panel.transform);
            closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(80, -200);

            panel.AddComponent<TestWebGL.Game.UI.OrdersPanel>();

            PrefabUtility.SaveAsPrefabAsset(panel, $"{PREFAB_PATH}/OrdersPanel.prefab");
            Object.DestroyImmediate(panel);
        }

        private static void CreateAchievementPanelPrefab()
        {
            GameObject panel = CreateBasePanel("AchievementPanel", new Vector2(400, 500));

            // 标题
            GameObject titleGO = CreateTextElement("Title", "成就面板", panel.transform);
            titleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 220);

            // 进度文本
            GameObject progressGO = CreateTextElement("ProgressText", "成就进度: 0/8", panel.transform);
            progressGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 180);

            // 成就容器
            GameObject container = new GameObject("AchievementsContainer", typeof(RectTransform));
            container.transform.SetParent(panel.transform, false);

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(350, 300);
            containerRect.anchoredPosition = new Vector2(0, 20);

            // 关闭按钮
            GameObject closeBtn = CreateButton("CloseButton", "关闭", panel.transform);
            closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);

            panel.AddComponent<TestWebGL.Game.UI.AchievementPanel>();

            PrefabUtility.SaveAsPrefabAsset(panel, $"{PREFAB_PATH}/AchievementPanel.prefab");
            Object.DestroyImmediate(panel);
        }

        // 辅助方法
        private static GameObject CreateBasePanel(string name, Vector2 size)
        {
            GameObject panel = new GameObject(name);

            // 背景
            Image background = panel.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // RectTransform
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;

            return panel;
        }

        private static GameObject CreateTextElement(string name, string text, Transform parent)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent);

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.font = GetDefaultFontAsset();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            RectTransform rect = textGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 30);

            return textGO;
        }

        private static GameObject CreateButton(string name, string text, Transform parent)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent);

            // Button组件
            Button button = buttonGO.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
            button.colors = colors;

            // 背景图片
            Image background = buttonGO.AddComponent<Image>();
            background.color = colors.normalColor;

            // 文本
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.font = GetDefaultFontAsset();
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            // 设置RectTransforms
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(70, 50);

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonGO;
        }

        private static TMP_FontAsset GetDefaultFontAsset()
        {
            const string defaultFontPath = "Assets/Resources/Fonts/AlibabaPuHuiTi-3-105-Heavy SDF.asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(defaultFontPath);
            if (font != null)
                return font;

            font = TMPro.TMP_Settings.defaultFontAsset;
            if (font != null)
                return font;

            Debug.LogWarning($"[UIPrefabCreator] 未找到字体资产 '{defaultFontPath}'，使用 TMP 默认字体。");
            return null;
        }

        private static GameObject CreateSlider(string name, Transform parent)
        {
            GameObject sliderGO = new GameObject(name);
            sliderGO.transform.SetParent(parent);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 50;

            // 背景
            GameObject backgroundGO = new GameObject("Background");
            backgroundGO.transform.SetParent(sliderGO.transform);
            Image bgImage = backgroundGO.AddComponent<Image>();
            bgImage.color = Color.gray;

            // 填充
            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(backgroundGO.transform);
            Image fillImage = fillGO.AddComponent<Image>();
            fillImage.color = Color.green;

            // 滑块
            GameObject handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(sliderGO.transform);
            Image handleImage = handleGO.AddComponent<Image>();
            handleImage.color = Color.white;

            // 设置Slider引用
            slider.targetGraphic = handleImage;
            slider.fillRect = fillGO.GetComponent<RectTransform>();
            slider.handleRect = handleGO.GetComponent<RectTransform>();

            // 设置RectTransforms
            RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(200, 20);

            RectTransform bgRect = backgroundGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            RectTransform fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            RectTransform handleRect = handleGO.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);
            handleRect.anchoredPosition = new Vector2(slider.value / slider.maxValue * 200 - 100, 0);

            return sliderGO;
        }
    }
}