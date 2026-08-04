using UnityEngine;

namespace EggClickerGame
{
    [System.Serializable]
    public class Creature
    {
        [Header("Identity Parameters")]
        public string creatureID;      // Unique ID used for saving (e.g. "dragon_fire")
        public string creatureName;    // Public display name (e.g. "Fire Dragon")
        public Sprite creatureSprite;  // The sprite asset you already have

        [Header("Classification Tier")]
        [Tooltip("Select the rarity class. The drop weights are handled automatically by the Journal Manager.")]
        public CreatureRarity rarity;  // Adds the Common, Epic, Rare dropdown menu in your Inspector
    }
}
