using UnityEngine;

namespace SaveWorld.Game.UI
{
    public abstract class UIPanelBase : MonoBehaviour
    {
        public abstract void Initialize();

        public abstract void Refresh();
    }
}
