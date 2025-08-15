using System;
using GameFramework.Event;

namespace Runtime.Logic.Event
{
    public class LeaveGameEventArgs : GameEventArgs
    {
        /// <summary>
        /// 加载全局配置成功事件编号。
        /// </summary>
        public static readonly int EventId = typeof(LeaveGameEventArgs).GetHashCode();
        
        public string NextSceneName { get; set; }
        public Type NextProcedureType { get; set; }
        
        public override void Clear()
        {
            
        }

        public override int Id => EventId;
        
    }
}