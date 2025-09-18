using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld
{
    public class PlayerAction
    {
        public PlayerActionType ActionType { get; set; }
        public int CardIndex { get; set; }
        public int TerrainIndex { get; set; }
    }
}