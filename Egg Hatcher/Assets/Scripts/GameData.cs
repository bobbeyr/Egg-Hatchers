using System.Collections.Generic;
using UnityEngine;

namespace EggClickerGame
{
    [System.Serializable]
    public class GameData
    {
        public int tapsHatched;
        public int autoHatchCount;
        public int tapsPerClick;
        public int autoHatchCost;
        public long lastSaveTime;
        public int totalTapsInCurrentCycle;
        public int cracksNeeded;
        public bool isBroken;

        // FIXED: New fields added to track shop parameters inside the JSON template
        public float autoHatchInterval;
        public int levelTapStrength;
        public int costTapStrength;
        public int levelAutoHatcher;
        public int costAutoHatcher;
        public int levelHatchSpeed;
        public int costHatchSpeed;

        public List<string> journalCreatureIDs = new List<string>();
        public List<int> journalCreatureCounts = new List<int>();

    }
}
