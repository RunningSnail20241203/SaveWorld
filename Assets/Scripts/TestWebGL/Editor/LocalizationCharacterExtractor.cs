using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SaveWorld.Editor
{
    /// <summary>
    /// 本地化字符提取工具
    /// 从所有Localization表中提取所有唯一字符，生成TXT文件
    /// </summary>
    public static class LocalizationCharacterExtractor
    {
        [MenuItem("本地化工具/提取所有字符到TXT")]
        public static void ExtractAllCharacters()
        {
            HashSet<char> allCharacters = new HashSet<char>();
            
            // 遍历所有字符串表
            foreach (var table in LocalizationSettings.StringDatabase.AllTables)
            {
                foreach (var entry in table.Values)
                {
                    string text = entry.GetLocalizedString();
                    foreach (char c in text)
                    {
                        allCharacters.Add(c);
                    }
                }
            }

            // 排序字符
            List<char> sortedChars = allCharacters.OrderBy(c => c).ToList();
            
            // 生成输出内容
            string output = string.Join("", sortedChars);
            
            // 保存文件
            string path = EditorUtility.SaveFilePanel("保存字符集", "Assets/TextMesh Pro/Fonts/", "localization_all_chars.txt", "txt");
            
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, output);
                Debug.Log($"✅ 已成功提取 {allCharacters.Count} 个唯一字符，保存到: {path}");
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("本地化工具/更新字体字符集")]
        public static void UpdateFontCharacterSet()
        {
            ExtractAllCharacters();
            string targetPath = "Assets/TextMesh Pro/Fonts/chinese_game_font_chars.txt";
            
            if (File.Exists(targetPath))
            {
                HashSet<char> allCharacters = new HashSet<char>();
                
                foreach (var table in LocalizationSettings.StringDatabase.AllTables)
                {
                    foreach (var entry in table.Values)
                    {
                        string text = entry.GetLocalizedString();
                        foreach (char c in text)
                        {
                            allCharacters.Add(c);
                        }
                    }
                }

                List<char> sortedChars = allCharacters.OrderBy(c => c).ToList();
                string output = string.Join("", sortedChars);
                
                File.WriteAllText(targetPath, output);
                Debug.Log($"✅ 字体字符集已更新，共 {allCharacters.Count} 个字符");
                AssetDatabase.Refresh();
            }
        }
    }
}