using System;

namespace TestWebGL.Game.Orders
{
    /// <summary>
    /// 订单数据类 - 表示一个游戏订单
    /// </summary>
    [Serializable]
    public class OrderData
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 订单标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 订单描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 订单奖励
        /// </summary>
        public int Reward { get; set; }

        /// <summary>
        /// 剩余时间（秒）
        /// </summary>
        public float RemainingTime { get; set; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// 是否已领取奖励
        /// </summary>
        public bool IsClaimed { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public OrderType Type { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public OrderData(string id, string title, string description, int reward, float duration, OrderType type = OrderType.Normal)
        {
            Id = id;
            Title = title;
            Description = description;
            Reward = reward;
            RemainingTime = duration;
            Type = type;
            IsCompleted = false;
            IsClaimed = false;
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        public void Update(float deltaTime)
        {
            if (IsCompleted || IsClaimed) return;

            RemainingTime -= deltaTime;
            if (RemainingTime <= 0)
            {
                RemainingTime = 0;
                IsCompleted = true;
            }
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        public bool ClaimReward()
        {
            if (IsCompleted && !IsClaimed)
            {
                IsClaimed = true;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 订单类型枚举
    /// </summary>
    public enum OrderType
    {
        Normal,
        Urgent,
        Special
    }
}