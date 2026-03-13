using UnityEngine;
using UnityEditor;
using System.IO;

namespace TestWebGL.Editor
{
    /// <summary>
    /// 资源创建工具
    /// 创建基础的占位符资源文件
    /// </summary>
    public class ResourceCreator
    {
        private const string RESOURCE_PATH = "Assets/Resources";

        [MenuItem("Tools/TestWebGL/Create Placeholder Resources")]
        public static void CreatePlaceholderResources()
        {
            CreatePlaceholderTextures();
            CreatePlaceholderAudio();
            Debug.Log("占位符资源创建完成！");
            AssetDatabase.Refresh();
        }

        private static void CreatePlaceholderTextures()
        {
            // 创建物品图标占位符
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/Items/Water_L1.png", Color.blue);
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/Items/Food_L1.png", Color.green);
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/Items/Tool_L1.png", Color.gray);
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/Items/Home_L1.png", Color.yellow);
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/Items/Medical_L1.png", Color.red);
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/Items/Energy_L1.png", Color.cyan);
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/Items/Knowledge_L1.png", Color.magenta);
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/Items/Hope_L1.png", Color.white);
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/Items/Explore_L1.png", new Color(1f, 0.5f, 0f));

            // 创建默认图标
            CreatePlaceholderTexture($"{RESOURCE_PATH}/Icons/DefaultItem.png", Color.black);

            // 创建UI精灵
            CreatePlaceholderTexture($"{RESOURCE_PATH}/UI/PanelBackground.png", new Color(0.1f, 0.1f, 0.1f, 0.9f));
            CreatePlaceholderTexture($"{RESOURCE_PATH}/UI/ButtonNormal.png", new Color(0.3f, 0.3f, 0.3f));
            CreatePlaceholderTexture($"{RESOURCE_PATH}/UI/ButtonHover.png", new Color(0.4f, 0.4f, 0.4f));
            CreatePlaceholderTexture($"{RESOURCE_PATH}/UI/ButtonPressed.png", new Color(0.2f, 0.2f, 0.2f));
        }

        private static void CreatePlaceholderTexture(string path, Color color)
        {
            // 确保目录存在
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // 创建64x64的纹理
            Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[64 * 64];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            // 保存为PNG
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            // 清理内存
            Object.DestroyImmediate(texture);

            Debug.Log($"创建占位符纹理: {path}");
        }

        private static void CreatePlaceholderAudio()
        {
            // 注意：Unity不直接支持代码创建音频文件
            // 这里只创建目录和提示信息
            string audioPath = $"{RESOURCE_PATH}/Audio";
            if (!Directory.Exists(audioPath))
                Directory.CreateDirectory(audioPath);

            // 创建一个说明文件
            string readmePath = $"{audioPath}/README.txt";
            string readmeContent = @"
音频资源说明：

请将以下音频文件放入此文件夹：

1. CraftSuccess.wav - 合成成功音效
2. CraftFailure.wav - 合成失败音效
3. UnlockSuccess.wav - 解锁成功音效
4. ExploreClick.wav - 探索点击音效
5. OrderComplete.wav - 订单完成音效
6. GridFull.wav - 满格提示音效

音频格式建议：
- WAV格式，16位，44100Hz
- 文件大小控制在100KB以内
- 音效长度1-2秒

临时替代方案：
当前系统会输出调试信息代替实际音效播放。
";
            File.WriteAllText(readmePath, readmeContent);

            Debug.Log("创建音频资源说明文件");
        }

        [MenuItem("Tools/TestWebGL/Setup Project Structure")]
        public static void SetupProjectStructure()
        {
            // 创建所有必要的文件夹
            string[] directories = {
                "Assets/Prefabs",
                "Assets/Prefabs/UI",
                "Assets/Resources",
                "Assets/Resources/Icons",
                "Assets/Resources/Icons/Items",
                "Assets/Resources/UI",
                "Assets/Resources/Audio",
                "Assets/Resources/Fonts",
                "Assets/Scenes",
                "Assets/Scripts",
                "Assets/Scripts/TestWebGL",
                "Assets/Scripts/TestWebGL/Editor"
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Debug.Log($"创建目录: {dir}");
                }
            }

            Debug.Log("项目结构设置完成！");
        }
    }
}