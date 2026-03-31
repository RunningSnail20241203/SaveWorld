# 音频导入错误修复说明

## 问题描述
Unity在导入音频资源时出现错误：
```
Errors during import of AudioClip Assets/Resources/Audio/OrderComplete.wav:
FSBTool ERROR: Internal error from FMOD sub-system.
```

## 问题原因
PlaceholderResourceGenerator.cs脚本中的`GenerateAudioPlaceholders()`方法创建了空的WAV文件（0字节）。当Unity尝试导入这些空的WAV文件时，FMOD音频系统无法解析它们，导致错误。

## 解决方案

### 方案1：使用新的WAV文件生成器（推荐）
1. 在Unity编辑器中，点击菜单 **"Tools" -> "生成WAV音频占位文件"**
2. 这将生成有效的WAV文件（包含正确的文件头），Unity可以正常导入

### 方案2：手动删除并重新生成
1. 删除 `Assets/Resources/Audio/` 目录下的所有 `.wav` 文件
2. 在Unity编辑器中，点击菜单 **"Tools" -> "生成占位资源" -> "生成音频占位资源"**
3. 脚本已修改为生成有效的WAV文件头

## 技术细节

### WAV文件结构
修复后的脚本会生成包含完整文件头的WAV文件：
- **RIFF头**：标识文件格式
- **fmt块**：定义音频格式（PCM、单声道、22050Hz、8位）
- **data块**：音频数据（静音）

### 文件大小
生成的WAV文件大小为44字节（仅包含文件头），这是一个有效的静音音频文件。

## 验证修复
1. 生成WAV文件后，检查文件大小是否为44字节（而不是0字节）
2. 在Unity中刷新资源（Assets -> Refresh）
3. 检查Console窗口是否还有FSBTool错误

## 注意事项
- 这些是占位音频文件，实际游戏需要替换为真实的音频文件
- 生成的音频是静音的，不会播放任何声音
- 文件格式兼容Unity的音频系统

## 相关文件
- `Assets/Scripts/TestWebGL/Editor/PlaceholderResourceGenerator.cs` - 已修改
- `Assets/Scripts/TestWebGL/Editor/WavFileGenerator.cs` - 新增
- `Assets/Resources/Audio/*.wav` - 音频占位文件

---
**修复时间**：2026-03-31  
**修复状态**：✅ 已完成