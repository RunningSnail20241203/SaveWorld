using System;
using HybridCLR.Editor.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HybridCLR.Editor;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Editor.HybridCLR
{
    public static class BuildAssetsCommand
    {
        private static string HybridClrDataDir = $"{Application.dataPath[..Application.dataPath.IndexOf("Assets", StringComparison.Ordinal)]}HybridCLRData";

        private static string HotUpdateDllsDir = $"{HybridClrDataDir}/HotUpdateDlls";

        private static string ToRelativeAssetPath(string s)
        {
            return s[s.IndexOf("Assets/", StringComparison.Ordinal)..];
        }

        public static void BuildAddressable(BuildTarget target)
        {
            AddressableAssetSettings.BuildPlayerContent(out var result);
        }

        public static void CopyHotUpdateDlls(BuildTarget target)
        {
            var dllOriginDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            var assemblyNames = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
            foreach (var assemblyName in assemblyNames)
            {
                var oldPath = Path.Combine(dllOriginDir, $"{assemblyName}.dll");
                var newPath = Path.Combine(Application.dataPath, "Runtime/HotUpdateDlls", $"{assemblyName}.dll.bytes");
                File.Copy(oldPath, newPath, true);
                Debug.Log($"copy {oldPath} to {newPath}");
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Build/Test")]
        public static void Test()
        {
            var activeTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildAddressable(activeTarget);
        }
    }
}
