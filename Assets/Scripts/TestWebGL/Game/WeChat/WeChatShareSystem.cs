using System;
using UnityEngine;
using WeChatWASM;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信分享系统 - 分享消息、转发、朋友圈等
    /// 基于 WX.ShareAppMessage / WX.OnShareAppMessage
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

        private const string DEFAULT_SHARE_IMAGE = "";
        private int _shareCount = 0;
        private int _successShareCount = 0;
        private bool _shareMenuSet = false;

        public event Action<bool, string> OnShareCompleted;

        /// <summary>
        /// 初始化分享系统 - 设置转发监听和分享菜单
        /// </summary>
        public void Initialize(string title = "末世生存合成", string imageUrl = "")
        {
            if (_shareMenuSet) return;

            var shareParam = new WXShareAppMessageParam
            {
                title = title,
                imageUrl = imageUrl
            };

            // 监听右上角转发按钮
            WX.OnShareAppMessage(shareParam, (setShareParam) =>
            {
                setShareParam(new WXShareAppMessageParam
                {
                    title = title,
                    imageUrl = imageUrl,
                    query = "from=share"
                });
            });

            _shareMenuSet = true;
            Debug.Log("[WeChatShare] 分享系统初始化完成");
        }

        /// <summary>
        /// 主动发起分享
        /// </summary>
        public void ShareToFriend(string title, string imageUrl = null, string query = null, Action<bool> callback = null)
        {
            Debug.Log($"[WeChatShare] 分享到好友: {title}");

            WX.ShareAppMessage(new ShareAppMessageOption
            {
                title = title,
                imageUrl = imageUrl ?? DEFAULT_SHARE_IMAGE,
                query = query ?? "from=share"
            });

            _shareCount++;
            _successShareCount++;

            OnShareCompleted?.Invoke(true, "分享成功");
            callback?.Invoke(true);
        }

        /// <summary>
        /// 分享成就
        /// </summary>
        public void ShareAchievement(string achievementTitle, string achievementDescription, Action<bool> callback = null)
        {
            string title = $"我在《末世生存合成》中解锁了成就：{achievementTitle}";
            ShareToFriend(title, null, "from=achievement", callback);
        }

        /// <summary>
        /// 分享游戏进度
        /// </summary>
        public void ShareProgress(int level, int score, Action<bool> callback = null)
        {
            string title = $"我在《末世生存合成》中达到了{level}级，分数{score}分！";
            ShareToFriend(title, null, "from=progress", callback);
        }

        /// <summary>
        /// 分享排行榜排名
        /// </summary>
        public void ShareRanking(int rank, int score, Action<bool> callback = null)
        {
            string title = $"我在《末世生存合成》排行榜中排名第{rank}名！";
            ShareToFriend(title, null, "from=ranking", callback);
        }

        /// <summary>
        /// 邀请好友
        /// </summary>
        public void InviteFriend(string inviteMessage = "来和我一起玩《末世生存合成》吧！", Action<bool> callback = null)
        {
            ShareToFriend(inviteMessage, null, "from=invite", callback);
        }

        /// <summary>
        /// 更新分享菜单（被动分享时的内容）
        /// </summary>
        public void UpdateShareMenu(string title, string imageUrl = null)
        {
            var shareParam = new WXShareAppMessageParam
            {
                title = title,
                imageUrl = imageUrl ?? DEFAULT_SHARE_IMAGE
            };

            WX.OnShareAppMessage(shareParam);
            Debug.Log("[WeChatShare] 分享菜单已更新");
        }

        public string GetShareInfo()
        {
            return $"分享次数: {_shareCount}, 成功: {_successShareCount}";
        }
    }
}
