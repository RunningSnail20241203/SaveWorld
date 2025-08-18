//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace SaveWorld
{
    /// <summary>
    /// 游戏模式。
    /// </summary>
    public enum GameMode : byte
    {
        /// <summary>
        /// 单机模式
        /// </summary>
        PlaySingle,
        /// <summary>
        /// 匹配模式
        /// </summary>
        PlayMatch,
        /// <summary>
        /// 邀请模式
        /// </summary>
        PlayRoom,
    }
}
