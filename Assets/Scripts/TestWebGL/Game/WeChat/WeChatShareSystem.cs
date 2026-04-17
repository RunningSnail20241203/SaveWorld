using System;
using UnityEngine;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信分享系统
    /// 负责微信分享、邀请好友等功能
    /// </summary>
    public class WeChatShareSystem : MonoBehaviour
    {
        private static WeChatShareSystem s_instance;
        public static WeChatShareSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("WeChatShareSystem");
                    s_instance = go.AddComponent<WeChatShareSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 分享配置
        private const string DEFAULT_SHARE_IMAGE = "share_default.jpg";
        private const string DEFAULT_SHARE_QUERY = "from=share";
        
        // 初始化状态
        private bool _isInitialized = false;
        
        // 分享统计
        private int _shareCount = 0;
        private int _successShareCount = 0;

        // 事件
        public event Action<bool, string> OnShareCompleted;
        public event Action<bool, string> OnShareResult;
        public event Action<string> OnShareCallback;

        /// <summary>
        /// 初始化微信分享系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[WeChatShare] 初始化微信分享系统...");

            // 检查微信环境
            if (!WeChatAPI.IsAvailable())
            {
                Debug.LogWarning("[WeChatShare] 微信环境不可用，分享功能受限");
            }

            // 设置分享回调
            SetupShareCallback();

            _isInitialized = true;
            Debug.Log("[WeChatShare] 微信分享系统初始化完成");
        }

        /// <summary>
        /// 设置分享回调
        /// </summary>
        private void SetupShareCallback()
        {
            // 监听分享成功事件
            WeChatAPI.CallWeChatAPI("onShareAppMessage", null, (result) =>
            {
                Debug.Log($"[WeChatShare] 分享回调：{result}");
                OnShareCallback?.Invoke(result);
                
                // 解析分享结果
                ParseShareResult(result);
            });
        }

        /// <summary>
        /// 分享到朋友
        /// </summary>
        public void ShareToFriend(string title, string description = null, string imageUrl = null, string query = null, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatShare] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            Debug.Log($"[WeChatShare] 分享到朋友：{title}");

            // 构建分享参数
            string shareParams = BuildShareParams(title, description, imageUrl, query);

            // 调用微信分享API
            WeChatAPI.CallWeChatAPI("shareAppMessage", shareParams, (result) =>
            {
                _shareCount++;
                
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                if (success)
                {
                    _successShareCount++;
                    Debug.Log($"[WeChatShare] 分享成功：{title}");
                }
                else
                {
                    Debug.LogWarning($"[WeChatShare] 分享失败：{title}");
                }

                OnShareCompleted?.Invoke(success, success ? "分享成功" : "分享失败");
                callback?.Invoke(success, success ? "分享成功" : "分享失败");
            });
        }

        /// <summary>
        /// 分享到朋友圈
        /// </summary>
        public void ShareToTimeline(string title, string description = null, string imageUrl = null, string query = null, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatShare] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            Debug.Log($"[WeChatShare] 分享到朋友圈：{title}");

            // 构建分享参数
            string shareParams = BuildShareParams(title, description, imageUrl, query);

            // 调用微信朋友圈分享API
            WeChatAPI.CallWeChatAPI("shareTimeline", shareParams, (result) =>
            {
                _shareCount++;
                
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                if (success)
                {
                    _successShareCount++;
                    Debug.Log($"[WeChatShare] 朋友圈分享成功：{title}");
                }
                else
                {
                    Debug.LogWarning($"[WeChatShare] 朋友圈分享失败：{title}");
                }

                OnShareCompleted?.Invoke(success, success ? "分享成功" : "分享失败");
                callback?.Invoke(success, success ? "分享成功" : "分享失败");
            });
        }

        /// <summary>
        /// 分享游戏成就
        /// </summary>
        public void ShareAchievement(string achievementTitle, string achievementDescription, Action<bool, string> callback = null)
        {
            string title = $"我在《末世生存合成》中解锁了成就：{achievementTitle}";
            string description = achievementDescription;
            
            ShareToFriend(title, description, null, "from=achievement", callback);
        }

        /// <summary>
        /// 分享游戏进度
        /// </summary>
        public void ShareProgress(int level, int score, Action<bool, string> callback = null)
        {
            string title = $"我在《末世生存合成》中达到了{level}级，分数{score}分！";
            string description = "快来挑战我吧！";
            
            ShareToFriend(title, description, null, "from=progress", callback);
        }

        /// <summary>
        /// 分享排行榜
        /// </summary>
        public void ShareRanking(int rank, int score, Action<bool, string> callback = null)
        {
            string title = $"我在《末世生存合成》排行榜中排名第{rank}名！";
            string description = $"我的分数是{score}分，快来挑战我吧！";
            
            ShareToFriend(title, description, null, "from=ranking", callback);
        }

        /// <summary>
        /// 邀请好友
        /// </summary>
        public void InviteFriend(string inviteMessage, Action<bool, string> callback = null)
        {
            string title = "来和我一起玩《末世生存合成》吧！";
            string description = inviteMessage;
            
            ShareToFriend(title, description, null, "from=invite", callback);
        }

        /// <summary>
        /// 构建分享参数
        /// </summary>
        private string BuildShareParams(string title, string description, string imageUrl, string query)
        {
            var shareData = new WeChatShareData
            {
                title = title,
                desc = description ?? "末世生存合成游戏",
                imageUrl = imageUrl ?? DEFAULT_SHARE_IMAGE,
                query = query ?? DEFAULT_SHARE_QUERY
            };

            return JsonUtility.ToJson(shareData);
        }

        /// <summary>
        /// 解析分享结果
        /// </summary>
        private void ParseShareResult(string result)
        {
            try
            {
                var shareResult = JsonUtility.FromJson<WeChatShareResult>(result);
                
                if (shareResult.success)
                {
                    _successShareCount++;
                    Debug.Log("[WeChatShare] 分享成功");
                    OnShareResult?.Invoke(true, "分享成功");
                }
                else
                {
                    Debug.LogWarning($"[WeChatShare] 分享失败：{shareResult.error}");
                    OnShareResult?.Invoke(false, shareResult.error);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WeChatShare] 分享结果解析失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新分享菜单
        /// </summary>
        public void UpdateShareMenu(string title, string imageUrl = null, string query = null, Action<bool, string> callback = null)
        {
            Debug.Log("[WeChatShare] 更新分享菜单...");

            string shareParams = BuildShareParams(title, null, imageUrl, query);

            WeChatAPI.CallWeChatAPI("updateShareMenu", shareParams, (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                Debug.Log($"[WeChatShare] 分享菜单更新：{(success ? "成功" : "失败")}");
                callback?.Invoke(success, success ? "更新成功" : "更新失败");
            });
        }

        /// <summary>
        /// 获取分享信息
        /// </summary>
        public void GetShareInfo(string shareTicket, Action<bool, string, string> callback = null)
        {
            Debug.Log($"[WeChatShare] 获取分享信息：{shareTicket}");

            WeChatAPI.CallWeChatAPI("getShareInfo", $"{{\"shareTicket\":\"{shareTicket}\"}}", (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    callback?.Invoke(false, "获取分享信息失败", null);
                    return;
                }

                try
                {
                    var shareInfo = JsonUtility.FromJson<WeChatShareInfo>(result);
                    
                    if (shareInfo.success)
                    {
                        Debug.Log("[WeChatShare] 分享信息获取成功");
                        callback?.Invoke(true, "获取成功", shareInfo.data);
                    }
                    else
                    {
                        Debug.LogWarning($"[WeChatShare] 分享信息获取失败：{shareInfo.error}");
                        callback?.Invoke(false, shareInfo.error, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatShare] 分享信息解析失败：{ex.Message}");
                    callback?.Invoke(false, "分享信息解析失败", null);
                }
            });
        }

        /// <summary>
        /// 获取分享统计信息
        /// </summary>
        public string GetShareInfo()
        {
            return $"分享次数：{_shareCount}, 成功次数：{_successShareCount}, " +
                   $"成功率：{(_shareCount > 0 ? (_successShareCount * 100 / _shareCount) : 0)}%";
        }

        /// <summary>
        /// 重置分享统计
        /// </summary>
        public void ResetShareStats()
        {
            _shareCount = 0;
            _successShareCount = 0;
            Debug.Log("[WeChatShare] 分享统计已重置");
        }
    }

    /// <summary>
    /// 微信分享数据
    /// </summary>
    [System.Serializable]
    public class WeChatShareData
    {
        public string title;
        public string desc;
        public string imageUrl;
        public string query;
    }

    /// <summary>
    /// 微信分享结果
    /// </summary>
    [System.Serializable]
    public class WeChatShareResult
    {
        public bool success;
        public string error;
        public string shareTicket;
    }

    /// <summary>
    /// 微信分享信息
    /// </summary>
    [System.Serializable]
    public class WeChatShareInfo
    {
        public bool success;
        public string data;
        public string error;
    }
}