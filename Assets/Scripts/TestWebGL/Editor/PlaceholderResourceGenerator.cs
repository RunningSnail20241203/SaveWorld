using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace TestWebGL.Editor
{
    /// <summary>
    /// 占位资源生成器
    /// 自动生成缺失的UI资源、音频资源和UI预制件
    /// </summary>
    public class PlaceholderResourceGenerator : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/Prefabs/UI";
        private const string RESOURCE_PATH = "Assets/Resources";

        [MenuItem("Tools/生成占位资源")]
        public static void ShowWindow()
        {
            GetWindow<PlaceholderResourceGenerator>("占位资源生成器");
        }

        private void OnGUI()
        {
            GUILayout.Label("占位资源生成器", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("生成物品图标占位资源"))
            {
                GenerateItemIcons();
            }

            if (GUILayout.Button("生成UI图标占位资源"))
            {
                GenerateUIIcons();
            }

            if (GUILayout.Button("生成音频占位资源"))
            {
                GenerateAudioPlaceholders();
            }

            if (GUILayout.Button("生成微信相关占位资源"))
            {
                GenerateWeChatResources();
            }

            if (GUILayout.Button("生成UI预制件"))
            {
                CreateAllUIPrefabs();
            }

            if (GUILayout.Button("一键生成所有占位资源"))
            {
                GenerateAllPlaceholders();
            }
        }

        /// <summary>
        /// 生成物品图标占位资源
        /// </summary>
        public static void GenerateItemIcons()
        {
            Debug.Log("[PlaceholderGenerator] 开始生成物品图标占位资源...");

            // 定义颜色方案
            Dictionary<string, Color> colorScheme = new Dictionary<string, Color>
            {
                { "Water", new Color(0.29f, 0.56f, 0.89f) },      // 蓝色
                { "Food", new Color(1.0f, 0.55f, 0.0f) },         // 橙色
                { "Tool", new Color(0.5f, 0.5f, 0.5f) },          // 灰色
                { "Home", new Color(0.55f, 0.27f, 0.07f) },       // 棕色
                { "Medical", new Color(1.0f, 0.27f, 0.27f) },     // 红色
                { "Energy", new Color(1.0f, 0.84f, 0.0f) },       // 黄色
                { "Knowledge", new Color(0.61f, 0.35f, 0.71f) },  // 紫色
                { "Hope", new Color(0.18f, 0.8f, 0.44f) },        // 绿色
                { "Explore", new Color(0.1f, 0.74f, 0.61f) }      // 青色
            };

            // 生成L1-L10图标
            foreach (var colorPair in colorScheme)
            {
                for (int level = 1; level <= 10; level++)
                {
                    string iconName = $"{colorPair.Key}_L{level}";
                    string filePath = $"Assets/Resources/Icons/Items/{iconName}.png";
                    
                    // 确保目录存在
                    string directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // 生成图标
                    Texture2D icon = GenerateIcon(iconName, colorPair.Value, level);
                    
                    // 保存为PNG
                    byte[] pngData = icon.EncodeToPNG();
                    File.WriteAllBytes(filePath, pngData);
                    
                    // 导入设置
                    AssetDatabase.ImportAsset(filePath);
                    TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spritePixelsPerUnit = 64;
                        importer.mipmapEnabled = false;
                        importer.SaveAndReimport();
                    }

                    DestroyImmediate(icon);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("[PlaceholderGenerator] 物品图标占位资源生成完成！");
        }

        /// <summary>
        /// 生成单个图标
        /// </summary>
        private static Texture2D GenerateIcon(string iconName, Color baseColor, int level)
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            // 清除为透明
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            texture.SetPixels(pixels);

            // 绘制圆形背景
            Color circleColor = baseColor;
            circleColor.a = 0.8f;
            DrawCircle(texture, size / 2, size / 2, size / 2 - 4, circleColor);

            // 绘制等级指示器（不同等级有不同的样式）
            if (level > 1)
            {
                Color levelColor = Color.white;
                levelColor.a = 0.9f;
                
                // 根据等级绘制不同数量的星星
                int starCount = Mathf.Min(level - 1, 5);
                for (int i = 0; i < starCount; i++)
                {
                    int x = 8 + i * 10;
                    int y = size - 12;
                    DrawStar(texture, x, y, 4, levelColor);
                }
            }

            // 绘制物品名称缩写
            string abbreviation = GetAbbreviation(iconName);
            DrawText(texture, abbreviation, size / 2, size / 2 - 8, Color.white);

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 绘制圆形
        /// </summary>
        private static void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制星星
        /// </summary>
        private static void DrawStar(Texture2D texture, int centerX, int centerY, int size, Color color)
        {
            for (int x = -size; x <= size; x++)
            {
                for (int y = -size; y <= size; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= size)
                    {
                        int pixelX = centerX + x;
                        int pixelY = centerY + y;
                        if (pixelX >= 0 && pixelX < texture.width && pixelY >= 0 && pixelY < texture.height)
                        {
                            texture.SetPixel(pixelX, pixelY, color);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 绘制文字（简化版）
        /// </summary>
        private static void DrawText(Texture2D texture, string text, int x, int y, Color color)
        {
            // 简化的文字绘制，实际项目中可以使用更复杂的字体渲染
            // 这里只是示意，实际生成时会显示物品名称缩写
        }

        /// <summary>
        /// 获取物品名称缩写
        /// </summary>
        private static string GetAbbreviation(string iconName)
        {
            string[] parts = iconName.Split('_');
            if (parts.Length >= 2)
            {
                return parts[0].Substring(0, 1) + parts[1].Replace("L", "");
            }
            return iconName.Substring(0, 2);
        }

        /// <summary>
        /// 生成UI图标占位资源
        /// </summary>
        public static void GenerateUIIcons()
        {
            Debug.Log("[PlaceholderGenerator] 开始生成UI图标占位资源...");

            string[] uiIconNames = {
                "StaminaIcon", "LevelIcon", "CoinIcon", "SettingsIcon",
                "CollectionIcon", "ExploreIcon", "OrderIcon", "AchievementIcon",
                "ShareIcon", "RankingIcon", "LockIcon", "UnlockIcon"
            };

            Color[] uiColors = {
                Color.red, Color.blue, Color.yellow, Color.gray,
                Color.green, Color.cyan, Color.magenta, Color.blue,
                Color.green, Color.red, Color.gray, Color.white
            };

            for (int i = 0; i < uiIconNames.Length; i++)
            {
                string iconName = uiIconNames[i];
                Color iconColor = uiColors[i];
                
                string filePath = $"Assets/Resources/Icons/UI/{iconName}.png";
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 生成图标
                Texture2D icon = GenerateUIIcon(iconName, iconColor);
                
                // 保存为PNG
                byte[] pngData = icon.EncodeToPNG();
                File.WriteAllBytes(filePath, pngData);
                
                // 导入设置
                AssetDatabase.ImportAsset(filePath);
                TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 32;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }

                DestroyImmediate(icon);
            }

            AssetDatabase.Refresh();
            Debug.Log("[PlaceholderGenerator] UI图标占位资源生成完成！");
        }

        /// <summary>
        /// 生成单个UI图标
        /// </summary>
        private static Texture2D GenerateUIIcon(string iconName, Color baseColor)
        {
            int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            // 清除为透明
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            texture.SetPixels(pixels);

            // 根据图标类型绘制不同形状
            switch (iconName)
            {
                case "StaminaIcon":
                    DrawHeart(texture, size / 2, size / 2, size / 3, baseColor);
                    break;
                case "LevelIcon":
                    DrawStar(texture, size / 2, size / 2, size / 3, baseColor);
                    break;
                case "CoinIcon":
                    DrawCircle(texture, size / 2, size / 2, size / 3, baseColor);
                    break;
                default:
                    DrawSquare(texture, size / 2, size / 2, size / 3, baseColor);
                    break;
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 绘制心形
        /// </summary>
        private static void DrawHeart(Texture2D texture, int centerX, int centerY, int size, Color color)
        {
            for (int x = -size; x <= size; x++)
            {
                for (int y = -size; y <= size; y++)
                {
                    float xNorm = (float)x / size;
                    float yNorm = (float)y / size;
                    
                    // 心形方程
                    float heart = xNorm * xNorm + (yNorm - Mathf.Sqrt(Mathf.Abs(xNorm))) * (yNorm - Mathf.Sqrt(Mathf.Abs(xNorm)));
                    
                    if (heart <= 1.0f)
                    {
                        int pixelX = centerX + x;
                        int pixelY = centerY + y;
                        if (pixelX >= 0 && pixelX < texture.width && pixelY >= 0 && pixelY < texture.height)
                        {
                            texture.SetPixel(pixelX, pixelY, color);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 绘制方形
        /// </summary>
        private static void DrawSquare(Texture2D texture, int centerX, int centerY, int size, Color color)
        {
            for (int x = -size; x <= size; x++)
            {
                for (int y = -size; y <= size; y++)
                {
                    int pixelX = centerX + x;
                    int pixelY = centerY + y;
                    if (pixelX >= 0 && pixelX < texture.width && pixelY >= 0 && pixelY < texture.height)
                    {
                        texture.SetPixel(pixelX, pixelY, color);
                    }
                }
            }
        }

        /// <summary>
        /// 生成音频占位资源
        /// </summary>
        public static void GenerateAudioPlaceholders()
        {
            Debug.Log("[PlaceholderGenerator] 开始生成音频占位资源...");

            string[] audioNames = {
                "CraftSuccess", "CraftFailure", "UnlockSuccess",
                "ExploreClick", "OrderComplete", "GridFull"
            };

            foreach (string audioName in audioNames)
            {
                string filePath = $"Assets/Resources/Audio/{audioName}.wav";
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 创建有效的WAV文件（包含静音数据）
                CreateValidWavFile(filePath);
                Debug.Log($"[PlaceholderGenerator] 创建WAV音频文件：{audioName}.wav");
            }

            AssetDatabase.Refresh();
            Debug.Log("[PlaceholderGenerator] 音频占位资源生成完成！");
        }

        /// <summary>
        /// 创建有效的WAV文件（包含静音数据）
        /// </summary>
        private static void CreateValidWavFile(string filePath)
        {
            // 创建一个有效的WAV文件，包含静音数据
            // 格式：单声道、16位、22050Hz、0.1秒静音
            
            int sampleRate = 22050;
            int bitsPerSample = 16;
            int channels = 1;
            float duration = 0.1f; // 0.1秒
            
            int numSamples = (int)(sampleRate * duration);
            int dataSize = numSamples * channels * (bitsPerSample / 8);
            int fileSize = 44 + dataSize; // 44字节头部 + 数据
            
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // RIFF头
                writer.Write(new char[] { 'R', 'I', 'F', 'F' }); // ChunkID
                writer.Write((uint)(fileSize - 8)); // ChunkSize (文件总大小 - 8)
                writer.Write(new char[] { 'W', 'A', 'V', 'E' }); // Format
                
                // fmt子块
                writer.Write(new char[] { 'f', 'm', 't', ' ' }); // Subchunk1ID
                writer.Write((uint)16); // Subchunk1Size (PCM格式为16)
                writer.Write((ushort)1); // AudioFormat (PCM = 1)
                writer.Write((ushort)channels); // NumChannels (单声道 = 1)
                writer.Write((uint)sampleRate); // SampleRate (采样率)
                writer.Write((uint)(sampleRate * channels * bitsPerSample / 8)); // ByteRate
                writer.Write((ushort)(channels * bitsPerSample / 8)); // BlockAlign
                writer.Write((ushort)bitsPerSample); // BitsPerSample (16位)
                
                // data子块
                writer.Write(new char[] { 'd', 'a', 't', 'a' }); // Subchunk2ID
                writer.Write((uint)dataSize); // Subchunk2Size (数据大小)
                
                // 写入静音数据（全0）
                for (int i = 0; i < numSamples; i++)
                {
                    writer.Write((short)0); // 16位静音样本
                }
            }
        }

        /// <summary>
        /// 生成微信相关占位资源
        /// </summary>
        public static void GenerateWeChatResources()
        {
            Debug.Log("[PlaceholderGenerator] 开始生成微信相关占位资源...");

            // 生成分享图片
            GenerateShareImage();
            
            // 生成登录按钮
            GenerateLoginButton();
            
            // 生成支付图标
            GeneratePayIcon();

            AssetDatabase.Refresh();
            Debug.Log("[PlaceholderGenerator] 微信相关占位资源生成完成！");
        }

        /// <summary>
        /// 生成分享图片
        /// </summary>
        private static void GenerateShareImage()
        {
            int width = 200;
            int height = 200;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            // 背景色
            Color bgColor = new Color(0.2f, 0.3f, 0.4f);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = bgColor;
            }
            texture.SetPixels(pixels);

            // 绘制简单的游戏Logo
            DrawText(texture, "末世", width / 2, height / 2 + 20, Color.white);
            DrawText(texture, "生存", width / 2, height / 2 - 10, Color.white);
            DrawText(texture, "合成", width / 2, height / 2 - 40, Color.white);

            texture.Apply();

            string filePath = "Assets/Resources/WeChat/WeChatShareImage.jpg";
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            byte[] jpgData = texture.EncodeToJPG(80);
            File.WriteAllBytes(filePath, jpgData);
            
            AssetDatabase.ImportAsset(filePath);
            DestroyImmediate(texture);
        }

        /// <summary>
        /// 生成登录按钮
        /// </summary>
        private static void GenerateLoginButton()
        {
            int width = 200;
            int height = 50;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            // 微信绿色
            Color wechatGreen = new Color(0.0f, 0.8f, 0.0f);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = wechatGreen;
            }
            texture.SetPixels(pixels);

            // 绘制圆角矩形
            DrawRoundedRect(texture, 5, 5, width - 10, height - 10, 10, wechatGreen);

            texture.Apply();

            string filePath = "Assets/Resources/WeChat/WeChatLoginButton.png";
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);
            
            AssetDatabase.ImportAsset(filePath);
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            DestroyImmediate(texture);
        }

        /// <summary>
        /// 生成支付图标
        /// </summary>
        private static void GeneratePayIcon()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            // 清除为透明
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            texture.SetPixels(pixels);

            // 绘制支付图标（简单的钱包形状）
            Color payColor = new Color(0.0f, 0.6f, 0.0f);
            DrawSquare(texture, size / 2, size / 2, size / 3, payColor);

            texture.Apply();

            string filePath = "Assets/Resources/WeChat/WeChatPayIcon.png";
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);
            
            AssetDatabase.ImportAsset(filePath);
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            DestroyImmediate(texture);
        }

        /// <summary>
        /// 绘制圆角矩形
        /// </summary>
        private static void DrawRoundedRect(Texture2D texture, int x, int y, int width, int height, int radius, Color color)
        {
            for (int px = x; px < x + width; px++)
            {
                for (int py = y; py < y + height; py++)
                {
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        texture.SetPixel(px, py, color);
                    }
                }
            }
        }

        /// <summary>
        /// 一键生成所有占位资源
        /// </summary>
        public static void GenerateAllPlaceholders()
        {
            Debug.Log("[PlaceholderGenerator] 开始生成所有占位资源...");

            GenerateItemIcons();
            GenerateUIIcons();
            GenerateAudioPlaceholders();
            GenerateWeChatResources();
            CreateAllUIPrefabs();

            Debug.Log("[PlaceholderGenerator] 所有占位资源生成完成！");
            Debug.Log("[PlaceholderGenerator] 现在可以运行游戏测试了！");
        }

        #region UI预制件创建

        public static void CreateAllUIPrefabs()
        {
            // 确保文件夹存在
            EnsureDirectoriesExist();

            // 创建主Canvas预制件
            CreateMainCanvasPrefab();

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

        /// <summary>
        /// 创建主Canvas预制件
        /// </summary>
        private static void CreateMainCanvasPrefab()
        {
            GameObject canvasGO = new GameObject("MainCanvas");

            // 添加Canvas组件
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            // 添加CanvasScaler
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 添加GraphicRaycaster
            canvasGO.AddComponent<GraphicRaycaster>();

            // 创建预制件
            PrefabUtility.SaveAsPrefabAsset(canvasGO, $"{PREFAB_PATH}/MainCanvas.prefab");
            Object.DestroyImmediate(canvasGO);
            
            Debug.Log("[PlaceholderGenerator] MainCanvas预制体创建完成");
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
            // 先确保GridCellUI预制体存在
            string gridCellUIPath = $"{PREFAB_PATH}/GridCellUI.prefab";
            GameObject gridCellUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gridCellUIPath);
            
            if (gridCellUIPrefab == null)
            {
                Debug.LogWarning("[PlaceholderGenerator] GridCellUI预制体不存在，先创建它");
                CreateGridCellUIPrefab();
                gridCellUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gridCellUIPath);
            }

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
            TestWebGL.Game.UI.GridUI gridUIComponent = gridUI.AddComponent<TestWebGL.Game.UI.GridUI>();
            
            // 使用SerializedObject设置cellPrefab引用
            SerializedObject serializedGridUI = new SerializedObject(gridUIComponent);
            serializedGridUI.FindProperty("cellPrefab").objectReferenceValue = gridCellUIPrefab;
            serializedGridUI.ApplyModifiedProperties();

            // 创建预制件
            PrefabUtility.SaveAsPrefabAsset(gridUI, $"{PREFAB_PATH}/GridUI.prefab");
            Object.DestroyImmediate(gridUI);
            
            Debug.Log("[PlaceholderGenerator] GridUI预制体创建完成，已自动设置cellPrefab引用");
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

            // 添加垂直布局
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.UpperCenter;

            // 玩家名称
            GameObject nameGO = CreateTextElement("PlayerName", "玩家信息", panel.transform);

            // 等级信息
            GameObject levelGO = CreateTextElement("LevelInfo", "等级 1", panel.transform);

            // 经验条容器
            GameObject expContainer = new GameObject("ExperienceBar");
            expContainer.transform.SetParent(panel.transform, false);
            RectTransform expContainerRect = expContainer.AddComponent<RectTransform>();
            expContainerRect.sizeDelta = new Vector2(280, 20);

            // 经验条背景
            GameObject expBg = new GameObject("Background");
            expBg.transform.SetParent(expContainer.transform, false);
            Image expBgImage = expBg.AddComponent<Image>();
            expBgImage.color = Color.gray;
            RectTransform expBgRect = expBg.GetComponent<RectTransform>();
            expBgRect.anchorMin = Vector2.zero;
            expBgRect.anchorMax = Vector2.one;
            expBgRect.offsetMin = Vector2.zero;
            expBgRect.offsetMax = Vector2.zero;

            // 经验条填充
            GameObject expFill = new GameObject("Fill");
            expFill.transform.SetParent(expBg.transform, false);
            Image expFillImage = expFill.AddComponent<Image>();
            expFillImage.color = Color.blue;
            RectTransform expFillRect = expFill.GetComponent<RectTransform>();
            expFillRect.anchorMin = Vector2.zero;
            expFillRect.anchorMax = Vector2.one;
            expFillRect.offsetMin = Vector2.zero;
            expFillRect.offsetMax = Vector2.zero;

            // 经验条Slider
            Slider experienceSlider = expBg.AddComponent<Slider>();
            experienceSlider.fillRect = expFillRect;
            experienceSlider.minValue = 0;
            experienceSlider.maxValue = 1;
            experienceSlider.interactable = false;

            // 经验文本
            GameObject expTextGO = CreateTextElement("ExperienceText", "经验: 0/100", expContainer.transform);

            // 体力条容器
            GameObject staminaContainer = new GameObject("StaminaBar");
            staminaContainer.transform.SetParent(panel.transform, false);
            RectTransform staminaContainerRect = staminaContainer.AddComponent<RectTransform>();
            staminaContainerRect.sizeDelta = new Vector2(280, 20);

            // 体力条背景
            GameObject staminaBg = new GameObject("Background");
            staminaBg.transform.SetParent(staminaContainer.transform, false);
            Image staminaBgImage = staminaBg.AddComponent<Image>();
            staminaBgImage.color = Color.gray;
            RectTransform staminaBgRect = staminaBg.GetComponent<RectTransform>();
            staminaBgRect.anchorMin = Vector2.zero;
            staminaBgRect.anchorMax = Vector2.one;
            staminaBgRect.offsetMin = Vector2.zero;
            staminaBgRect.offsetMax = Vector2.zero;

            // 体力条填充
            GameObject staminaFill = new GameObject("Fill");
            staminaFill.transform.SetParent(staminaBg.transform, false);
            Image staminaFillImage = staminaFill.AddComponent<Image>();
            staminaFillImage.color = Color.green;
            RectTransform staminaFillRect = staminaFill.GetComponent<RectTransform>();
            staminaFillRect.anchorMin = Vector2.zero;
            staminaFillRect.anchorMax = Vector2.one;
            staminaFillRect.offsetMin = Vector2.zero;
            staminaFillRect.offsetMax = Vector2.zero;

            // 体力条Slider
            Slider staminaSlider = staminaBg.AddComponent<Slider>();
            staminaSlider.fillRect = staminaFillRect;
            staminaSlider.minValue = 0;
            staminaSlider.maxValue = 1;
            staminaSlider.interactable = false;

            // 体力文本
            GameObject staminaTextGO = CreateTextElement("StaminaText", "体力: 100/100", staminaContainer.transform);

            // 游戏时长
            GameObject playTimeGO = CreateTextElement("PlayTime", "游戏时长: 00:00:00", panel.transform);

            // 获取组件引用
            TextMeshProUGUI playerNameText = nameGO.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI levelText = levelGO.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI experienceText = expTextGO.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI staminaText = staminaTextGO.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI playTimeText = playTimeGO.GetComponent<TextMeshProUGUI>();

            // 添加PlayerInfoPanel脚本并设置引用
            TestWebGL.Game.UI.PlayerInfoPanel playerInfoPanel = panel.AddComponent<TestWebGL.Game.UI.PlayerInfoPanel>();
            
            // 使用SerializedObject设置引用
            SerializedObject serializedPanel = new SerializedObject(playerInfoPanel);
            serializedPanel.FindProperty("playerNameText").objectReferenceValue = playerNameText;
            serializedPanel.FindProperty("levelText").objectReferenceValue = levelText;
            serializedPanel.FindProperty("experienceText").objectReferenceValue = experienceText;
            serializedPanel.FindProperty("experienceSlider").objectReferenceValue = experienceSlider;
            serializedPanel.FindProperty("staminaText").objectReferenceValue = staminaText;
            serializedPanel.FindProperty("staminaSlider").objectReferenceValue = staminaSlider;
            serializedPanel.FindProperty("playTimeText").objectReferenceValue = playTimeText;
            serializedPanel.ApplyModifiedProperties();

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

            // 创建按钮并获取引用
            Button exploreButton = CreateButton("ExploreButton", "探索", container.transform).GetComponent<Button>();
            Button settingsButton = CreateButton("SettingsButton", "设置", container.transform).GetComponent<Button>();
            Button ordersButton = CreateButton("OrdersButton", "订单", container.transform).GetComponent<Button>();
            Button achievementsButton = CreateButton("AchievementsButton", "成就", container.transform).GetComponent<Button>();
            Button saveButton = CreateButton("SaveButton", "保存", container.transform).GetComponent<Button>();

            // 获取按钮文本引用
            TextMeshProUGUI exploreButtonText = exploreButton.GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI settingsButtonText = settingsButton.GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI ordersButtonText = ordersButton.GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI achievementsButtonText = achievementsButton.GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI saveButtonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();

            // 添加ControlPanel脚本并设置引用
            TestWebGL.Game.UI.ControlPanel controlPanel = panel.AddComponent<TestWebGL.Game.UI.ControlPanel>();
            
            // 使用SerializedObject设置引用
            SerializedObject serializedPanel = new SerializedObject(controlPanel);
            serializedPanel.FindProperty("exploreButton").objectReferenceValue = exploreButton;
            serializedPanel.FindProperty("settingsButton").objectReferenceValue = settingsButton;
            serializedPanel.FindProperty("ordersButton").objectReferenceValue = ordersButton;
            serializedPanel.FindProperty("achievementsButton").objectReferenceValue = achievementsButton;
            serializedPanel.FindProperty("saveButton").objectReferenceValue = saveButton;
            serializedPanel.FindProperty("exploreButtonText").objectReferenceValue = exploreButtonText;
            serializedPanel.FindProperty("settingsButtonText").objectReferenceValue = settingsButtonText;
            serializedPanel.FindProperty("ordersButtonText").objectReferenceValue = ordersButtonText;
            serializedPanel.FindProperty("achievementsButtonText").objectReferenceValue = achievementsButtonText;
            serializedPanel.FindProperty("saveButtonText").objectReferenceValue = saveButtonText;
            serializedPanel.ApplyModifiedProperties();

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

            // 获取组件引用
            Image backgroundImage = popup.GetComponent<Image>(); // 获取背景Image组件
            Image itemIcon = iconGO.GetComponent<Image>();
            TextMeshProUGUI itemNameText = nameGO.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemDescriptionText = descGO.GetComponent<TextMeshProUGUI>();
            Button actionButton = actionBtn.GetComponent<Button>();
            Button closeButton = closeBtn.GetComponent<Button>();

            // 添加ItemDetailPopup脚本并设置引用
            TestWebGL.Game.UI.ItemDetailPopup itemDetailPopup = popup.AddComponent<TestWebGL.Game.UI.ItemDetailPopup>();
            
            // 使用SerializedObject设置引用
            SerializedObject serializedPopup = new SerializedObject(itemDetailPopup);
            serializedPopup.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
            serializedPopup.FindProperty("itemIcon").objectReferenceValue = itemIcon;
            serializedPopup.FindProperty("itemNameText").objectReferenceValue = itemNameText;
            serializedPopup.FindProperty("itemDescriptionText").objectReferenceValue = itemDescriptionText;
            serializedPopup.FindProperty("actionButton").objectReferenceValue = actionButton;
            serializedPopup.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedPopup.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(popup, $"{PREFAB_PATH}/ItemDetailPopup.prefab");
            Object.DestroyImmediate(popup);
        }

        private static void CreateSettingsPanelPrefab()
        {
            GameObject panel = CreateBasePanel("SettingsPanel", new Vector2(400, 500));

            // 添加垂直布局
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.UpperCenter;

            // 标题
            GameObject titleGO = CreateTextElement("Title", "游戏设置", panel.transform);

            // 音量设置容器
            GameObject volumeContainer = new GameObject("VolumeSettings");
            volumeContainer.transform.SetParent(panel.transform, false);
            VerticalLayoutGroup volumeLayout = volumeContainer.AddComponent<VerticalLayoutGroup>();
            volumeLayout.spacing = 10;
            volumeLayout.childAlignment = TextAnchor.UpperLeft;
            RectTransform volumeRect = volumeContainer.GetComponent<RectTransform>();
            volumeRect.sizeDelta = new Vector2(360, 120);

            // 音量标题
            CreateTextElement("VolumeTitle", "音量设置", volumeContainer.transform);

            // 主音量
            CreateSliderWithText(volumeContainer, "MasterVolume", "主音量");

            // 音乐音量
            CreateSliderWithText(volumeContainer, "MusicVolume", "音乐");

            // 音效音量
            CreateSliderWithText(volumeContainer, "SFXVolume", "音效");

            // 显示设置容器
            GameObject displayContainer = new GameObject("DisplaySettings");
            displayContainer.transform.SetParent(panel.transform, false);
            VerticalLayoutGroup displayLayout = displayContainer.AddComponent<VerticalLayoutGroup>();
            displayLayout.spacing = 10;
            displayLayout.childAlignment = TextAnchor.UpperLeft;
            RectTransform displayRect = displayContainer.GetComponent<RectTransform>();
            displayRect.sizeDelta = new Vector2(360, 80);

            // 显示标题
            CreateTextElement("DisplayTitle", "显示设置", displayContainer.transform);

            // 全屏切换
            CreateToggleWithText(displayContainer, "Fullscreen", "全屏模式");

            // 垂直同步
            CreateToggleWithText(displayContainer, "VSync", "垂直同步");

            // 游戏设置容器
            GameObject gameContainer = new GameObject("GameSettings");
            gameContainer.transform.SetParent(panel.transform, false);
            VerticalLayoutGroup gameLayout = gameContainer.AddComponent<VerticalLayoutGroup>();
            gameLayout.spacing = 10;
            gameLayout.childAlignment = TextAnchor.UpperLeft;
            RectTransform gameRect = gameContainer.GetComponent<RectTransform>();
            gameRect.sizeDelta = new Vector2(360, 80);

            // 游戏标题
            CreateTextElement("GameTitle", "游戏设置", gameContainer.transform);

            // 自动保存
            CreateToggleWithText(gameContainer, "AutoSave", "自动保存");

            // 显示提示
            CreateToggleWithText(gameContainer, "ShowTips", "显示提示");

            // 按钮容器
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(panel.transform, false);
            HorizontalLayoutGroup buttonLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 20;
            buttonLayout.childAlignment = TextAnchor.MiddleCenter;
            RectTransform buttonRect = buttonContainer.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(360, 50);

            // 重置按钮
            GameObject resetBtn = CreateButton("ResetButton", "重置", buttonContainer.transform);

            // 应用按钮
            GameObject applyBtn = CreateButton("ApplyButton", "应用", buttonContainer.transform);

            // 关闭按钮
            GameObject closeBtn = CreateButton("CloseButton", "关闭", buttonContainer.transform);

            // 获取组件引用
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            Image backgroundImage = panel.GetComponent<Image>();
            TextMeshProUGUI titleText = titleGO.GetComponent<TextMeshProUGUI>();
            Button applyButton = applyBtn.GetComponent<Button>();
            Button resetButton = resetBtn.GetComponent<Button>();
            Button closeButton = closeBtn.GetComponent<Button>();

            // 添加SettingsPanel脚本并设置引用
            TestWebGL.Game.UI.SettingsPanel settingsPanel = panel.AddComponent<TestWebGL.Game.UI.SettingsPanel>();
            
            // 使用SerializedObject设置引用
            SerializedObject serializedPanel = new SerializedObject(settingsPanel);
            serializedPanel.FindProperty("panelRect").objectReferenceValue = panelRect;
            serializedPanel.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
            serializedPanel.FindProperty("titleText").objectReferenceValue = titleText;
            serializedPanel.FindProperty("applyButton").objectReferenceValue = applyButton;
            serializedPanel.FindProperty("resetButton").objectReferenceValue = resetButton;
            serializedPanel.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedPanel.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(panel, $"{PREFAB_PATH}/SettingsPanel.prefab");
            Object.DestroyImmediate(panel);
        }

        /// <summary>
        /// 创建带文本的滑块
        /// </summary>
        private static void CreateSliderWithText(GameObject parent, string name, string label)
        {
            GameObject container = new GameObject(name + "Container");
            container.transform.SetParent(parent.transform, false);

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(340, 30);

            // 标签
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(container.transform, false);

            TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.font = GetDefaultFontAsset();
            labelText.text = label;
            labelText.fontSize = 14;
            labelText.color = Color.white;

            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(80, 30);

            // 滑块
            GameObject sliderGO = new GameObject("Slider");
            sliderGO.transform.SetParent(container.transform, false);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;

            // 滑块背景
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(sliderGO.transform, false);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.gray;
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // 滑块填充
            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(bgGO.transform, false);
            Image fillImage = fillGO.AddComponent<Image>();
            fillImage.color = Color.green;
            RectTransform fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            slider.fillRect = fillRect;

            RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(150, 20);

            // 数值文本
            GameObject textGO = new GameObject("Value");
            textGO.transform.SetParent(container.transform, false);

            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.font = GetDefaultFontAsset();
            text.text = "50%";
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Right;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(40, 30);
        }

        /// <summary>
        /// 创建带文本的切换开关
        /// </summary>
        private static void CreateToggleWithText(GameObject parent, string name, string label)
        {
            GameObject container = new GameObject(name + "Container");
            container.transform.SetParent(parent.transform, false);

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(340, 30);

            // 切换开关
            GameObject toggleGO = new GameObject("Toggle");
            toggleGO.transform.SetParent(container.transform, false);

            Toggle toggle = toggleGO.AddComponent<Toggle>();

            // 背景
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(toggleGO.transform, false);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.gray;
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(20, 20);

            // 选中标记
            GameObject checkmarkGO = new GameObject("Checkmark");
            checkmarkGO.transform.SetParent(bgGO.transform, false);
            Image checkImage = checkmarkGO.AddComponent<Image>();
            checkImage.color = Color.green;
            RectTransform checkRect = checkmarkGO.GetComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;

            toggle.graphic = checkImage;

            RectTransform toggleRect = toggleGO.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(30, 30);

            // 标签
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(container.transform, false);

            TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.font = GetDefaultFontAsset();
            labelText.text = label;
            labelText.fontSize = 14;
            labelText.color = Color.white;

            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(200, 30);
        }

        private static void CreateOrdersPanelPrefab()
        {
            GameObject panel = CreateBasePanel("OrdersPanel", new Vector2(400, 500));

            // 获取背景Image组件（CreateBasePanel已添加）
            Image backgroundImage = panel.GetComponent<Image>();

            // 标题
            GameObject titleGO = CreateTextElement("Title", "订单面板", panel.transform);
            titleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 220);

            // 订单容器
            GameObject container = new GameObject("OrdersContainer", typeof(RectTransform));
            container.transform.SetParent(panel.transform, false);

            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(350, 350);
            containerRect.anchoredPosition = new Vector2(0, 20);

            // 关闭按钮
            GameObject closeBtn = CreateButton("CloseButton", "关闭", panel.transform);
            closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);

            // 获取组件引用
            TextMeshProUGUI titleText = titleGO.GetComponent<TextMeshProUGUI>();
            Button closeButton = closeBtn.GetComponent<Button>();

            // 添加OrdersPanel脚本并设置引用
            TestWebGL.Game.UI.OrdersPanel ordersPanel = panel.AddComponent<TestWebGL.Game.UI.OrdersPanel>();
            
            // 使用SerializedObject设置引用
            SerializedObject serializedPanel = new SerializedObject(ordersPanel);
            serializedPanel.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
            serializedPanel.FindProperty("titleText").objectReferenceValue = titleText;
            serializedPanel.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedPanel.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(panel, $"{PREFAB_PATH}/OrdersPanel.prefab");
            Object.DestroyImmediate(panel);
        }

        private static void CreateAchievementPanelPrefab()
        {
            // 先创建成就项目预制件
            GameObject tempItem = CreateAchievementItemPrefab();
            // 销毁临时创建的GameObject
            if (tempItem != null)
            {
                Object.DestroyImmediate(tempItem);
            }

            // 加载已保存的预制件资源
            GameObject achievementItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_PATH}/AchievementItem.prefab");

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

            // 获取组件引用
            TextMeshProUGUI progressText = progressGO.GetComponent<TextMeshProUGUI>();
            Transform achievementListContainer = container.transform;
            Button closeButton = closeBtn.GetComponent<Button>();

            // 添加AchievementPanel脚本并设置引用
            TestWebGL.Game.UI.AchievementPanel achievementPanel = panel.AddComponent<TestWebGL.Game.UI.AchievementPanel>();
            
            // 使用SerializedObject设置引用
            SerializedObject serializedPanel = new SerializedObject(achievementPanel);
            serializedPanel.FindProperty("achievementItemPrefab").objectReferenceValue = achievementItemPrefab;
            serializedPanel.FindProperty("progressText").objectReferenceValue = progressText;
            serializedPanel.FindProperty("achievementListContainer").objectReferenceValue = achievementListContainer;
            serializedPanel.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedPanel.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(panel, $"{PREFAB_PATH}/AchievementPanel.prefab");
            Object.DestroyImmediate(panel);
        }

        /// <summary>
        /// 创建成就项目预制件
        /// </summary>
        private static GameObject CreateAchievementItemPrefab()
        {
            GameObject item = new GameObject("AchievementItem");

            // 背景
            Image backgroundImage = item.AddComponent<Image>();
            backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

            // 设置RectTransform
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(330, 60);

            // 图标
            GameObject iconGO = new GameObject("IconImage");
            iconGO.transform.SetParent(item.transform, false);
            Image iconImage = iconGO.AddComponent<Image>();
            iconImage.color = Color.yellow;

            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(10, 0);
            iconRect.sizeDelta = new Vector2(40, 40);

            // 标题文本
            GameObject titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(item.transform, false);
            TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.font = GetDefaultFontAsset();
            titleText.text = "成就标题";
            titleText.fontSize = 14;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Left;

            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(60, 0);
            titleRect.offsetMax = new Vector2(-10, -5);

            // 描述文本
            GameObject descGO = new GameObject("DescriptionText");
            descGO.transform.SetParent(item.transform, false);
            TextMeshProUGUI descText = descGO.AddComponent<TextMeshProUGUI>();
            descText.font = GetDefaultFontAsset();
            descText.text = "成就描述";
            descText.fontSize = 11;
            descText.color = Color.gray;
            descText.alignment = TextAlignmentOptions.Left;

            RectTransform descRect = descGO.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 0.5f);
            descRect.offsetMin = new Vector2(60, 5);
            descRect.offsetMax = new Vector2(-10, 0);

            // 进度文本
            GameObject progressGO = new GameObject("ProgressText");
            progressGO.transform.SetParent(item.transform, false);
            TextMeshProUGUI progressText = progressGO.AddComponent<TextMeshProUGUI>();
            progressText.font = GetDefaultFontAsset();
            progressText.text = "0/10";
            progressText.fontSize = 12;
            progressText.color = Color.white;
            progressText.alignment = TextAlignmentOptions.Right;

            RectTransform progressRect = progressGO.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(1, 0.5f);
            progressRect.anchorMax = new Vector2(1, 0.5f);
            progressRect.pivot = new Vector2(1, 0.5f);
            progressRect.anchoredPosition = new Vector2(-10, 0);
            progressRect.sizeDelta = new Vector2(60, 30);

            // 保存为预制件
            PrefabUtility.SaveAsPrefabAsset(item, $"{PREFAB_PATH}/AchievementItem.prefab");
            
            return item;
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

            Debug.LogWarning($"[PlaceholderGenerator] 未找到字体资产 '{defaultFontPath}'，使用 TMP 默认字体。");
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

        #endregion
    }
}