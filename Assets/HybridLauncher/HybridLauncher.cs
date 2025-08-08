using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

public class HybridLauncher : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(DownLoadAssets(StartGame));
    }

    private static void StartGame()
    {
        LoadMetadataForAOTAssemblies();

        const string mainScene = "Main";
        Addressables.LoadSceneAsync(mainScene).Completed += _ =>
        {
            Debug.Log($"LoadSceneAsync :{mainScene} Completed");
        };
    }

    #region download assets

    private static readonly Dictionary<string, byte[]> s_AssetDatas = new();

    private static List<string> AOTMetaAssemblyFiles { get; } = new()
    {
        Capacity = 0
    };

    private static IEnumerator DownLoadAssets(Action onDownloadComplete)
    {
        var assets = AOTMetaAssemblyFiles;

        foreach (var asset in assets)
        {
            var dllPath = GetWebRequestPath(asset);
            Debug.Log($"start download asset:{dllPath}");
            var www = UnityWebRequest.Get(dllPath);
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
#else
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
#endif
            else
            {
                // Or retrieve results as binary data
                var assetData = www.downloadHandler.data;
                Debug.Log($"dll:{asset}  size:{assetData.Length}");
                s_AssetDatas[asset] = assetData;
            }
        }

        onDownloadComplete();

        yield break;

        string GetWebRequestPath(string asset)
        {
            var path = $"{Application.streamingAssetsPath}/{asset}";
            if (!path.Contains("://"))
            {
                path = "file://" + path;
            }

            return path;
        }
    }

    #endregion

    #region Load MetaData

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误

        const HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyFiles)
        {
            var dllBytes = s_AssetDatas[aotDllName];
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }

    #endregion
}