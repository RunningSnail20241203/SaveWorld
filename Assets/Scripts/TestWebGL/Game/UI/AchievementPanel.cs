using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestWebGL.Game.Achievement;
using System.Collections.Generic;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// 成就面板
    /// 显示所有成就及其解锁状态
    /// </summary>
    public class AchievementPanel : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private GameObject achievementItemPrefab;
        [SerializeField] private Transform achievementListContainer;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button closeButton;

        private bool _isVisible = false;
        private List<GameObject> _achievementItems = new List<GameObject>();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            // 从预制件获取UI组件引用
            if (achievementItemPrefab == null)
                achievementItemPrefab = Resources.Load<GameObject>("Prefabs/UI/AchievementItem");
            if (achievementListContainer == null)
                achievementListContainer = transform.Find("AchievementsContainer");
            if (progressText == null)
                progressText = transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
            if (closeButton == null)
                closeButton = transform.Find("CloseButton")?.GetComponent<Button>();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            // 订阅成就解锁事件
            AchievementSystem.Instance.OnAchievementUnlocked += OnAchievementUnlocked;

            Debug.Log("[AchievementPanel] 初始化完成");
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show()
        {
            if (gameObject == null) return;

            gameObject.SetActive(true);
            _isVisible = true;

            RefreshAchievementList();
            UpdateProgressText();

            Debug.Log("[AchievementPanel] 面板已显示");
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            if (gameObject == null) return;

            gameObject.SetActive(false);
            _isVisible = false;

            Debug.Log("[AchievementPanel] 面板已隐藏");
        }

        /// <summary>
        /// 刷新成就列表
        /// </summary>
        private void RefreshAchievementList()
        {
            // 清理现有项目
            foreach (var item in _achievementItems)
            {
                Destroy(item);
            }
            _achievementItems.Clear();

            // 获取所有成就
            var achievements = AchievementSystem.Instance.GetAllAchievements();

            foreach (var kvp in achievements)
            {
                CreateAchievementItem(kvp.Value);
            }
        }

        /// <summary>
        /// 创建成就项目
        /// </summary>
        private void CreateAchievementItem(AchievementData achievement)
        {
            if (achievementItemPrefab == null || achievementListContainer == null) return;

            GameObject itemObj = Instantiate(achievementItemPrefab, achievementListContainer);
            _achievementItems.Add(itemObj);

            // 设置成就信息
            var titleText = itemObj.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            var descriptionText = itemObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            var progressText = itemObj.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
            var iconImage = itemObj.transform.Find("IconImage")?.GetComponent<Image>();
            var backgroundImage = itemObj.transform.Find("BackgroundImage")?.GetComponent<Image>();

            if (titleText != null)
            {
                titleText.text = achievement.title;
                titleText.color = achievement.isUnlocked ? Color.yellow : Color.gray;
            }

            if (descriptionText != null)
            {
                descriptionText.text = achievement.description;
                descriptionText.color = achievement.isUnlocked ? Color.white : Color.gray;
            }

            if (progressText != null)
            {
                if (achievement.isUnlocked)
                {
                    progressText.text = "已解锁";
                    progressText.color = Color.green;
                }
                else
                {
                    progressText.text = $"{achievement.progress}/{achievement.targetProgress}";
                    progressText.color = Color.white;
                }
            }

            // 设置背景颜色
            if (backgroundImage != null)
            {
                backgroundImage.color = achievement.isUnlocked ?
                    new Color(0.2f, 0.2f, 0.8f, 0.8f) : // 解锁：蓝色
                    new Color(0.3f, 0.3f, 0.3f, 0.8f);  // 未解锁：灰色
            }

            // 设置图标（如果有的话）
            if (iconImage != null)
            {
                // 这里可以根据成就类型设置不同的图标
                // 暂时使用默认颜色
                iconImage.color = achievement.isUnlocked ? Color.yellow : Color.gray;
            }
        }

        /// <summary>
        /// 更新进度文本
        /// </summary>
        private void UpdateProgressText()
        {
            if (progressText != null)
            {
                int unlocked = AchievementSystem.Instance.GetUnlockedCount();
                int total = AchievementSystem.Instance.GetTotalCount();
                progressText.text = $"成就进度: {unlocked}/{total}";
            }
        }

        /// <summary>
        /// 成就解锁事件处理
        /// </summary>
        private void OnAchievementUnlocked(AchievementData achievement)
        {
            Debug.Log($"[AchievementPanel] 成就解锁: {achievement.title}");

            // 如果面板可见，刷新显示
            if (_isVisible)
            {
                RefreshAchievementList();
                UpdateProgressText();
            }

            // 这里可以添加成就解锁动画或音效
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void OnDestroy()
        {
            if (AchievementSystem.Instance != null)
            {
                AchievementSystem.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
            }
        }

        public bool IsVisible => _isVisible;
    }
}