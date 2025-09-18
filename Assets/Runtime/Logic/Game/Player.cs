using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld
{
    // 玩家类
    public class Player
    {
        public List<Card> Deck { get; set;} = new List<Card>();
        public List<Card> Hand { get; set;} = new List<Card>();
        public int Energy { get; set; }
        public bool IsAI { get; set; }
    }
}