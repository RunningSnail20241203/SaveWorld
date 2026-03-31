using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestWebGL.Game.Social
{
    /// <summary>
    /// 社交系统
    /// 负责好友、排行榜、分享等社交功能
    /// </summary>
    public class SocialSystem : MonoBehaviour
    {
        private static SocialSystem s_instance;
        public static SocialSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("SocialSystem");
                    s_instance = go.AddComponent<SocialSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 社交配置
        private const string FRIENDS_KEY = "social_friends";
        private const string RANKING_KEY = "social_ranking";
        private const int MAX_FRIENDS = 50;

        // 好友列表
        private List<FriendData> _friends = new List<FriendData>();
        
        // 排行榜数据
        private List<RankingEntry> _ranking = new List<RankingEntry>();
        
        // 初始化状态
        private bool _isInitialized = false;

        // 事件
        public event Action<bool, string> OnFriendAdded;
        public event Action<bool, string> OnFriendRemoved;
        public event Action<List<RankingEntry>> OnRankingUpdated;
        public event Action<bool, string> OnShareCompleted;

        /// <summary>
        /// 初始化社交系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[Social] 初始化社交系统...");

            // 加载本地数据
            LoadLocalData();

            _isInitialized = true;
            Debug.Log("[Social] 社交系统初始化完成");
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        public void AddFriend(string friendId, string friendName, string friendAvatar = null)
        {
            if (!_isInitialized) return;

            // 检查是否已经是好友
            if (_friends.Exists(f => f.friendId == friendId))
            {
                OnFriendAdded?.Invoke(false, "该玩家已经是您的好友");
                return;
            }

            // 检查好友数量限制
            if (_friends.Count >= MAX_FRIENDS)
            {
                OnFriendAdded?.Invoke(false, $"好友数量已达上限({MAX_FRIENDS})");
                return;
            }

            // 添加好友
            var friend = new FriendData
            {
                friendId = friendId,
                friendName = friendName,
                friendAvatar = friendAvatar,
                addTime = DateTime.Now,
                isOnline = false,
                lastOnlineTime = DateTime.Now
            };

            _friends.Add(friend);
            SaveLocalData();

            OnFriendAdded?.Invoke(true, "好友添加成功");
            Debug.Log($"[Social] 添加好友: {friendName}");
        }

        /// <summary>
        /// 移除好友
        /// </summary>
        public void RemoveFriend(string friendId)
        {
            if (!_isInitialized) return;

            var friend = _friends.Find(f => f.friendId == friendId);
            if (friend == null)
            {
                OnFriendRemoved?.Invoke(false, "该玩家不是您的好友");
                return;
            }

            _friends.Remove(friend);
            SaveLocalData();

            OnFriendRemoved?.Invoke(true, "好友移除成功");
            Debug.Log($"[Social] 移除好友: {friend.friendName}");
        }

        /// <summary>
        /// 获取好友列表
        /// </summary>
        public List<FriendData> GetFriends()
        {
            return new List<FriendData>(_friends);
        }

        /// <summary>
        /// 获取在线好友
        /// </summary>
        public List<FriendData> GetOnlineFriends()
        {
            return _friends.FindAll(f => f.isOnline);
        }

        /// <summary>
        /// 更新好友状态
        /// </summary>
        public void UpdateFriendStatus(string friendId, bool isOnline)
        {
            var friend = _friends.Find(f => f.friendId == friendId);
            if (friend != null)
            {
                friend.isOnline = isOnline;
                friend.lastOnlineTime = DateTime.Now;
                SaveLocalData();
            }
        }

        /// <summary>
        /// 赠送体力给好友
        /// </summary>
        public void SendStaminaToFriend(string friendId, int amount)
        {
            if (!_isInitialized) return;

            var friend = _friends.Find(f => f.friendId == friendId);
            if (friend == null)
            {
                Debug.LogWarning($"[Social] 好友不存在: {friendId}");
                return;
            }

            // 这里实现赠送体力的逻辑
            // 可以调用微信API或自建服务器
            
            Debug.Log($"[Social] 向好友 {friend.friendName} 赠送 {amount} 体力");
        }

        /// <summary>
        /// 请求好友赠送体力
        /// </summary>
        public void RequestStaminaFromFriend(string friendId)
        {
            if (!_isInitialized) return;

            var friend = _friends.Find(f => f.friendId == friendId);
            if (friend == null)
            {
                Debug.LogWarning($"[Social] 好友不存在: {friendId}");
                return;
            }

            // 这里实现请求体力的逻辑
            Debug.Log($"[Social] 向好友 {friend.friendName} 请求体力");
        }

        /// <summary>
        /// 更新排行榜
        /// </summary>
        public void UpdateRanking(string playerId, string playerName, int score, string avatar = null)
        {
            if (!_isInitialized) return;

            // 查找现有记录
            var existingEntry = _ranking.Find(r => r.playerId == playerId);
            if (existingEntry != null)
            {
                // 更新分数（只保留最高分）
                if (score > existingEntry.score)
                {
                    existingEntry.score = score;
                    existingEntry.updateTime = DateTime.Now;
                }
            }
            else
            {
                // 添加新记录
                var entry = new RankingEntry
                {
                    playerId = playerId,
                    playerName = playerName,
                    avatar = avatar,
                    score = score,
                    rank = 0,
                    updateTime = DateTime.Now
                };
                _ranking.Add(entry);
            }

            // 排序并更新排名
            _ranking.Sort((a, b) => b.score.CompareTo(a.score));
            for (int i = 0; i < _ranking.Count; i++)
            {
                _ranking[i].rank = i + 1;
            }

            SaveLocalData();
            OnRankingUpdated?.Invoke(_ranking);

            Debug.Log($"[Social] 更新排行榜: {playerName} - {score}分");
        }

        /// <summary>
        /// 获取排行榜
        /// </summary>
        public List<RankingEntry> GetRanking(int count = 10)
        {
            return _ranking.GetRange(0, Math.Min(count, _ranking.Count));
        }

        /// <summary>
        /// 获取玩家排名
        /// </summary>
        public RankingEntry GetPlayerRank(string playerId)
        {
            return _ranking.Find(r => r.playerId == playerId);
        }

        /// <summary>
        /// 分享游戏
        /// </summary>
        public void ShareGame(string title, string description, string imageUrl = null)
        {
            if (!_isInitialized) return;

            // 调用微信分享API
            // WX.shareAppMessage({
            //     title: title,
            //     desc: description,
            //     imageUrl: imageUrl
            // });

            Debug.Log($"[Social] 分享游戏: {title}");
            OnShareCompleted?.Invoke(true, "分享成功");
        }

        /// <summary>
        /// 分享成就
        /// </summary>
        public void ShareAchievement(string achievementTitle, string achievementDescription)
        {
            string title = $"我在《末世生存合成》中解锁了成就：{achievementTitle}";
            string description = achievementDescription;
            ShareGame(title, description);
        }

        /// <summary>
        /// 分享排行榜
        /// </summary>
        public void ShareRanking(int rank, int score)
        {
            string title = $"我在《末世生存合成》排行榜中排名第{rank}名！";
            string description = $"我的分数是{score}分，快来挑战我吧！";
            ShareGame(title, description);
        }

        /// <summary>
        /// 邀请好友
        /// </summary>
        public void InviteFriend(string inviteMessage)
        {
            if (!_isInitialized) return;

            // 调用微信邀请API
            // WX.shareAppMessage({
            //     title: '来和我一起玩《末世生存合成》吧！',
            //     desc: inviteMessage,
            //     query: 'invite=true'
            // });

            Debug.Log("[Social] 邀请好友");
        }

        /// <summary>
        /// 处理邀请
        /// </summary>
        public void HandleInvite(string inviterId, string inviterName)
        {
            Debug.Log($"[Social] 收到邀请: {inviterName} ({inviterId})");
            
            // 可以在这里添加邀请奖励逻辑
        }

        /// <summary>
        /// 保存本地数据
        /// </summary>
        private void SaveLocalData()
        {
            try
            {
                // 保存好友列表
                string friendsJson = JsonUtility.ToJson(new FriendListWrapper { friends = _friends });
                PlayerPrefs.SetString(FRIENDS_KEY, friendsJson);

                // 保存排行榜
                string rankingJson = JsonUtility.ToJson(new RankingWrapper { ranking = _ranking });
                PlayerPrefs.SetString(RANKING_KEY, rankingJson);

                PlayerPrefs.Save();
                Debug.Log("[Social] 本地数据保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Social] 保存本地数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载本地数据
        /// </summary>
        private void LoadLocalData()
        {
            try
            {
                // 加载好友列表
                if (PlayerPrefs.HasKey(FRIENDS_KEY))
                {
                    string friendsJson = PlayerPrefs.GetString(FRIENDS_KEY);
                    var wrapper = JsonUtility.FromJson<FriendListWrapper>(friendsJson);
                    if (wrapper != null && wrapper.friends != null)
                    {
                        _friends = wrapper.friends;
                    }
                }

                // 加载排行榜
                if (PlayerPrefs.HasKey(RANKING_KEY))
                {
                    string rankingJson = PlayerPrefs.GetString(RANKING_KEY);
                    var wrapper = JsonUtility.FromJson<RankingWrapper>(rankingJson);
                    if (wrapper != null && wrapper.ranking != null)
                    {
                        _ranking = wrapper.ranking;
                    }
                }

                Debug.Log("[Social] 本地数据加载成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Social] 加载本地数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取社交统计信息
        /// </summary>
        public string GetSocialInfo()
        {
            int onlineFriends = _friends.FindAll(f => f.isOnline).Count;
            return $"好友: {_friends.Count}人 (在线: {onlineFriends}人), 排行榜: {_ranking.Count}人";
        }

        /// <summary>
        /// 清除所有社交数据
        /// </summary>
        public void ClearAllData()
        {
            _friends.Clear();
            _ranking.Clear();
            PlayerPrefs.DeleteKey(FRIENDS_KEY);
            PlayerPrefs.DeleteKey(RANKING_KEY);
            PlayerPrefs.Save();
            Debug.Log("[Social] 所有社交数据已清除");
        }
    }

    /// <summary>
    /// 好友数据
    /// </summary>
    [System.Serializable]
    public class FriendData
    {
        public string friendId;
        public string friendName;
        public string friendAvatar;
        public DateTime addTime;
        public bool isOnline;
        public DateTime lastOnlineTime;
    }

    /// <summary>
    /// 排行榜条目
    /// </summary>
    [System.Serializable]
    public class RankingEntry
    {
        public string playerId;
        public string playerName;
        public string avatar;
        public int score;
        public int rank;
        public DateTime updateTime;
    }

    /// <summary>
    /// 好友列表包装类（用于JSON序列化）
    /// </summary>
    [System.Serializable]
    public class FriendListWrapper
    {
        public List<FriendData> friends;
    }

    /// <summary>
    /// 排行榜包装类（用于JSON序列化）
    /// </summary>
    [System.Serializable]
    public class RankingWrapper
    {
        public List<RankingEntry> ranking;
    }
}