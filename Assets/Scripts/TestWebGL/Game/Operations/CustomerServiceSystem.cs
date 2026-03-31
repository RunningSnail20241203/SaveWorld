using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestWebGL.Game.Operations
{
    /// <summary>
    /// 客服系统
    /// 负责玩家反馈、问题报告、客服支持等功能
    /// </summary>
    public class CustomerServiceSystem : MonoBehaviour
    {
        private static CustomerServiceSystem s_instance;
        public static CustomerServiceSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("CustomerServiceSystem");
                    s_instance = go.AddComponent<CustomerServiceSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 客服配置
        private const string FEEDBACK_KEY = "customer_feedback";
        private const string TICKET_KEY = "customer_ticket";
        private const int MAX_FEEDBACK_HISTORY = 50;
        private const int MAX_TICKET_HISTORY = 20;

        // 反馈历史
        private List<FeedbackData> _feedbackHistory = new List<FeedbackData>();
        
        // 工单历史
        private List<TicketData> _ticketHistory = new List<TicketData>();
        
        // 初始化状态
        private bool _isInitialized = false;

        // 事件
        public event Action<bool, string> OnFeedbackSubmitted;
        public event Action<bool, string> OnTicketCreated;
        public event Action<TicketData> OnTicketUpdated;
        public event Action<List<FeedbackData>> OnFeedbackHistoryLoaded;

        /// <summary>
        /// 初始化客服系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[CustomerService] 初始化客服系统...");

            // 加载历史数据
            LoadFeedbackHistory();
            LoadTicketHistory();

            _isInitialized = true;
            Debug.Log("[CustomerService] 客服系统初始化完成");
        }

        /// <summary>
        /// 提交反馈
        /// </summary>
        public void SubmitFeedback(string feedbackType, string content, string contactInfo = null, string screenshot = null)
        {
            if (!_isInitialized) return;

            Debug.Log($"[CustomerService] 提交反馈: {feedbackType}");

            // 创建反馈数据
            var feedback = new FeedbackData
            {
                feedbackId = Guid.NewGuid().ToString(),
                feedbackType = feedbackType,
                content = content,
                contactInfo = contactInfo,
                screenshot = screenshot,
                submitTime = DateTime.Now,
                status = FeedbackStatus.Pending,
                playerInfo = GetPlayerInfo()
            };

            // 添加到历史记录
            _feedbackHistory.Add(feedback);

            // 限制历史记录数量
            if (_feedbackHistory.Count > MAX_FEEDBACK_HISTORY)
            {
                _feedbackHistory.RemoveAt(0);
            }

            // 保存到本地
            SaveFeedbackHistory();

            // 上传到服务器
            UploadFeedback(feedback);

            OnFeedbackSubmitted?.Invoke(true, "反馈提交成功");
            Debug.Log($"[CustomerService] 反馈提交成功: {feedback.feedbackId}");
        }

        /// <summary>
        /// 创建工单
        /// </summary>
        public void CreateTicket(string subject, string description, string category, string priority = "normal")
        {
            if (!_isInitialized) return;

            Debug.Log($"[CustomerService] 创建工单: {subject}");

            // 创建工单数据
            var ticket = new TicketData
            {
                ticketId = Guid.NewGuid().ToString(),
                subject = subject,
                description = description,
                category = category,
                priority = priority,
                status = TicketStatus.Open,
                createTime = DateTime.Now,
                updateTime = DateTime.Now,
                playerInfo = GetPlayerInfo(),
                messages = new List<TicketMessage>()
            };

            // 添加初始消息
            ticket.messages.Add(new TicketMessage
            {
                sender = "player",
                content = description,
                timestamp = DateTime.Now
            });

            // 添加到历史记录
            _ticketHistory.Add(ticket);

            // 限制历史记录数量
            if (_ticketHistory.Count > MAX_TICKET_HISTORY)
            {
                _ticketHistory.RemoveAt(0);
            }

            // 保存到本地
            SaveTicketHistory();

            // 上传到服务器
            UploadTicket(ticket);

            OnTicketCreated?.Invoke(true, "工单创建成功");
            Debug.Log($"[CustomerService] 工单创建成功: {ticket.ticketId}");
        }

        /// <summary>
        /// 回复工单
        /// </summary>
        public void ReplyToTicket(string ticketId, string message)
        {
            if (!_isInitialized) return;

            var ticket = _ticketHistory.Find(t => t.ticketId == ticketId);
            if (ticket == null)
            {
                Debug.LogWarning($"[CustomerService] 工单不存在: {ticketId}");
                return;
            }

            // 添加回复消息
            ticket.messages.Add(new TicketMessage
            {
                sender = "player",
                content = message,
                timestamp = DateTime.Now
            });

            ticket.updateTime = DateTime.Now;

            // 保存到本地
            SaveTicketHistory();

            // 上传到服务器
            UploadTicketReply(ticketId, message);

            OnTicketUpdated?.Invoke(ticket);
            Debug.Log($"[CustomerService] 工单回复成功: {ticketId}");
        }

        /// <summary>
        /// 关闭工单
        /// </summary>
        public void CloseTicket(string ticketId)
        {
            if (!_isInitialized) return;

            var ticket = _ticketHistory.Find(t => t.ticketId == ticketId);
            if (ticket == null)
            {
                Debug.LogWarning($"[CustomerService] 工单不存在: {ticketId}");
                return;
            }

            ticket.status = TicketStatus.Closed;
            ticket.updateTime = DateTime.Now;

            // 保存到本地
            SaveTicketHistory();

            // 上传到服务器
            UpdateTicketStatus(ticketId, TicketStatus.Closed);

            OnTicketUpdated?.Invoke(ticket);
            Debug.Log($"[CustomerService] 工单已关闭: {ticketId}");
        }

        /// <summary>
        /// 获取反馈历史
        /// </summary>
        public List<FeedbackData> GetFeedbackHistory()
        {
            return new List<FeedbackData>(_feedbackHistory);
        }

        /// <summary>
        /// 获取工单历史
        /// </summary>
        public List<TicketData> GetTicketHistory()
        {
            return new List<TicketData>(_ticketHistory);
        }

        /// <summary>
        /// 获取待处理工单
        /// </summary>
        public List<TicketData> GetPendingTickets()
        {
            return _ticketHistory.FindAll(t => t.status == TicketStatus.Open || t.status == TicketStatus.InProgress);
        }

        /// <summary>
        /// 获取玩家信息
        /// </summary>
        private PlayerInfo GetPlayerInfo()
        {
            // 这里可以从GameManager获取玩家信息
            return new PlayerInfo
            {
                playerId = "player_" + SystemInfo.deviceUniqueIdentifier,
                playerName = "玩家",
                level = 1,
                deviceModel = SystemInfo.deviceModel,
                operatingSystem = SystemInfo.operatingSystem,
                gameVersion = Application.version
            };
        }

        /// <summary>
        /// 上传反馈到服务器
        /// </summary>
        private void UploadFeedback(FeedbackData feedback)
        {
            // 这里实现实际的上传逻辑
            // 可以调用微信API或自建服务器
            Debug.Log($"[CustomerService] 上传反馈到服务器: {feedback.feedbackId}");
        }

        /// <summary>
        /// 上传工单到服务器
        /// </summary>
        private void UploadTicket(TicketData ticket)
        {
            // 这里实现实际的上传逻辑
            Debug.Log($"[CustomerService] 上传工单到服务器: {ticket.ticketId}");
        }

        /// <summary>
        /// 上传工单回复
        /// </summary>
        private void UploadTicketReply(string ticketId, string message)
        {
            // 这里实现实际的上传逻辑
            Debug.Log($"[CustomerService] 上传工单回复: {ticketId}");
        }

        /// <summary>
        /// 更新工单状态
        /// </summary>
        private void UpdateTicketStatus(string ticketId, TicketStatus status)
        {
            // 这里实现实际的更新逻辑
            Debug.Log($"[CustomerService] 更新工单状态: {ticketId} -> {status}");
        }

        /// <summary>
        /// 保存反馈历史
        /// </summary>
        private void SaveFeedbackHistory()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(new FeedbackHistoryWrapper { feedbacks = _feedbackHistory });
                PlayerPrefs.SetString(FEEDBACK_KEY, jsonData);
                PlayerPrefs.Save();
                Debug.Log("[CustomerService] 反馈历史保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CustomerService] 保存反馈历史失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载反馈历史
        /// </summary>
        private void LoadFeedbackHistory()
        {
            try
            {
                if (PlayerPrefs.HasKey(FEEDBACK_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(FEEDBACK_KEY);
                    var wrapper = JsonUtility.FromJson<FeedbackHistoryWrapper>(jsonData);
                    if (wrapper != null && wrapper.feedbacks != null)
                    {
                        _feedbackHistory = wrapper.feedbacks;
                    }
                }
                Debug.Log("[CustomerService] 反馈历史加载成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CustomerService] 加载反馈历史失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存工单历史
        /// </summary>
        private void SaveTicketHistory()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(new TicketHistoryWrapper { tickets = _ticketHistory });
                PlayerPrefs.SetString(TICKET_KEY, jsonData);
                PlayerPrefs.Save();
                Debug.Log("[CustomerService] 工单历史保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CustomerService] 保存工单历史失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载工单历史
        /// </summary>
        private void LoadTicketHistory()
        {
            try
            {
                if (PlayerPrefs.HasKey(TICKET_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(TICKET_KEY);
                    var wrapper = JsonUtility.FromJson<TicketHistoryWrapper>(jsonData);
                    if (wrapper != null && wrapper.tickets != null)
                    {
                        _ticketHistory = wrapper.tickets;
                    }
                }
                Debug.Log("[CustomerService] 工单历史加载成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CustomerService] 加载工单历史失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取客服统计信息
        /// </summary>
        public string GetCustomerServiceInfo()
        {
            int pendingTickets = GetPendingTickets().Count;
            return $"反馈: {_feedbackHistory.Count}条, 工单: {_ticketHistory.Count}条 (待处理: {pendingTickets}条)";
        }

        /// <summary>
        /// 清除所有客服数据
        /// </summary>
        public void ClearAllData()
        {
            _feedbackHistory.Clear();
            _ticketHistory.Clear();
            PlayerPrefs.DeleteKey(FEEDBACK_KEY);
            PlayerPrefs.DeleteKey(TICKET_KEY);
            PlayerPrefs.Save();
            Debug.Log("[CustomerService] 所有客服数据已清除");
        }
    }

    /// <summary>
    /// 反馈状态
    /// </summary>
    public enum FeedbackStatus
    {
        Pending,    // 待处理
        Processing, // 处理中
        Resolved,   // 已解决
        Closed      // 已关闭
    }

    /// <summary>
    /// 工单状态
    /// </summary>
    public enum TicketStatus
    {
        Open,       // 待处理
        InProgress, // 处理中
        Resolved,   // 已解决
        Closed      // 已关闭
    }

    /// <summary>
    /// 反馈数据
    /// </summary>
    [System.Serializable]
    public class FeedbackData
    {
        public string feedbackId;
        public string feedbackType;
        public string content;
        public string contactInfo;
        public string screenshot;
        public DateTime submitTime;
        public FeedbackStatus status;
        public PlayerInfo playerInfo;
    }

    /// <summary>
    /// 工单数据
    /// </summary>
    [System.Serializable]
    public class TicketData
    {
        public string ticketId;
        public string subject;
        public string description;
        public string category;
        public string priority;
        public TicketStatus status;
        public DateTime createTime;
        public DateTime updateTime;
        public PlayerInfo playerInfo;
        public List<TicketMessage> messages;
    }

    /// <summary>
    /// 工单消息
    /// </summary>
    [System.Serializable]
    public class TicketMessage
    {
        public string sender;
        public string content;
        public DateTime timestamp;
    }

    /// <summary>
    /// 玩家信息
    /// </summary>
    [System.Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string playerName;
        public int level;
        public string deviceModel;
        public string operatingSystem;
        public string gameVersion;
    }

    /// <summary>
    /// 反馈历史包装类
    /// </summary>
    [System.Serializable]
    public class FeedbackHistoryWrapper
    {
        public List<FeedbackData> feedbacks;
    }

    /// <summary>
    /// 工单历史包装类
    /// </summary>
    [System.Serializable]
    public class TicketHistoryWrapper
    {
        public List<TicketData> tickets;
    }
}