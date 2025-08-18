//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace SaveWorld
{
    public class PlaySingleGame : GameBase
    {
        private float m_ElapseSeconds;

        public override GameMode GameMode => GameMode.PlaySingle;

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds, realElapseSeconds);

            
        }
    }
}
