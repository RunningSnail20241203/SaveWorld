using System;
using GameFramework.Resource;

namespace SaveWorld
{
    public interface ICustomResourceManager
    {
        void ToLoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, int priority, object userData);

        void ToUnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData);

        void ToLoadAsset(string assetName, Type assetType, int priority,
            GameFramework.Resource.LoadAssetCallbacks loadAssetCallbacks, object userData);

        void ToUnloadAsset(object asset);

        void LaunchAsset(Action complet);
    }
}