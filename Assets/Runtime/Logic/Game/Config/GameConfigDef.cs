using UnityEngine;

namespace SaveWorld
{
    public class GameConfigDef : ScriptableObject
    {
        public int DeckSize;
        public int MaxPlayers;
        public int InitialHandSize;
        public int InitialEnergy;
        public int EnergyPerTurn;
        public int MaxTerrains;
    }
}