using System.Collections.Generic;
using System.Linq;
using System.Text;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Installer;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.HybridCLR
{
    public static class BuildPlayerCommand
    {
        private static readonly List<string> ExcludeHotUpdateAssemblies = new() // 不需要热更的Assembly
        {
            "HybridLauncher",
        };

        [MenuItem("Build/FullPackage/打完整包")]
        public static void BuildFullPlayer()
        {
            // 尝试安装HybridClr
            TryInstallHybridClr();

            // 需要热更的Assembly添加到HybridClrSetting中
            AddHotUpdateAssembliesToHybridClrSetting();

            return;
            // 生成所有HybridClr资源
            PrebuildCommand.GenerateAll();

            var activeTarget = EditorUserBuildSettings.activeBuildTarget;

            // 拷贝dll
            BuildAssetsCommand.CopyHotUpdateDlls();

            // 打Addressable
            BuildAssetsCommand.BuildAddressable(activeTarget);

            // 打player
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray(),
                target = activeTarget,
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError("打包失败");
            }
        }

        private static void TryInstallHybridClr()
        {
            Debug.Log("尝试安装HybridClr");
            var ic = new InstallerController();
            if (!ic.HasInstalledHybridCLR())
            {
                ic.InstallDefaultHybridCLR();
                Debug.Log("安装HybridClr 完毕");
            }
            else
            {
                Debug.Log("已经 安装HybridClr");
            }
        }

        private static void AddHotUpdateAssembliesToHybridClrSetting()
        {
            var gs = SettingsUtil.HybridCLRSettings;
            // 找到所有的AssemblyDefinitionAsset
            var guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets" });
            Debug.Log($"找到{guids.Length}个AssemblyDefinitionAsset");

            gs.hotUpdateAssemblyDefinitions = guids
                .Select(AssetDatabase.GUIDToAssetPath) // 转换为路径
                .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>) // 加载Asset
                .Where(assemblyDefinitionAsset => !ExcludeHotUpdateAssemblies.Contains(assemblyDefinitionAsset.name)) // 过滤掉不需要的
                .ToArray(); // 转换为数组
            HybridCLRSettings.Save();
            
            Debug.Log($"添加{gs.hotUpdateAssemblyDefinitions.Length}个AssemblyDefinitionAsset到HybridClrSetting");
            var sd = new StringBuilder();
            foreach (var assemblyDefinitionAsset in gs.hotUpdateAssemblyDefinitions)
            {
                sd.AppendLine(assemblyDefinitionAsset.name);
            }
            Debug.Log($"添加的AssemblyDefinitionAsset:\n{sd}");
        }
    }
}