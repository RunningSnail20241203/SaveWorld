using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaveWorld
{
    // 地形类
    public class Terrain
    {
        public float Modifier { get; set; } = 1.0f;
        public Dictionary<int, List<Card>> PlayerCards { get; } = new Dictionary<int, List<Card>>();
    
        public Terrain()
        {
            PlayerCards[0] = new List<Card>();
            PlayerCards[1] = new List<Card>();
        }
    
        public void AddCard(Card card, int playerId)
        {
            PlayerCards[playerId].Add(card);
        }
    
        public void RemoveCard(Card card, int playerId)
        {
            PlayerCards[playerId].Remove(card);
        }
    
        public float CalculatePower(int playerId)
        {
            return PlayerCards[playerId].Sum(card => card.Power) * Modifier;
        }
    }
}