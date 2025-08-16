using GameFramework.Event;

namespace Runtime.Logic.Event
{
    public class UIClickLoginBtnEventArgs : GameEventArgs
    {
        public static readonly int EventID = typeof(UIClickLoginBtnEventArgs).GetHashCode();
        public override void Clear()
        {
           
        }

        public override int Id => EventID;
    }
}