using UnityEngine;

namespace EggClickerGame
{
    [System.Serializable]
    public class Creature
    {
        public string creatureID; // Unique ID used for saving (e.g. "dragon_fire")
        public string creatureName; // Public display name (e.g. "Fire Dragon")
        public Sprite creatureSprite; // The sprite asset you already have
        [Tooltip("Higher weight means more common. e.g., Common=100, Rare=20, Legendary=2")]
        public int rarityWeight = 100;
    }
}
