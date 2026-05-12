using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SaveWorld.Editor
{
    public class LocalizationTextCollector : EditorWindow
    {
        [Serializable]
        public class TextEntry
        {
            public bool Include = true;
            public string Key;
            public string Table;
            public string ChineseText;
            public string FilePath;
            public int LineNumber;
            public string Context;
        }

        private readonly List<TextEntry> _entries = new List<TextEntry>();
        private Vector2 _scrollPos;
        private string _filterText = "";
        private string _filterTable = "All";
        private bool _scanComplete;
        private bool _showEditableKeys;
        private string _csvOutputPath = "Assets/Localization/CSV";
        private string _statusMessage = "";

        private static readonly string[] TableOptions = { "All", "Items", "UI", "Errors", "Messages", "Orders" };

        private static readonly string[] AttributeBlacklist =
        {
            "[Header(", "[MenuItem(", "[Tooltip(", "[ContextMenu(",
            "[HelpURL(", "[FormerlySerializedAs(", "[AddComponentMenu(",
            "[CreateAssetMenu(", "[SelectionBase]"
        };

        private static readonly HashSet<string> DebugMethodNames = new HashSet<string>
        {
            "Debug.Log", "Debug.LogWarning", "Debug.LogError", "Debug.LogFormat",
            "Debug.Assert", "Debug.AssertFormat", "Debug.LogException",
            "print"
        };

        [MenuItem("本地化工具/收集所有中文文案")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalizationTextCollector>("文案收集器");
            window.minSize = new Vector2(800, 500);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("扫描代码", GUILayout.Width(120), GUILayout.Height(30)))
            {
                ScanAllCodeFiles();
            }
            if (GUILayout.Button("导出CSV", GUILayout.Width(120), GUILayout.Height(30)))
            {
                ExportCSV();
            }
            if (GUILayout.Button("刷新预制体扫描", GUILayout.Width(140), GUILayout.Height(30)))
            {
                ScanPrefabs();
            }
            if (GUILayout.Button("全选", GUILayout.Width(60), GUILayout.Height(30)))
            {
                foreach (var e in _entries) e.Include = true;
            }
            if (GUILayout.Button("取消全选", GUILayout.Width(80), GUILayout.Height(30)))
            {
                foreach (var e in _entries) e.Include = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _filterText = EditorGUILayout.TextField("筛选文本", _filterText);
            int selectedIdx = Array.IndexOf(TableOptions, _filterTable);
            if (selectedIdx < 0) selectedIdx = 0;
            selectedIdx = EditorGUILayout.Popup("筛选表", selectedIdx, TableOptions);
            _filterTable = TableOptions[selectedIdx];
            _showEditableKeys = EditorGUILayout.Toggle("编辑Key", _showEditableKeys);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("CSV输出路径", _csvOutputPath);
            EditorGUILayout.BeginHorizontal();
            _csvOutputPath = EditorGUILayout.TextField(_csvOutputPath);
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string selected = EditorUtility.SaveFolderPanel("选择CSV输出目录", "Assets/Localization", "CSV");
                if (!string.IsNullOrEmpty(selected))
                {
                    int assetsIdx = selected.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                    _csvOutputPath = assetsIdx >= 0 ? selected.Substring(assetsIdx) : selected;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }

            EditorGUILayout.Space();

            float totalBarWidth = position.width - 30;
            int itemCount = 0, uiCount = 0, errorCount = 0, msgCount = 0, orderCount = 0;
            foreach (var e in _entries)
            {
                switch (e.Table)
                {
                    case "Items": itemCount++; break;
                    case "UI": uiCount++; break;
                    case "Errors": errorCount++; break;
                    case "Messages": msgCount++; break;
                    case "Orders": orderCount++; break;
                }
            }
            EditorGUILayout.LabelField(
                $"总计: {_entries.Count} 条 | Items: {itemCount} | UI: {uiCount} | Errors: {errorCount} | Messages: {msgCount} | Orders: {orderCount}");

            if (!_scanComplete && _entries.Count == 0)
            {
                EditorGUILayout.HelpBox("点击「扫描代码」按钮扫描所有CS文件中的中文文案。", MessageType.Info);
                return;
            }

            // Table header
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("✓", GUILayout.Width(20));
            EditorGUILayout.LabelField("Key", GUILayout.Width(160));
            EditorGUILayout.LabelField("中文", GUILayout.Width(200));
            EditorGUILayout.LabelField("表", GUILayout.Width(55));
            EditorGUILayout.LabelField("上下文", GUILayout.Width(100));
            EditorGUILayout.LabelField("文件", GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            var filtered = GetFilteredEntries();
            foreach (var entry in filtered)
            {
                EditorGUILayout.BeginHorizontal();

                entry.Include = EditorGUILayout.Toggle(entry.Include, GUILayout.Width(20));

                if (_showEditableKeys)
                {
                    entry.Key = EditorGUILayout.TextField(entry.Key, GUILayout.Width(160));
                }
                else
                {
                    EditorGUILayout.LabelField(entry.Key, GUILayout.Width(160));
                }

                EditorGUILayout.LabelField(entry.ChineseText, GUILayout.Width(200));
                EditorGUILayout.LabelField(entry.Table, GUILayout.Width(55));
                EditorGUILayout.LabelField(entry.Context, GUILayout.Width(100));
                EditorGUILayout.LabelField(Path.GetFileName(entry.FilePath) + ":" + entry.LineNumber, GUILayout.Width(200));

                if (GUILayout.Button("定位", GUILayout.Width(40)))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(entry.FilePath);
                    if (obj != null)
                        AssetDatabase.OpenAsset(obj, entry.LineNumber);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private IEnumerable<TextEntry> GetFilteredEntries()
        {
            return _entries.Where(e =>
            {
                if (!string.IsNullOrEmpty(_filterText))
                {
                    if (!e.ChineseText.Contains(_filterText) && !e.Key.Contains(_filterText))
                        return false;
                }
                if (_filterTable != "All" && e.Table != _filterTable)
                    return false;
                return true;
            });
        }

        #region Scan

        private void ScanAllCodeFiles()
        {
            _entries.Clear();
            _scanComplete = false;

            string scriptsPath = Application.dataPath + "/Scripts/";
            if (!Directory.Exists(scriptsPath))
            {
                _statusMessage = "Scripts目录不存在: " + scriptsPath;
                return;
            }

            var csFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);
            int total = csFiles.Length;
            int processed = 0;

            for (int i = 0; i < csFiles.Length; i++)
            {
                string file = csFiles[i];
                string relativePath = "Assets" + file.Substring(Application.dataPath.Length).Replace('\\', '/');
                ScanFileForChineseStrings(file, relativePath);
                processed++;

                if (processed % 50 == 0)
                {
                    EditorUtility.DisplayProgressBar("扫描中...", $"已处理 {processed}/{total} 个文件", (float)processed / total);
                }
            }

            // Scan prefabs
            ScanPrefabs();

            EditorUtility.ClearProgressBar();

            DeduplicateEntries();
            _scanComplete = true;
            _statusMessage = $"扫描完成！共找到 {_entries.Count} 条中文文案（代码 + 预制体）";
            Repaint();
        }

        private void ScanPrefabs()
        {
            string prefabRoot = Application.dataPath + "/Resources/Prefabs/";
            if (!Directory.Exists(prefabRoot)) return;

            var prefabFiles = Directory.GetFiles(prefabRoot, "*.prefab", SearchOption.AllDirectories);

            for (int i = 0; i < prefabFiles.Length; i++)
            {
                string file = prefabFiles[i];
                string relativePath = "Assets" + file.Substring(Application.dataPath.Length).Replace('\\', '/');
                ScanPrefabForChineseText(file, relativePath);

                if (i % 20 == 0)
                {
                    EditorUtility.DisplayProgressBar("扫描预制体...", $"已处理 {i + 1}/{prefabFiles.Length}", 0.9f + 0.1f * (i + 1) / prefabFiles.Length);
                }
            }
        }

        private void ScanPrefabForChineseText(string filePath, string relativePath)
        {
            try
            {
                string content = File.ReadAllText(filePath, Encoding.UTF8);

                // TextMeshPro m_text fields:  m_text: 中文内容
                var textMatches = Regex.Matches(content, @"m_text:\s*(.+)$", RegexOptions.Multiline);
                foreach (Match m in textMatches)
                {
                    string val = m.Groups[1].Value.Trim();
                    if (ContainsChinese(val))
                    {
                        string key = GenerateUIKey(val, Path.GetFileNameWithoutExtension(filePath));
                        AddEntry(key, "UI", val, relativePath, GetLineNumber(content, m.Index), "Prefab m_text");
                    }
                }

                // Legacy Text component:  m_Text: 中文内容
                var legacyMatches = Regex.Matches(content, @"m_Text:\s*(.+)$", RegexOptions.Multiline);
                foreach (Match m in legacyMatches)
                {
                    string val = m.Groups[1].Value.Trim();
                    if (ContainsChinese(val))
                    {
                        string key = GenerateUIKey(val, Path.GetFileNameWithoutExtension(filePath));
                        AddEntry(key, "UI", val, relativePath, GetLineNumber(content, m.Index), "Prefab m_Text");
                    }
                }

                // Placeholder text:  m_PlaceholderText: 中文
                var phMatches = Regex.Matches(content, @"m_PlaceholderText:\s*(.+)$", RegexOptions.Multiline);
                foreach (Match m in phMatches)
                {
                    string val = m.Groups[1].Value.Trim();
                    if (ContainsChinese(val))
                    {
                        string key = GenerateUIKey(val, Path.GetFileNameWithoutExtension(filePath));
                        AddEntry(key, "UI", val, relativePath, GetLineNumber(content, m.Index), "Prefab Placeholder");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LocalizationTextCollector] 扫描预制体失败: {relativePath}, {ex.Message}");
            }
        }

        private void ScanFileForChineseStrings(string filePath, string relativePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                string fullContent = File.ReadAllText(filePath, Encoding.UTF8);
                string fileName = Path.GetFileName(filePath);

                // For ItemConfig.cs, use structured extraction
                if (fileName == "ItemConfig.cs")
                {
                    ExtractItemConfigStrings(lines, relativePath);
                    return;
                }

                // For AchievementSystem.cs, use structured extraction only (skip general scanning)
                if (fileName == "AchievementSystem.cs")
                {
                    ExtractAchievementStrings(lines, relativePath);
                    continue;
                }

                // Track block comment state
                bool inBlockComment = false;

                for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
                {
                    int lineNumber = lineIdx + 1;
                    string trimmedLine = lines[lineIdx].TrimStart();

                    // Handle block comments
                    if (inBlockComment)
                    {
                        if (trimmedLine.Contains("*/"))
                        {
                            inBlockComment = false;
                            int endIdx = trimmedLine.IndexOf("*/", StringComparison.Ordinal) + 2;
                            if (endIdx < trimmedLine.Length)
                            {
                                ParseLineForStrings(trimmedLine.Substring(endIdx), filePath, relativePath, fileName, lineNumber);
                            }
                        }
                        continue;
                    }

                    if (trimmedLine.Contains("/*"))
                    {
                        int startIdx = trimmedLine.IndexOf("/*", StringComparison.Ordinal);
                        if (startIdx > 0)
                        {
                            ParseLineForStrings(trimmedLine.Substring(0, startIdx), filePath, relativePath, fileName, lineNumber);
                        }
                        int closeIdx = trimmedLine.IndexOf("*/", startIdx + 2, StringComparison.Ordinal);
                        if (closeIdx < 0)
                        {
                            inBlockComment = true;
                            continue;
                        }
                        int endIdx = closeIdx + 2;
                        if (endIdx < trimmedLine.Length)
                        {
                            ParseLineForStrings(trimmedLine.Substring(endIdx), filePath, relativePath, fileName, lineNumber);
                        }
                        continue;
                    }

                    // Skip single-line comments (//)
                    if (trimmedLine.StartsWith("//"))
                        continue;

                    // Skip Unity attributes that are Editor-only
                    if (IsBlacklistedAttribute(trimmedLine))
                        continue;

                    // Remove trailing comment from the line
                    string cleanLine = RemoveTrailingComment(trimmedLine);
                    if (string.IsNullOrWhiteSpace(cleanLine))
                        continue;

                    ParseLineForStrings(cleanLine, filePath, relativePath, fileName, lineNumber);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LocalizationTextCollector] 扫描文件失败: {relativePath}, {ex.Message}");
            }
        }

        private void ParseLineForStrings(string line, string filePath, string relativePath, string fileName, int lineNumber)
        {
            // Extract all double-quoted strings from the line
            var strings = ExtractQuotedStrings(line);
            foreach (var extracted in strings)
            {
                if (!ContainsChinese(extracted))
                    continue;

                // Skip if it's a Debug.Log call and the string is just a log prefix
                if (IsDebugLogString(line))
                    continue;

                string table = DetermineTable(fileName, line);
                string key = GenerateKey(extracted, table, fileName, line);
                string context = DetermineContext(line, fileName);

                AddEntry(key, table, extracted, relativePath, lineNumber, context);
            }
        }

        private List<string> ExtractQuotedStrings(string line)
        {
            var result = new List<string>();
            bool inString = false;
            int stringStart = -1;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '$' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    i++; // Skip the $" together
                    inString = true;
                    stringStart = i + 1;
                    continue;
                }

                if (c == '\\' && i + 1 < line.Length)
                {
                    i++; // Skip escaped character
                    continue;
                }

                if (c == '"')
                {
                    if (!inString)
                    {
                        inString = true;
                        stringStart = i + 1;
                    }
                    else
                    {
                        inString = false;
                        if (stringStart >= 0)
                        {
                            string str = line.Substring(stringStart, i - stringStart);
                            result.Add(str);
                        }
                        stringStart = -1;
                    }
                }
            }

            return result;
        }

        private static bool IsBlacklistedAttribute(string line)
        {
            foreach (var attr in AttributeBlacklist)
            {
                if (line.StartsWith(attr, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static bool IsDebugLogString(string line)
        {
            foreach (var method in DebugMethodNames)
            {
                if (line.Contains(method + "(") || line.Contains(method + " (") ||
                    line.Contains(method + "Format(") || line.Contains(method + "Format ("))
                    return true;
            }
            return false;
        }

        private static string RemoveTrailingComment(string line)
        {
            bool inString = false;
            char prev = '\0';
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"' && prev != '\\')
                    inString = !inString;
                if (!inString && c == '/' && i + 1 < line.Length && line[i + 1] == '/')
                    return line.Substring(0, i).TrimEnd();
                prev = c;
            }
            return line;
        }

        private static bool ContainsChinese(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                if (c >= 0x4e00 && c <= 0x9fff) return true;
                if (c >= 0x3400 && c <= 0x4dbf) return true;
                if (c >= 0x3000 && c <= 0x303f) return true;
                if (c >= 0xff00 && c <= 0xffef) return true;
                if (c >= 0xf900 && c <= 0xfaff) return true;
            }
            return false;
        }

        private static int GetLineNumber(string content, int charIndex)
        {
            int lineNum = 1;
            for (int i = 0; i < charIndex && i < content.Length; i++)
            {
                if (content[i] == '\n') lineNum++;
            }
            return lineNum;
        }

        #endregion

        #region Structured Extractors

        private void ExtractItemConfigStrings(string[] lines, string relativePath)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                int lineNum = i + 1;
                string line = lines[i].Trim();

                // Match: { ItemType.Xxx, new ItemData { ... itemName = "中文名称", ... description = "中文描述" ... } },
                if (line.StartsWith("//")) continue;

                // Extract ItemType
                var itemTypeMatch = Regex.Match(line, @"\{\s*ItemType\.(\w+)\s*,");
                string enumName = itemTypeMatch.Success ? itemTypeMatch.Groups[1].Value : null;

                if (enumName != null && line.Contains("itemName"))
                {
                    // Try to get name and description from this and next lines
                    string fullBlock = line;
                    int j = i + 1;
                    while (j < lines.Length && !lines[j].Trim().StartsWith("},") && j < i + 5)
                    {
                        fullBlock += lines[j];
                        j++;
                    }

                    var nameMatch = Regex.Match(fullBlock, @"itemName\s*=\s*""([^""]*)""");
                    var descMatch = Regex.Match(fullBlock, @"description\s*=\s*""([^""]*)""");

                    if (nameMatch.Success && ContainsChinese(nameMatch.Groups[1].Value))
                    {
                        AddEntry(enumName, "Items", nameMatch.Groups[1].Value, relativePath, lineNum, "ItemName");
                    }
                    if (descMatch.Success && ContainsChinese(descMatch.Groups[1].Value))
                    {
                        AddEntry(enumName + "_Desc", "Items", descMatch.Groups[1].Value, relativePath, lineNum, "ItemDesc");
                    }
                }

                // Fallback strings in GetItemName / GetItemDescription
                if (line.Contains("\"未知物品\"") && !line.TrimStart().StartsWith("//"))
                {
                    AddEntry("Unknown_Item", "Items", "未知物品", relativePath, lineNum, "Fallback");
                }
                if (line.Contains("\"未知物品描述\"") && !line.TrimStart().StartsWith("//"))
                {
                    AddEntry("Unknown_Item_Desc", "Items", "未知物品描述", relativePath, lineNum, "Fallback");
                }
            }
        }

        private void ExtractAchievementStrings(string[] lines, string relativePath)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                int lineNum = i + 1;
                string line = lines[i].Trim();

                if (line.StartsWith("//")) continue;

                // Match: AddAchievementIfNotExists(achievements, AchievementType.Xxx, "中文名", "中文描述", ...)
                var match = Regex.Match(line, @"AddAchievementIfNotExists\s*\([^,]+,\s*\w+\.(\w+),\s*""([^""]*)""\s*,\s*""([^""]*)""");
                if (match.Success)
                {
                    string typeName = match.Groups[1].Value;
                    string name = match.Groups[2].Value;
                    string desc = match.Groups[3].Value;

                    if (ContainsChinese(name))
                    {
                        AddEntry("Achievement_" + typeName + "_Name", "Messages", name, relativePath, lineNum, "AchievementName");
                    }
                    if (ContainsChinese(desc))
                    {
                        AddEntry("Achievement_" + typeName + "_Desc", "Messages", desc, relativePath, lineNum, "AchievementDesc");
                    }
                }

                // Match: Reason = "成就已经解锁" / "成就条件未达成"
                var reasonMatch = Regex.Match(line, @"Reason\s*=\s*""([^""]*)""");
                if (reasonMatch.Success && ContainsChinese(reasonMatch.Groups[1].Value))
                {
                    string reason = reasonMatch.Groups[1].Value;
                    string key = GenerateErrorKey(reason);
                    AddEntry(key, "Errors", reason, relativePath, lineNum, "AchievementError");
                }
            }
        }

        #region Categorization

        private static string DetermineTable(string fileName, string line)
        {
            // Error messages from engines
            if (fileName == "CraftingEngine.cs" || fileName == "OrderEngine.cs" ||
                fileName == "ExplorationEngine.cs" || fileName == "ExplorationSystem.cs")
            {
                if (line.Contains("OnCraftFailure") || line.Contains(".Fail(") ||
                    line.Contains("Reason =") || line.Contains("reason =") ||
                    line.Contains("RemoveError") || line.Contains("PlaceError"))
                    return "Errors";
                return "Messages";
            }

            // Item config
            if (fileName == "ItemConfig.cs")
            {
                if (line.Contains("description ="))
                    return "Items";
                if (line.Contains("itemName ="))
                    return "Items";
                return "Items";
            }

            // Achievement
            if (fileName == "AchievementSystem.cs")
            {
                if (line.Contains("Reason ="))
                    return "Errors";
                return "Messages";
            }

            // Placeholder UI
            if (fileName == "PlaceholderResourceGenerator.cs")
            {
                if (line.Contains(".text =") || line.Contains("CreateTextElement") ||
                    line.Contains("ButtonText"))
                    return "UI";
                return "UI";
            }

            // Player
            if (fileName == "PlayerManager.cs")
            {
                if (line.Contains("playerName ="))
                    return "Messages";
                return "Messages";
            }

            // WeChat
            if (fileName == "WeChatShareSystem.cs" || fileName.Contains("WeChat"))
            {
                return "Messages";
            }

            // Storage
            if (fileName.Contains("Storage") || fileName.Contains("Cloud"))
            {
                if (line.Contains("成功") || line.Contains("失败") || line.Contains("错误") || line.Contains("异常"))
                    return "Errors";
                return "Messages";
            }

            // UI files
            if (fileName.Contains("UI") || fileName.EndsWith("UI.cs") || fileName == "ControlPanel.cs")
            {
                return "UI";
            }

            // Order related
            if (fileName.Contains("Order"))
            {
                return "Orders";
            }

            return "Messages";
        }

        private static string DetermineContext(string line, string fileName)
        {
            var contexts = new List<string>();

            if (line.Contains("OnCraftFailure") || line.Contains("reason =") || line.Contains("Reason ="))
                contexts.Add("Error");
            if (line.Contains(".Fail("))
                contexts.Add("FailResult");
            if (line.Contains("itemName ="))
                contexts.Add("ItemName");
            if (line.Contains("description ="))
                contexts.Add("ItemDesc");
            if (line.Contains(".text =") || line.Contains("CreateTextElement"))
                contexts.Add("UIText");
            if (line.Contains("title =") || line.Contains("inviteMessage"))
                contexts.Add("MsgTitle");
            if (line.Contains("Name ="))
                contexts.Add("Name");
            if (line.Contains("Description ="))
                contexts.Add("Desc");
            if (line.Contains("playerName"))
                contexts.Add("Default");

            return contexts.Count > 0 ? string.Join("|", contexts) : fileName.Replace(".cs", "");
        }

        #endregion

        #region Key Generation

        private string GenerateKey(string text, string table, string fileName, string line)
        {
            // Try to extract key from surrounding code context
            string contextKey = ExtractKeyFromContext(line);
            if (!string.IsNullOrEmpty(contextKey))
                return contextKey;

            switch (table)
            {
                case "Errors":
                    return GenerateErrorKey(text);
                case "UI":
                    return GenerateUIKey(text, fileName);
                case "Messages":
                    return GenerateMessageKey(text);
                case "Orders":
                    return GenerateOrderKey(text);
                default:
                    return GenerateMessageKey(text);
            }
        }

        private static string ExtractKeyFromContext(string line)
        {
            // Pattern: CreateTextElement("Name", "中文", ...)
            var teMatch = Regex.Match(line, @"CreateTextElement\s*\(\s*""(\w+)""");
            if (teMatch.Success)
                return teMatch.Groups[1].Value;

            // Pattern: variableName = "中文"
            var varMatch = Regex.Match(line, @"(\w+)(Text|Name|Title|Label)\s*=\s*""");
            if (varMatch.Success)
                return varMatch.Groups[1].Value + varMatch.Groups[2].Value;

            return null;
        }

        private static string GenerateErrorKey(string text)
        {
            // Shorten common Chinese error messages to English key-like names
            var map = new Dictionary<string, string>
            {
                ["格子无法合成"] = "Error_Cell_Cannot_Craft",
                ["未找到合成规则"] = "Error_No_Crafting_Rule",
                ["没有空格子放置合成结果"] = "Error_No_Empty_Cell",
                ["物品类型不匹配"] = "Error_Item_Type_Mismatch",
                ["物品等级不匹配"] = "Error_Item_Level_Mismatch",
                ["物品数量不足（需要2个）"] = "Error_Insufficient_Quantity",
                ["来源格子无效"] = "Error_Invalid_Source_Cell",
                ["目标格子不是锁定格子"] = "Error_Target_Not_Locked",
                ["目标格子无效"] = "Error_Invalid_Target_Cell",
                ["不同类型物品无法合并"] = "Error_Cannot_Merge_Different_Types",
                ["没有可以合成的物品"] = "Error_No_Craftable_Item",
                ["体力不足"] = "Error_Insufficient_Stamina",
                ["背包已满"] = "Error_Backpack_Full",
                ["订单不存在"] = "Error_Order_Not_Found",
                ["订单已完成"] = "Error_Order_Completed",
                ["订单已过期"] = "Error_Order_Expired",
                ["背包中没有需要的物品"] = "Error_No_Required_Item",
                ["成就已经解锁"] = "Error_Achievement_Already_Unlocked",
                ["成就条件未达成"] = "Error_Achievement_Not_Completed",
                ["保存失败"] = "Error_Save_Failed",
                ["加载失败"] = "Error_Load_Failed",
                ["设置序列化失败"] = "Error_Settings_Serialize_Failed",
                ["设置数据损坏，使用默认设置"] = "Error_Settings_Corrupted",
                ["设置反序列化失败，使用默认设置"] = "Error_Settings_Deserialize_Failed",
                ["同步进行中"] = "Error_Sync_In_Progress",
                ["无本地存档"] = "Error_No_Local_Save",
                ["云端同步成功"] = "Info_Cloud_Sync_Success",
                ["云端数据已下载到本地"] = "Info_Cloud_Data_Downloaded",
            };

            if (map.TryGetValue(text, out string key))
                return key;

            // Fallback: hash-based key
            return "Error_Unknown_" + Math.Abs(text.GetHashCode()).ToString("X4");
        }

        private static string GenerateUIKey(string text, string fileName)
        {
            var map = new Dictionary<string, string>
            {
                ["探索"] = "UI_Btn_Explore",
                ["订单"] = "UI_Btn_Order",
                ["设置"] = "UI_Btn_Settings",
                ["成就"] = "UI_Btn_Achievements",
                ["保存"] = "UI_Btn_Save",
                ["合成"] = "UI_Btn_Craft",
                ["背包"] = "UI_Panel_Backpack",
                ["玩家信息"] = "UI_Panel_PlayerInfo",
                ["游戏设置"] = "UI_Panel_GameSettings",
                ["订单管理"] = "UI_Panel_OrderManager",
                ["活跃订单"] = "UI_Tab_ActiveOrders",
                ["已完成"] = "UI_Tab_Completed",
                ["选择订单查看详情"] = "UI_Text_SelectOrderHint",
                ["完成订单"] = "UI_Btn_CompleteOrder",
                ["探索结果"] = "UI_Title_ExplorationResult",
                ["成就面板"] = "UI_Panel_Achievements",
                ["物品名称"] = "UI_Label_ItemName",
                ["物品描述"] = "UI_Label_ItemDesc",
                ["提交"] = "UI_Btn_Submit",
                ["刷新"] = "UI_Btn_Refresh",
                ["音量设置"] = "UI_Label_VolumeSettings",
                ["显示设置"] = "UI_Label_DisplaySettings",
                ["按钮"] = "UI_Btn_Generic",
            };

            if (map.TryGetValue(text, out string key))
                return key;

            // For dynamic strings like "等级 1", "体力: 100/100", "经验: 0/100"
            if (text.Contains("等级"))
                return "UI_Format_LevelInfo";
            if (text.Contains("体力"))
                return "UI_Format_Stamina";
            if (text.Contains("经验"))
                return "UI_Format_Experience";
            if (text.Contains("金币"))
                return "UI_Format_Gold";
            if (text.Contains("游戏时长"))
                return "UI_Format_PlayTime";

            return "UI_" + Path.GetFileNameWithoutExtension(fileName) + "_" + Math.Abs(text.GetHashCode()).ToString("X4");
        }

        private static string GenerateMessageKey(string text)
        {
            var map = new Dictionary<string, string>
            {
                ["玩家"] = "Default_PlayerName",
                ["末世生存合成"] = "Game_Title",
                ["来和我一起玩《末世生存合成》吧！"] = "Share_Invite_Default",
                ["分享成功"] = "Share_Success",
                ["保存成功"] = "Storage_Save_Success",
                ["加载成功"] = "Storage_Load_Success",
                ["成功"] = "Result_Success",
                ["失败"] = "Result_Failure",
                ["成就标题"] = "UI_Achievement_Title_Placeholder",
                ["成就描述"] = "UI_Achievement_Desc_Placeholder",
            };

            if (map.TryGetValue(text, out string key))
                return key;

            // Handle share format strings
            if (text.Contains("我在《末世生存合成》中"))
            {
                if (text.Contains("解锁了成就"))
                    return "Share_Achievement_Format";
                if (text.Contains("达到了") && text.Contains("级"))
                    return "Share_Progress_Format";
                if (text.Contains("排行榜"))
                    return "Share_Ranking_Format";
            }

            return "Msg_" + Math.Abs(text.GetHashCode()).ToString("X4");
        }

        private static string GenerateOrderKey(string text)
        {
            return "Order_" + Math.Abs(text.GetHashCode()).ToString("X4");
        }

        #endregion

        #region Entry Management

        private void AddEntry(string key, string table, string chineseText, string filePath, int lineNumber, string context)
        {
            _entries.Add(new TextEntry
            {
                Key = key,
                Table = table,
                ChineseText = chineseText,
                FilePath = filePath,
                LineNumber = lineNumber,
                Context = context
            });
        }

        private void DeduplicateEntries()
        {
            var seen = new Dictionary<string, TextEntry>();

            foreach (var entry in _entries)
            {
                string dedupKey = entry.ChineseText + "|" + entry.Table;
                if (!seen.ContainsKey(dedupKey))
                {
                    seen[dedupKey] = entry;
                }
                else
                {
                    var existing = seen[dedupKey];
                    if (IsGeneratedKey(existing.Key) && !IsGeneratedKey(entry.Key))
                    {
                        seen[dedupKey] = entry;
                    }
                }
            }

            _entries.Clear();
            _entries.AddRange(seen.Values.OrderBy(e => e.Table).ThenBy(e => e.Key));
        }

        private static bool IsGeneratedKey(string key)
        {
            // A "generated" key is one that was auto-assigned via hash (4 hex digits at end, or starts with Msg_/Order_)
            if (key.StartsWith("Msg_") || key.StartsWith("Order_"))
                return true;
            if (key.Contains("_Unknown_"))
                return true;
            return false;
        }

        #endregion

        #region CSV Export

        private void ExportCSV()
        {
            var selectedEntries = _entries.Where(e => e.Include).ToList();
            if (selectedEntries.Count == 0)
            {
                _statusMessage = "没有选中任何条目，请先勾选要导出的文案。";
                return;
            }

            // Ensure output directory exists
            string fullPath = Path.Combine(Application.dataPath, "..", _csvOutputPath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            // Group by table
            var tables = selectedEntries.GroupBy(e => e.Table);
            var exportedFiles = new List<string>();

            foreach (var group in tables)
            {
                string tableName = group.Key;
                string filePath = Path.Combine(fullPath, $"{tableName}.csv");

                // Generate CSV with Unity Localization compatible format
                // Format: Key,Id,Description,zh-CN,en
                var sb = new StringBuilder();
                sb.AppendLine("Key,Id,Description,zh-CN,en");

                foreach (var entry in group.OrderBy(e => e.Key))
                {
                    string desc = EscapeCsv(entry.Context ?? "");
                    string zh = EscapeCsv(entry.ChineseText);
                    string en = EscapeCsv(GenerateEnglishPlaceholder(entry.ChineseText, entry.Table));
                    sb.AppendLine($"{EscapeCsv(entry.Key)},,{desc},{zh},{en}");
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                exportedFiles.Add(filePath);
            }

            // Also generate a combined CSV for convenience
            {
                string combinedPath = Path.Combine(fullPath, "All_Localization.csv");
                var combinedSb = new StringBuilder();
                combinedSb.AppendLine("Key,Id,Description,Table,zh-CN,en");

                foreach (var entry in selectedEntries.OrderBy(e => e.Table).ThenBy(e => e.Key))
                {
                    string desc = EscapeCsv(entry.Context ?? "");
                    string zh = EscapeCsv(entry.ChineseText);
                    string en = EscapeCsv(GenerateEnglishPlaceholder(entry.ChineseText, entry.Table));
                    string table = EscapeCsv(entry.Table);
                    combinedSb.AppendLine($"{EscapeCsv(entry.Key)},,{desc},{table},{zh},{en}");
                }

                File.WriteAllText(combinedPath, combinedSb.ToString(), Encoding.UTF8);
                exportedFiles.Add(combinedPath);
            }

            AssetDatabase.Refresh();

            _statusMessage = $"导出完成！共 {selectedEntries.Count} 条，生成文件：\n" +
                string.Join("\n", exportedFiles.Select(f => "  " + f));
            Debug.Log($"[LocalizationTextCollector] CSV导出完成:\n{_statusMessage}");
        }

        private static string EscapeCsv(string text)
        {
            if (text == null) return "";
            if (text.Contains(",") || text.Contains("\"") || text.Contains("\n"))
            {
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            }
            return text;
        }

        private static string GenerateEnglishPlaceholder(string chinese, string table)
        {
            // For Items table, just use the key name converted to readable text
            // For others, provide the Chinese as-is (will need manual translation)
            if (table == "Items")
            {
                return $"[TODO] {chinese}";
            }
            return $"[待翻译] {chinese}";
        }

        #endregion
    }
}
