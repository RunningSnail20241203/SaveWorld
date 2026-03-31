using UnityEngine;
using TestWebGL.Game.UI;

namespace TestWebGL.Game.Test
{
    /// <summary>
    /// GridUI修复测试脚本
    /// 用于验证cellPrefab自动加载是否正常工作
    /// </summary>
    public class GridUIFixTest : MonoBehaviour
    {
        [Header("测试设置")]
        public bool runTestOnStart = true;
        
        private GridUI _gridUI;
        
        void Start()
        {
            if (runTestOnStart)
            {
                TestGridUIFix();
            }
        }
        
        [ContextMenu("测试GridUI修复")]
        public void TestGridUIFix()
        {
            Debug.Log("=== 开始测试GridUI修复 ===");
            
            // 创建GridUI组件
            GameObject gridUIGO = new GameObject("TestGridUI");
            gridUIGO.transform.SetParent(transform);
            _gridUI = gridUIGO.AddComponent<GridUI>();
            
            // 测试Initialize方法
            Debug.Log("调用GridUI.Initialize()...");
            _gridUI.Initialize();
            
            // 检查是否成功
            if (_gridUI.gridContainer != null)
            {
                Debug.Log("✅ GridUI初始化成功！gridContainer已创建");
                
                // 检查格子数量
                int cellCount = _gridUI.gridContainer.childCount;
                Debug.Log($"格子数量: {cellCount}");
                
                if (cellCount == 63) // 9行×7列
                {
                    Debug.Log("✅ 格子数量正确（63个）");
                }
                else
                {
                    Debug.LogWarning($"⚠️ 格子数量不正确，期望63个，实际{cellCount}个");
                }
            }
            else
            {
                Debug.LogError("❌ GridUI初始化失败！gridContainer为null");
            }
            
            Debug.Log("=== GridUI修复测试完成 ===");
        }
        
        void OnDestroy()
        {
            if (_gridUI != null)
            {
                Destroy(_gridUI.gameObject);
            }
        }
    }
}