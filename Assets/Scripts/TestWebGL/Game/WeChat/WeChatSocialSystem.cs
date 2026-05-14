using System;
using System.Collections.Generic;
using UnityEngine;
using WeChatWASM;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信社交系统 - 排行榜、开放数据域
    /// 基于 WX.GetRankManager / WX.SetUserCloudStorage
    /// 好友排行数据通过开放数据域渲染，此处只做分数上传
    /// </summary>
    public class WeChatSocialSystem : MonoBehaviour
    {
        private static WeChatSocialSystem s_instance;
        public static WeChatSocialSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("WeChatSocialSystem");
                    s_instance = go.AddComponent<WeChatSocialSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        private string _rankingKey = "game_score";
        private WXRankManager _rankManager;

        /// <summary>
        /// 从 WeChatConfigManager 加载社交配置
        /// </summary>
        public void LoadConfig()
        {
            _rankingKey = WeChatConfigManager.RankingKey;
            Debug.Log($"[WeChatSocial] 配置加载完成: rankingKey={_rankingKey}");
        }

        /// <summary>
        /// 更新自己的分数到云存储（用于排行榜）
        /// </summary>
        public void UpdateScore(int score, Action<bool, string> callback = null)
        {
            Debug.Log($"[WeChatSocial] 更新分数: {score}");

            WX.SetUserCloudStorage(new SetUserCloudStorageOption
            {
                KVDataList = new KVData[]
                {
                    new KVData { key = _rankingKey, value = score.ToString() }
                },
                success = (res) =>
                {
                    Debug.Log("[WeChatSocial] 分数更新成功");
                    callback?.Invoke(true, "更新成功");
                },
                fail = (res) =>
                {
                    Debug.LogError($"[WeChatSocial] 分数更新失败: {res.errMsg}");
                    callback?.Invoke(false, res.errMsg);
                }
            });
        }

        /// <summary>
        /// 获取开放数据域（用于在 Canvas 上渲染好友排行榜 UI）
        /// 好友排行数据通过开放数据域内的 JS 代码获取，此处只提供 Canvas 占位纹理
        /// </summary>
        public WXOpenDataContext GetOpenDataContext()
        {
            return WX.GetOpenDataContext();
        }

        /// <summary>
        /// 显示开放数据域（通过纹理占位）
        /// </summary>
        public void ShowOpenData(Texture texture, int x, int y, int width, int height)
        {
            WX.ShowOpenData(texture, x, y, width, height);
        }

        /// <summary>
        /// 隐藏开放数据域
        /// </summary>
        public void HideOpenData()
        {
            WX.HideOpenData();
        }

        public string GetSocialInfo()
        {
            return "社交系统就绪";
        }
    }
}
