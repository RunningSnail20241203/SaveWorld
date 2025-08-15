using GameFramework.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine;


public class ToAddressableManager : ICustomResourceManager
{ 
    private Dictionary<object, Queue<IEnumerator>> assetMap = new Dictionary<object, Queue<IEnumerator>>();

    public void ToLoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, int priority, object userData)
    {
        //AsyncOperationHandle<SceneInstance>
        var handle =  Addressables.LoadSceneAsync(sceneAssetName);

        //由于addressable没有加载进度事件
        //userData参数传递加载场景句柄，根据该句柄显示加载进度
        //  var percentage = handle.GetDownloadStatus().Percent;
        loadSceneCallbacks.LoadSceneUpdateCallback(sceneAssetName, 0, handle);

        handle.Completed += resource =>
        {
            if(resource.Status == AsyncOperationStatus.Succeeded)
            {
                loadSceneCallbacks.LoadSceneSuccessCallback(sceneAssetName, 0, userData);
                if (!assetMap.ContainsKey(sceneAssetName))
                {
                    Queue<IEnumerator> assetObjects = new Queue<IEnumerator>();
                    assetObjects.Enqueue(handle);
                    assetMap.Add(sceneAssetName, assetObjects);
                }
                else
                {
                    assetMap[sceneAssetName].Enqueue(handle);
                }
            }
            else
            {
                loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, LoadResourceStatus.NotExist, "", userData);
            }

        };

    } 


    public void ToUnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
    {
        if (assetMap.TryGetValue(sceneAssetName, out Queue<IEnumerator> assetObjects))
        {
            var handle = (AsyncOperationHandle<SceneInstance>) assetObjects.Dequeue();
            var unloadasync = Addressables.UnloadSceneAsync(handle);

            unloadasync.Completed += resource =>
            {
                if(resource.Status == AsyncOperationStatus.Succeeded)
                {
                    unloadSceneCallbacks.UnloadSceneSuccessCallback(sceneAssetName, userData);
                }
                else
                {
                     unloadSceneCallbacks.UnloadSceneFailureCallback(sceneAssetName, userData);
                }
            };

            if (assetObjects.Count == 0)
            {
                assetMap.Remove(sceneAssetName);
            }
        }

    }

    public void ToLoadAsset(string assetName, Type assetType, int priority, GameFramework.Resource.LoadAssetCallbacks loadAssetCallbacks, object userData)
    {
        if(assetType == null)
        {
            throw new Exception("加载参数assetType需要传入明确类型！");
        }


        if(assetType == typeof(GameObject))
        {
                RedirectHandle<GameObject>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(Texture))
        {
                RedirectHandle<Texture>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(Texture2D))
        {
                RedirectHandle<Texture2D>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(AudioClip))
        {
                RedirectHandle<AudioClip>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(Texture3D))
        {
                RedirectHandle<Texture3D>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(AnimationClip))
        {
                RedirectHandle<AnimationClip>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(TextAsset))
        {
                RedirectHandle<TextAsset>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(Material))
        {
                RedirectHandle<Material>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(Shader))
        {
                RedirectHandle<Shader>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(ShaderVariantCollection))
        {
                RedirectHandle<ShaderVariantCollection>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(Mesh))
        {
                RedirectHandle<Mesh>(assetName, priority, loadAssetCallbacks, userData);
        }
        if(assetType == typeof(TextMesh))
        {
                RedirectHandle<TextMesh>(assetName, priority, loadAssetCallbacks, userData);
        }


        throw new Exception("未找到该资源类型，请在这里自己加：" + assetType);
    }

    public void ToUnloadAsset(object asset)
    {
        if (assetMap.TryGetValue(asset, out Queue<IEnumerator> assetObjects))
        {
             var handle = assetObjects.Dequeue();
            Addressables.Release(handle);
            if(assetObjects.Count == 0)
            {
                assetMap.Remove(asset);
            }
        }
    }

    //重定向加载句柄，Addressable加载只支持明确类型
    private void RedirectHandle<TObject>(string assetName, int priority, GameFramework.Resource.LoadAssetCallbacks loadAssetCallbacks, object userData) where TObject : UnityEngine.Object
    {
        AsyncOperationHandle<TObject> handle =  Addressables.LoadAssetAsync<TObject>(assetName);
         //由于addressable没有加载进度事件
        //userData参数传递加载句柄，根据该句柄显示加载进度
        //  var percentage = handle.GetDownloadStatus().Percent;
        loadAssetCallbacks.LoadAssetUpdateCallback(assetName, 0, handle);

        handle.Completed += resource =>
        {
            if(resource.Status == AsyncOperationStatus.Succeeded)
            {
                loadAssetCallbacks.LoadAssetSuccessCallback(assetName, handle.Result, 0, userData);
                if (!assetMap.ContainsKey(handle.Result))
                {
                    Queue<IEnumerator> assetObjects= new Queue<IEnumerator>();
                    assetObjects.Enqueue(resource);
                    assetMap.Add(handle.Result, assetObjects);
                }
                else
                {
                    assetMap[handle.Result].Enqueue(handle);
                }
            }
            else
            {
                loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.AssetError, "", userData);
            }
        };
    }

    public void LaunchAsset(Action complet)
    {
       
        var handle = Addressables.InitializeAsync();
        handle.Completed += _ =>
        {
           complet?.Invoke();
        };

    }

}
