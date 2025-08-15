using GameFramework;
using GameFramework.Download;
using GameFramework.FileSystem;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using System;
using System.Collections.Generic;

namespace SaveWorld
{
    public class GFAdapterResourceManager : IResourceManager
    {
        #region Interface Override
        public string ReadOnlyPath { get; }
        public string ReadWritePath { get; }
        public ResourceMode ResourceMode { get; }
        public string CurrentVariant { get; }
        public PackageVersionListSerializer PackageVersionListSerializer { get; }
        public UpdatableVersionListSerializer UpdatableVersionListSerializer { get; }
        public ReadOnlyVersionListSerializer ReadOnlyVersionListSerializer { get; }
        public ReadWriteVersionListSerializer ReadWriteVersionListSerializer { get; }
        public ResourcePackVersionListSerializer ResourcePackVersionListSerializer { get; }
        public string ApplicableGameVersion { get; }
        public int InternalResourceVersion { get; }
        public int AssetCount { get; }
        public int ResourceCount { get; }
        public int ResourceGroupCount { get; }
        public string UpdatePrefixUri { get; set; }
        public int GenerateReadWriteVersionListLength { get; set; }
        public string ApplyingResourcePackPath { get; }
        public int ApplyWaitingCount { get; }
        public int UpdateRetryCount { get; set; }
        public IResourceGroup UpdatingResourceGroup { get; }
        public int UpdateWaitingCount { get; }
        public int UpdateWaitingWhilePlayingCount { get; }
        public int UpdateCandidateCount { get; }
        public int LoadTotalAgentCount { get; }
        public int LoadFreeAgentCount { get; }
        public int LoadWorkingAgentCount { get; }
        public int LoadWaitingTaskCount { get; }
        public float AssetAutoReleaseInterval { get; set; }
        public int AssetCapacity { get; set; }
        public float AssetExpireTime { get; set; }
        public int AssetPriority { get; set; }
        public float ResourceAutoReleaseInterval { get; set; }
        public int ResourceCapacity { get; set; }
        public float ResourceExpireTime { get; set; }
        public int ResourcePriority { get; set; }
        public event EventHandler<ResourceVerifyStartEventArgs> ResourceVerifyStart;
        public event EventHandler<ResourceVerifySuccessEventArgs> ResourceVerifySuccess;
        public event EventHandler<ResourceVerifyFailureEventArgs> ResourceVerifyFailure;
        public event EventHandler<ResourceApplyStartEventArgs> ResourceApplyStart;
        public event EventHandler<ResourceApplySuccessEventArgs> ResourceApplySuccess;
        public event EventHandler<ResourceApplyFailureEventArgs> ResourceApplyFailure;
        public event EventHandler<ResourceUpdateStartEventArgs> ResourceUpdateStart;
        public event EventHandler<ResourceUpdateChangedEventArgs> ResourceUpdateChanged;
        public event EventHandler<ResourceUpdateSuccessEventArgs> ResourceUpdateSuccess;
        public event EventHandler<ResourceUpdateFailureEventArgs> ResourceUpdateFailure;
        public event EventHandler<ResourceUpdateAllCompleteEventArgs> ResourceUpdateAllComplete;
        
         public void SetReadOnlyPath(string readOnlyPath)
        {
            throw new NotImplementedException();
        }

        public void SetReadWritePath(string readWritePath)
        {
            throw new NotImplementedException();
        }

        public void SetResourceMode(ResourceMode resourceMode)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentVariant(string currentVariant)
        {
            throw new NotImplementedException();
        }

        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            throw new NotImplementedException();
        }

        public void SetFileSystemManager(IFileSystemManager fileSystemManager)
        {
            throw new NotImplementedException();
        }

        public void SetDownloadManager(IDownloadManager downloadManager)
        {
            throw new NotImplementedException();
        }

        public void SetDecryptResourceCallback(DecryptResourceCallback decryptResourceCallback)
        {
            throw new NotImplementedException();
        }

        public void SetResourceHelper(IResourceHelper resourceHelper)
        {
            throw new NotImplementedException();
        }

        public void AddLoadResourceAgentHelper(ILoadResourceAgentHelper loadResourceAgentHelper)
        {
            throw new NotImplementedException();
        }

        public void InitResources(InitResourcesCompleteCallback initResourcesCompleteCallback)
        {
            throw new NotImplementedException();
        }

        public CheckVersionListResult CheckVersionList(int latestInternalResourceVersion)
        {
            throw new NotImplementedException();
        }

        public void UpdateVersionList(int versionListLength, int versionListHashCode, int versionListCompressedLength,
            int versionListCompressedHashCode, UpdateVersionListCallbacks updateVersionListCallbacks)
        {
            throw new NotImplementedException();
        }

        public void VerifyResources(int verifyResourceLengthPerFrame,
            VerifyResourcesCompleteCallback verifyResourcesCompleteCallback)
        {
            throw new NotImplementedException();
        }

        public void CheckResources(bool ignoreOtherVariant,
            CheckResourcesCompleteCallback checkResourcesCompleteCallback)
        {
            throw new NotImplementedException();
        }

        public void ApplyResources(string resourcePackPath,
            ApplyResourcesCompleteCallback applyResourcesCompleteCallback)
        {
            throw new NotImplementedException();
        }

