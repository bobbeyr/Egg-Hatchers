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
    }
}
