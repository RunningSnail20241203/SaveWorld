using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld
{
    // 卡牌基类
    public class Card
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public int Power { get; set; }
        public Action<GameState, int, Card> Ability { get; set; } // 卡牌特殊能力
    
        public virtual void OnPlay(GameState gameState, int terrainIndex)
        {
            Ability?.Invoke(gameState, terrainIndex, this);
        }
    
        public virtual void OnUndo(GameState gameState, int terrainIndex)
        {
            // 默认不做特殊处理，有能力的卡牌需要重写
        }
    }
}