using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    private string savePath;

    void Awake()
    {
        savePath = Application.persistentDataPath + "/savefile.json";
    }

    public void SaveGame(EggHatcher eggHatcher)
    {
        GameData data = new GameData
        {
            tapsHatched = eggHatcher.eggsHatched,
            autoHatchCount = eggHatcher.autoHatchCount,
            tapsPerClick = eggHatcher.eggsPerClick,
            autoHatchCost = eggHatcher.autoHatchCost,
            lastSaveTime = System.DateTime.UtcNow.Ticks
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved");
    }

    public void LoadGame(EggHatcher eggHatcher)
    {
        if (File.Exists(savePath))
        {
            // load saved data
            string json = File.ReadAllText(savePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            eggHatcher.eggsHatched = data.tapsHatched;
            eggHatcher.autoHatchCount = data.autoHatchCount;
            eggHatcher.eggsPerClick = data.tapsPerClick;
            eggHatcher.autoHatchCost = data.autoHatchCost;

            long lastTicks = data.lastSaveTime;
            long currentTicks = System.DateTime.UtcNow.Ticks;
            double secondsElapsed = (currentTicks - lastTicks) / (double)System.TimeSpan.TicksPerSecond;

            int offlineTaps = Mathf.FloorToInt((float)(secondsElapsed * eggHatcher.autoHatchCount));
            eggHatcher.eggsHatched += offlineTaps;

            eggHatcher.UpdateUI();

            Debug.Log($"Loaded game. Offline taps gained: {offlineTaps}");
        }
        else
        {
            Debug.Log("No save file found, setting defaults");
            eggHatcher.eggsHatched = 0;
            eggHatcher.autoHatchCount = 0;
            eggHatcher.eggsPerClick = 1;
            eggHatcher.autoHatchCost = 10;
            eggHatcher.UpdateUI();
        }
    }
}