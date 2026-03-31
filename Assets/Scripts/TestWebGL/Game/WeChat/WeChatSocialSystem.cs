using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestWebGL.Game.WeChat
{
    /// <summary>
    /// 微信社交系统
    /// 负责微信好友、排行榜、群聊等社交功能
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

        // 社交配置
        private const string RANKING_KEY = "game_ranking";
        private const string FRIEND_DATA_KEY = "friend_data";
        
        // 初始化状态
        private bool _isInitialized = false;
        
        // 好友数据
        private List<WeChatFriendData> _friends = new List<WeChatFriendData>();
        
        // 排行榜数据
        private List<WeChatRankingData> _ranking = new List<WeChatRankingData>();

        // 事件
        public event Action<bool, string> OnFriendsLoaded;
        public event Action<bool, string> OnRankingLoaded;
        public event Action<bool, string> OnFriendDataSaved;
        public event Action<WeChatFriendData> OnFriendAdded;
        public event Action<WeChatRankingData> OnRankingUpdated;

        /// <summary>
        /// 初始化微信社交系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[WeChatSocial] 初始化微信社交系统...");

            // 检查微信环境
            if (!WeChatAPI.IsAvailable())
            {
                Debug.LogWarning("[WeChatSocial] 微信环境不可用，社交功能受限");
            }

            _isInitialized = true;
            Debug.Log("[WeChatSocial] 微信社交系统初始化完成");
        }

        /// <summary>
        /// 获取好友数据
        /// </summary>
        public void GetFriends(Action<bool, string, List<WeChatFriendData>> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatSocial] 系统未初始化");
                callback?.Invoke(false, "系统未初始化", null);
                return;
            }

            Debug.Log("[WeChatSocial] 获取好友数据...");

            // 调用微信好友API
            WeChatAPI.CallWeChatAPI("getFriendCloudStorage", $"{{\"keyList\":[\"{FRIEND_DATA_KEY}\"]}}", (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning("[WeChatSocial] 获取好友数据失败");
                    OnFriendsLoaded?.Invoke(false, "获取好友数据失败");
                    callback?.Invoke(false, "获取好友数据失败", null);
                    return;
                }

                try
                {
                    var friendResult = JsonUtility.FromJson<WeChatFriendResult>(result);
                    
                    if (friendResult.success && friendResult.data != null)
                    {
                        _friends = friendResult.data;
                        
                        Debug.Log($"[WeChatSocial] 好友数据获取成功：{_friends.Count}个好友");
                        OnFriendsLoaded?.Invoke(true, "获取成功");
                        callback?.Invoke(true, "获取成功", _friends);
                    }
                    else
                    {
                        Debug.LogWarning("[WeChatSocial] 好友数据为空");
                        OnFriendsLoaded?.Invoke(false, "好友数据为空");
                        callback?.Invoke(false, "好友数据为空", null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatSocial] 好友数据解析失败：{ex.Message}");
                    OnFriendsLoaded?.Invoke(false, "好友数据解析失败");
                    callback?.Invoke(false, "好友数据解析失败", null);
                }
            });
        }

        /// <summary>
        /// 获取群聊数据
        /// </summary>
        public void GetGroupFriends(string shareTicket, Action<bool, string, List<WeChatFriendData>> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatSocial] 系统未初始化");
                callback?.Invoke(false, "系统未初始化", null);
                return;
            }

            Debug.Log($"[WeChatSocial] 获取群聊数据：{shareTicket}");

            // 调用微信群聊API
            WeChatAPI.CallWeChatAPI("getGroupCloudStorage", $"{{\"shareTicket\":\"{shareTicket}\",\"keyList\":[\"{FRIEND_DATA_KEY}\"]}}", (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning("[WeChatSocial] 获取群聊数据失败");
                    callback?.Invoke(false, "获取群聊数据失败", null);
                    return;
                }

                try
                {
                    var friendResult = JsonUtility.FromJson<WeChatFriendResult>(result);
                    
                    if (friendResult.success && friendResult.data != null)
                    {
                        Debug.Log($"[WeChatSocial] 群聊数据获取成功：{friendResult.data.Count}个成员");
                        callback?.Invoke(true, "获取成功", friendResult.data);
                    }
                    else
                    {
                        Debug.LogWarning("[WeChatSocial] 群聊数据为空");
                        callback?.Invoke(false, "群聊数据为空", null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatSocial] 群聊数据解析失败：{ex.Message}");
                    callback?.Invoke(false, "群聊数据解析失败", null);
                }
            });
        }

        /// <summary>
        /// 保存好友数据
        /// </summary>
        public void SaveFriendData(WeChatFriendData friendData, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatSocial] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            Debug.Log($"[WeChatSocial] 保存好友数据：{friendData.nickName}");

            string jsonData = JsonUtility.ToJson(friendData);

            // 调用微信存储API
            WeChatAPI.CallWeChatAPI("setUserCloudStorage", $"{{\"KVDataList\":[{{\"key\":\"{FRIEND_DATA_KEY}\",\"value\":\"{EscapeJsonString(jsonData)}\"}}]}}", (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                if (success)
                {
                    // 更新本地好友列表
                    var existingFriend = _friends.Find(f => f.openId == friendData.openId);
                    if (existingFriend != null)
                    {
                        existingFriend = friendData;
                    }
                    else
                    {
                        _friends.Add(friendData);
                    }
                    
                    Debug.Log($"[WeChatSocial] 好友数据保存成功：{friendData.nickName}");
                    OnFriendAdded?.Invoke(friendData);
                }
                else
                {
                    Debug.LogError($"[WeChatSocial] 好友数据保存失败：{friendData.nickName}");
                }

                OnFriendDataSaved?.Invoke(success, success ? "保存成功" : "保存失败");
                callback?.Invoke(success, success ? "保存成功" : "保存失败");
            });
        }

        /// <summary>
        /// 获取排行榜
        /// </summary>
        public void GetRanking(Action<bool, string, List<WeChatRankingData>> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatSocial] 系统未初始化");
                callback?.Invoke(false, "系统未初始化", null);
                return;
            }

            Debug.Log("[WeChatSocial] 获取排行榜...");

            // 调用微信排行榜API
            WeChatAPI.CallWeChatAPI("getFriendCloudStorage", $"{{\"keyList\":[\"{RANKING_KEY}\"]}}", (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning("[WeChatSocial] 获取排行榜失败");
                    OnRankingLoaded?.Invoke(false, "获取排行榜失败");
                    callback?.Invoke(false, "获取排行榜失败", null);
                    return;
                }

                try
                {
                    var rankingResult = JsonUtility.FromJson<WeChatRankingResult>(result);
                    
                    if (rankingResult.success && rankingResult.data != null)
                    {
                        _ranking = rankingResult.data;
                        
                        // 按分数排序
                        _ranking.Sort((a, b) => b.score.CompareTo(a.score));
                        
                        // 设置排名
                        for (int i = 0; i < _ranking.Count; i++)
                        {
                            _ranking[i].rank = i + 1;
                        }
                        
                        Debug.Log($"[WeChatSocial] 排行榜获取成功：{_ranking.Count}个玩家");
                        OnRankingLoaded?.Invoke(true, "获取成功");
                        callback?.Invoke(true, "获取成功", _ranking);
                    }
                    else
                    {
                        Debug.LogWarning("[WeChatSocial] 排行榜数据为空");
                        OnRankingLoaded?.Invoke(false, "排行榜数据为空");
                        callback?.Invoke(false, "排行榜数据为空", null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatSocial] 排行榜数据解析失败：{ex.Message}");
                    OnRankingLoaded?.Invoke(false, "排行榜数据解析失败");
                    callback?.Invoke(false, "排行榜数据解析失败", null);
                }
            });
        }

        /// <summary>
        /// 更新排行榜分数
        /// </summary>
        public void UpdateRankingScore(string playerId, string playerName, int score, string avatarUrl = null, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatSocial] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            Debug.Log($"[WeChatSocial] 更新排行榜分数：{playerName} - {score}分");

            var rankingData = new WeChatRankingData
            {
                playerId = playerId,
                playerName = playerName,
                score = score,
                avatarUrl = avatarUrl,
                updateTime = DateTime.Now
            };

            string jsonData = JsonUtility.ToJson(rankingData);

            // 调用微信存储API
            WeChatAPI.CallWeChatAPI("setUserCloudStorage", $"{{\"KVDataList\":[{{\"key\":\"{RANKING_KEY}\",\"value\":\"{EscapeJsonString(jsonData)}\"}}]}}", (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                if (success)
                {
                    // 更新本地排行榜
                    var existingRanking = _ranking.Find(r => r.playerId == playerId);
                    if (existingRanking != null)
                    {
                        if (score > existingRanking.score)
                        {
                            existingRanking.score = score;
                            existingRanking.updateTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        _ranking.Add(rankingData);
                    }
                    
                    // 重新排序
                    _ranking.Sort((a, b) => b.score.CompareTo(a.score));
                    for (int i = 0; i < _ranking.Count; i++)
                    {
                        _ranking[i].rank = i + 1;
                    }
                    
                    Debug.Log($"[WeChatSocial] 排行榜分数更新成功：{playerName}");
                    OnRankingUpdated?.Invoke(rankingData);
                }
                else
                {
                    Debug.LogError($"[WeChatSocial] 排行榜分数更新失败：{playerName}");
                }

                callback?.Invoke(success, success ? "更新成功" : "更新失败");
            });
        }

        /// <summary>
        /// 获取玩家排名
        /// </summary>
        public WeChatRankingData GetPlayerRank(string playerId)
        {
            return _ranking.Find(r => r.playerId == playerId);
        }

        /// <summary>
        /// 获取前N名玩家
        /// </summary>
        public List<WeChatRankingData> GetTopRanking(int count = 10)
        {
            return _ranking.GetRange(0, Math.Min(count, _ranking.Count));
        }

        /// <summary>
        /// 获取好友列表
        /// </summary>
        public List<WeChatFriendData> GetFriendsList()
        {
            return new List<WeChatFriendData>(_friends);
        }

        /// <summary>
        /// 获取排行榜列表
        /// </summary>
        public List<WeChatRankingData> GetRankingList()
        {
            return new List<WeChatRankingData>(_ranking);
        }

        /// <summary>
        /// 转义JSON字符串
        /// </summary>
        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            
            return str.Replace("\\", "\\\\")
                     .Replace("\"", "\\\"")
                     .Replace("\n", "\\n")
                     .Replace("\r", "\\r")
                     .Replace("\t", "\\t");
        }

        /// <summary>
        /// 获取社交统计信息
        /// </summary>
        public string GetSocialInfo()
        {
            return $"好友：{_friends.Count}人, 排行榜：{_ranking.Count}人, " +
                   $"微信环境：{(WeChatAPI.IsAvailable() ? "可用" : "不可用")}";
        }

        /// <summary>
        /// 清除社交数据
        /// </summary>
        public void ClearSocialData()
        {
            _friends.Clear();
            _ranking.Clear();
            Debug.Log("[WeChatSocial] 社交数据已清除");
        }
    }

    /// <summary>
    /// 微信好友数据
    /// </summary>
    [System.Serializable]
    public class WeChatFriendData
    {
        public string openId;
        public string nickName;
        public string avatarUrl;
        public int gender;
        public string city;
        public string province;
        public string country;
        public int level;
        public int score;
        public DateTime lastLoginTime;
    }

    /// <summary>
    /// 微信排行榜数据
    /// </summary>
    [System.Serializable]
    public class WeChatRankingData
    {
        public string playerId;
        public string playerName;
        public string avatarUrl;
        public int score;
        public int rank;
        public DateTime updateTime;
    }

    /// <summary>
    /// 微信好友结果
    /// </summary>
    [System.Serializable]
    public class WeChatFriendResult
    {
        public bool success;
        public List<WeChatFriendData> data;
        public string error;
    }

    /// <summary>
    /// 微信排行榜结果
    /// </summary>
    [System.Serializable]
    public class WeChatRankingResult
    {
        public bool success;
        public List<WeChatRankingData> data;
        public string error;
    }
}