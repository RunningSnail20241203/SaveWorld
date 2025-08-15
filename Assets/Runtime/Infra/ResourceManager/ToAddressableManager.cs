using System;
using GameFramework.Resource;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SaveWorld
{
    public class ToAddressableManager : ICustomResourceManager
    {
        public void ToLoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, int priority, object userData)
        {
            var handle = Addressables.LoadSceneAsync(sceneAssetName);
            loadSceneCallbacks?.LoadSceneUpdateCallback?.Invoke(sceneAssetName, 0f, handle);
            handle.Completed += _ =>
            {
                if (!handle.IsValid())
                {
                    loadSceneCallbacks?.LoadSceneFailureCallback?.Invoke(sceneAssetName, LoadResourceStatus.AssetError, $"{sceneAssetName} : handle is invalid", userData);
                }else if (handle.Status == AsyncOperationStatus.Failed)
                {
                    Addressables.Release(handle);
                    loadSceneCallbacks?.LoadSceneFailureCallback?.Invoke(sceneAssetName, LoadResourceStatus.AssetError, $"{sceneAssetName} : status is failed", userData);
                }
                loadSceneCallbacks?.LoadSceneSuccessCallback(sceneAssetName, -1, userData);
            };
        }

        public void ToUnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            // Addressables.UnloadSceneAsync()
        }

        public void ToLoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks,
            object userData)
        {
            throw new NotImplementedException();
        }

        public void ToUnloadAsset(object asset)
        {
            throw new NotImplementedException();
        }

        public void LaunchAsset(Action complet)
        {
            throw new NotImplementedException();
        }
    }
}