using GameFramework;

namespace SaveWorld
{
    /// <summary>
    /// GameBase 变量类。
    /// </summary>
    public sealed class VarGameBase : Variable<GameBase>
    {
        /// <summary>
        /// 初始化 GameBase 变量类的新实例。
        /// </summary>
        public VarGameBase()
        {
        }

        /// <summary>
        /// 从 GameBase 到 GameBase 变量类的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator VarGameBase(GameBase value)
        {
            var varValue = ReferencePool.Acquire<VarGameBase>();
            varValue.Value = value;
            return varValue;
        }

        /// <summary>
        /// 从 GameBase 变量类到 GameBase 的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator GameBase(VarGameBase value)
        {
            return value.Value;
        }
    }
}