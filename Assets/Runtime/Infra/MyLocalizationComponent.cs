using GameFramework;
using GameFramework.Localization;
using GameFramework.Resource;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;
using UnityGameFramework.Runtime;

namespace SaveWorld
{
    /// <summary>
    /// 本地化组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("SaveWorld Framework/MyLocalization")]
    public sealed class MyLocalizationComponent : GameFrameworkComponent
    {
        private ILocalizationManager m_LocalizationManager;

        [SerializeField]
        private string mLocalizationHelperTypeName = "UnityGameFramework.Runtime.DefaultLocalizationHelper";

        [SerializeField] private LocalizationHelperBase mCustomLocalizationHelper;

        /// <summary>
        /// 获取或设置本地化语言。
        /// </summary>
        public Language Language
        {
            get => m_LocalizationManager.Language;
            set => SetLocalizationLocale(value);
        }

        /// <summary>
        /// 获取系统语言。
        /// </summary>
        public Language SystemLanguage
        {
            get { return m_LocalizationManager.SystemLanguage; }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_LocalizationManager = GameFrameworkEntry.GetModule<ILocalizationManager>();
            if (m_LocalizationManager == null)
            {
                Log.Fatal("Localization manager is invalid.");
            }
        }

        private void Start()
        {
            var baseComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_LocalizationManager.SetResourceManager(baseComponent.EditorResourceMode
                ? baseComponent.EditorResourceHelper
                : GameFrameworkEntry.GetModule<IResourceManager>());

            var localizationHelper = Helper.CreateHelper(mLocalizationHelperTypeName, mCustomLocalizationHelper);
            if (localizationHelper == null)
            {
                Log.Error("Can not create localization helper.");
                return;
            }

            localizationHelper.name = "Localization Helper";
            var helperTransform = localizationHelper.transform;
            helperTransform.SetParent(transform);
            helperTransform.localScale = Vector3.one;

            m_LocalizationManager.SetDataProviderHelper(localizationHelper);
            m_LocalizationManager.SetLocalizationHelper(localizationHelper);

            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        public string GetString(string key)
        {
            return m_LocalizationManager.GetString(key);
        }

        public LocalizedString CreateLocalizedString(string table, string key)
        {
            return new LocalizedString(table, key);
        }

        private void SetLocalizationLocale(Language language)
        {
            if (language == Language.Unspecified) language = Language.ChineseSimplified;

            m_LocalizationManager.Language = language;

            var availableLocales = LocalizationSettings.AvailableLocales.Locales;
            var locale = availableLocales.Find(x => x.Identifier.Code == GetLanguageCode(language));
            LocalizationSettings.SelectedLocale = locale;
        }

        // 语言代码映射
        private string GetLanguageCode(Language language)
        {
            return language switch
            {
                Language.ChineseSimplified => "zh",
                Language.English => "en",
                _ => "zh"
            };
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            Log.Info($"Selected locale changed to '{locale.Identifier.Code}'." );
        }
    }
}