        public void UpdateResources(UpdateResourcesCompleteCallback updateResourcesCompleteCallback)
        {
            throw new NotImplementedException();
        }

        public void UpdateResources(string resourceGroupName,
            UpdateResourcesCompleteCallback updateResourcesCompleteCallback)
        {
            throw new NotImplementedException();
        }

        public void StopUpdateResources()
        {
            throw new NotImplementedException();
        }

        public bool VerifyResourcePack(string resourcePackPath)
        {
            throw new NotImplementedException();
        }

        public TaskInfo[] GetAllLoadAssetInfos()
        {
            throw new NotImplementedException();
        }

        public void GetAllLoadAssetInfos(List<TaskInfo> results)
        {
            throw new NotImplementedException();
        }

        public HasAssetResult HasAsset(string assetName)
        {
            throw new NotImplementedException();
        }
        
        public string GetBinaryPath(string binaryAssetName)
        {
            throw new NotImplementedException();
        }

        public bool GetBinaryPath(string binaryAssetName, out bool storageInReadOnly, out bool storageInFileSystem,
            out string relativePath, out string fileName)
        {
            throw new NotImplementedException();
        }

        public int GetBinaryLength(string binaryAssetName)
        {
            throw new NotImplementedException();
        }

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks)
        {
            throw new NotImplementedException();
        }

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadBinaryFromFileSystem(string binaryAssetName)
        {
            throw new NotImplementedException();
        }

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex)
        {
            throw new NotImplementedException();
        }

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int length)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, byte[] buffer, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }

        public int LoadBinarySegmentFromFileSystem(string binaryAssetName, int offset, byte[] buffer, int startIndex,
            int length)
        {
            throw new NotImplementedException();
        }

        public bool HasResourceGroup(string resourceGroupName)
        {
            throw new NotImplementedException();
        }

        public IResourceGroup GetResourceGroup()
        {
            throw new NotImplementedException();
        }

        public IResourceGroup GetResourceGroup(string resourceGroupName)
        {
            throw new NotImplementedException();
        }

        public IResourceGroup[] GetAllResourceGroups()
        {
            throw new NotImplementedException();
        }

        public void GetAllResourceGroups(List<IResourceGroup> results)
        {
            throw new NotImplementedException();
        }

        public IResourceGroupCollection GetResourceGroupCollection(params string[] resourceGroupNames)
        {
            throw new NotImplementedException();
        }

        public IResourceGroupCollection GetResourceGroupCollection(List<string> resourceGroupNames)
        {
            throw new NotImplementedException();
        }

        

        #endregion
       
        public static GFAdapterResourceManager Instance = new GFAdapterResourceManager();
        public bool UseCustomManager => true;

        private ICustomResourceManager m_ToCustomResourceManager;


        public void LaunchCustomAsset(Action complete)
        {
            m_ToCustomResourceManager = new ToAddressableManager();
            // toCustomResourceManager = new ToCatAssetManager();
            //toCustomResourceManager = new ToYooAssetManager();
            //toCustomResourceManager = new ToXAssetManager();

            m_ToCustomResourceManager.LaunchAsset(complete);
        }
        
        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks)
        {
            m_ToCustomResourceManager.ToLoadAsset(assetName, null, 0, loadAssetCallbacks, null);
        }

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks)
        {
            m_ToCustomResourceManager.ToLoadAsset(assetName, assetType, 0, loadAssetCallbacks, null);
        }

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks)
        {
            m_ToCustomResourceManager.ToLoadAsset(assetName, null, priority, loadAssetCallbacks, null);
        }

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            m_ToCustomResourceManager.ToLoadAsset(assetName, null, 0, loadAssetCallbacks, userData);
        }

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks)
        {
            m_ToCustomResourceManager.ToLoadAsset(assetName, assetType, priority, loadAssetCallbacks, null);
        }

        public void LoadAsset(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            m_ToCustomResourceManager.ToLoadAsset(assetName, assetType, 0, loadAssetCallbacks, userData);
        }

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            m_ToCustomResourceManager.ToLoadAsset(assetName, null, priority, loadAssetCallbacks, userData);
        }

        public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks,
            object userData)
        {
            m_ToCustomResourceManager.ToLoadAsset(assetName, assetType, priority, loadAssetCallbacks, userData);
            //throw new NotImplementedException();
        }

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks)
        {
            m_ToCustomResourceManager.ToLoadScene(sceneAssetName, loadSceneCallbacks, 0, null);
        }

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks)
        {
            m_ToCustomResourceManager.ToLoadScene(sceneAssetName, loadSceneCallbacks, priority, null);
        }

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData)
        {
            m_ToCustomResourceManager.ToLoadScene(sceneAssetName, loadSceneCallbacks, 0, userData);
        }

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks,
            object userData)
        {
            m_ToCustomResourceManager.ToLoadScene(sceneAssetName, loadSceneCallbacks, priority, userData);
        }

        public void UnloadAsset(object asset)
        {
            m_ToCustomResourceManager.ToUnloadAsset(asset);
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks)
        {
            m_ToCustomResourceManager.ToUnloadScene(sceneAssetName, unloadSceneCallbacks, null);
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            m_ToCustomResourceManager.ToUnloadScene(sceneAssetName, unloadSceneCallbacks, userData);
        }
        
    }
}