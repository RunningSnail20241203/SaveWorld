using System;
using HybridCLR.Editor.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;

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
            
        }

        public static void CopyHotUpdateDlls()
        {
            
        }

        [MenuItem("Build/Test")]
        public static void Test()
        {
            Debug.Log(HybridClrDataDir);
            Debug.Log(HotUpdateDllsDir);
        }
    }
}
