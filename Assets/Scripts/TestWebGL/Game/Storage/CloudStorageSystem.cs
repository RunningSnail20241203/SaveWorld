using UnityEngine;

namespace TestWebGL.Game.Storage
{
    /// <summary>
    /// 云存储系统 V2
    /// </summary>
    public class CloudStorageSystem
    {
        private static CloudStorageSystem s_instance;

        public static CloudStorageSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new CloudStorageSystem();
                }
                return s_instance;
            }
        }

        public void Initialize()
        {
            Debug.Log("[CloudStorageSystem] V2 初始化...");
            // TODO: V2 云存储系统实现
        }
    }
}