using UnityEngine;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// UI主题配置 - 统一管理所有界面样式、颜色、尺寸
    /// 不需要美术资源即可实现专业美观的界面
    /// </summary>
    public static class UIThemeConfig
    {
        #region 颜色配置
        
        // 主色调 - 末世生存风格
        public static readonly Color BackgroundMain = new Color(0.102f, 0.137f, 0.196f);   // #1A2332
        public static readonly Color BackgroundCard = new Color(0.157f, 0.200f, 0.267f);   // #283344
        public static readonly Color BorderNormal   = new Color(0.247f, 0.298f, 0.369f);   // #3F4C5E
        public static readonly Color BorderHighlight = new Color(1.000f, 0.624f, 0.263f);  // #FF9F43
        public static readonly Color TextPrimary    = new Color(0.910f, 0.918f, 0.929f);   // #E8EAED
        public static readonly Color TextSecondary  = new Color(0.604f, 0.647f, 0.694f);   // #9AA5B1
        public static readonly Color Success        = new Color(0.133f, 0.773f, 0.369f);   // #22C55E
        public static readonly Color Warning        = new Color(0.961f, 0.620f, 0.043f);   // #F59E0B
        public static readonly Color Danger         = new Color(0.937f, 0.267f, 0.278f);   // #EF4444
        public static readonly Color LockedOverlay  = new Color(0.000f, 0.000f, 0.000f, 0.7f);
        
        #endregion

        #region 尺寸配置
        
        // 参考分辨率 (微信小游戏标准)
        public const float ReferenceWidth = 750f;
        public const float ReferenceHeight = 1334f;
        
        // 全局边距
        public const float GlobalMargin = 24f;
        
        // 区域高度占比
        public const float TopBarHeightRatio = 0.12f;
        public const float GridAreaHeightRatio = 0.62f;
        public const float BottomBarHeightRatio = 0.26f;
        
        // 格子配置
        public const int GridColumns = 7;
        public const int GridRows = 9;
        public const float GridCellSize = 92f;
        public const float GridCellSpacing = 8f;
        public const float GridCellRadius = 12f;
        public const float GridCellBorderWidth = 2f;
        
        // 按钮配置
        public const float ButtonHeight = 56f;
        public const float ButtonRadius = 8f;
        
        // 字体大小
        public const int FontSizeTitle = 18;
        public const int FontSizeBody = 14;
        public const int FontSizeNumber = 16;
        public const int FontSizeSmall = 12;
        
        #endregion

        #region 布局辅助方法
        
        /// <summary>
        /// 获取自适应尺寸 (根据当前屏幕分辨率缩放)
        /// </summary>
        public static float GetScaledValue(float baseValue)
        {
            float screenRatio = Screen.width / ReferenceWidth;
            return baseValue * screenRatio;
        }
        
        /// <summary>
        /// 创建纯色精灵 (运行时动态生成)
        /// </summary>
        public static Sprite CreateSolidSprite(Color color, int width = 2, int height = 2)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }
        
        #endregion
    }
}