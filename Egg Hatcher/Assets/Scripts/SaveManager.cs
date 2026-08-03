using UnityEngine;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace EggClickerGame
{
    public class SaveManager : MonoBehaviour
    {
        private string savePath;

        [Header("Offline Balance Settings")]
        public float baseOfflineEfficiency = 0.5f;
        public float maxOfflineHours = 12f;

        private const string TimeApiUrl = "https://worldtimeapi.org";

        void Awake()
        {
            savePath = Application.persistentDataPath + "/savefile.json";
        }

        public void SaveGame(EggHatcher eggHatcher)
        {
            if (eggHatcher == null || eggHatcher.eggController == null) return;

            GameData data = new GameData
            {
                tapsHatched = eggHatcher.eggsHatched,
                autoHatchCount = eggHatcher.autoHatchCount,
                tapsPerClick = eggHatcher.eggsPerClick,
                autoHatchCost = eggHatcher.autoHatchCost,
                lastSaveTime = System.DateTime.UtcNow.Ticks,
                totalTapsInCurrentCycle = eggHatcher.eggController.GetTotalTapsInCurrentCycle(),
                cracksNeeded = eggHatcher.eggController.GetCracksNeeded(),
                isBroken = eggHatcher.eggController.IsBroken,
                autoHatchInterval = eggHatcher.autoHatchInterval
            };

            UpgradeButton[] allButtons = Object.FindObjectsByType<UpgradeButton>(FindObjectsSortMode.None);
            foreach (UpgradeButton btn in allButtons)
            {
                if (btn.upgradeName == "Tap Strength")
                {
                    data.levelTapStrength = btn.currentLevel;
                    data.costTapStrength = btn.currentCost;
                }
                else if (btn.upgradeName == "Auto Hatcher")
                {
                    data.levelAutoHatcher = btn.currentLevel;
                    data.costAutoHatcher = btn.currentCost;
                }
                else if (btn.upgradeName == "Hatch Speed")
                {
                    data.levelHatchSpeed = btn.currentLevel;
                    data.costHatchSpeed = btn.currentCost;
                }
            }

            if (CreatureJournalManager.Instance != null)
            {
                data.journalCreatureIDs = CreatureJournalManager.Instance.GetSaveIDs();
                data.journalCreatureCounts = CreatureJournalManager.Instance.GetSaveCounts();
            }

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(savePath, json);
            Debug.Log("Game Progress Saved Safely.");
        }

        public async void LoadGame(EggHatcher eggHatcher)
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                GameData data = JsonUtility.FromJson<GameData>(json);

                eggHatcher.eggsHatched = data.tapsHatched;
                eggHatcher.autoHatchCount = data.autoHatchCount;
                eggHatcher.eggsPerClick = data.tapsPerClick;
                eggHatcher.autoHatchCost = data.autoHatchCost;
                eggHatcher.autoHatchInterval = data.autoHatchInterval > 0 ? data.autoHatchInterval : 5f;

                if (eggHatcher.eggController != null)
                {
                    eggHatcher.eggController.LoadEggState(data.totalTapsInCurrentCycle, data.cracksNeeded, data.isBroken);
                }

                UpgradeButton[] allButtons = Object.FindObjectsByType<UpgradeButton>(FindObjectsSortMode.None);
                foreach (UpgradeButton btn in allButtons)
                {
                    string costKey = "Cost_" + btn.upgradeName.Replace(" ", "");
                    string levelKey = "Level_" + btn.upgradeName.Replace(" ", "");

                    if (btn.upgradeName == "Tap Strength")
                    {
                        btn.currentLevel = data.levelTapStrength;
                        btn.currentCost = data.costTapStrength > 0 ? data.costTapStrength : btn.initialCost;
                    }
                    else if (btn.upgradeName == "Auto Hatcher")
                    {
                        btn.currentLevel = data.levelAutoHatcher;
                        btn.currentCost = data.costAutoHatcher > 0 ? data.costAutoHatcher : btn.initialCost;
                    }
                    else if (btn.upgradeName == "Hatch Speed")
                    {
                        btn.currentLevel = data.levelHatchSpeed;
                        btn.currentCost = data.costHatchSpeed > 0 ? data.costHatchSpeed : btn.initialCost;
                    }

                    PlayerPrefs.SetInt(costKey, btn.currentCost);
                    PlayerPrefs.SetInt(levelKey, btn.currentLevel);
                    btn.UpdateButtonDisplay();
                }
                PlayerPrefs.Save();

                if (CreatureJournalManager.Instance != null)
                {
                    CreatureJournalManager.Instance.LoadFromSave(data.journalCreatureIDs, data.journalCreatureCounts);
                }

                long currentTicks = await GetNetworkTimeTicksAsync();
                long lastTicks = data.lastSaveTime;

                if (currentTicks < lastTicks)
                {
                    Debug.LogWarning("Time anomaly checked. Offline rewards canceled.");
                    currentTicks = lastTicks;
                }

                double secondsElapsed = (currentTicks - lastTicks) / (double)System.TimeSpan.TicksPerSecond;

                double maxOfflineSeconds = maxOfflineHours * 3600f;
                if (secondsElapsed > maxOfflineSeconds)
                {
                    secondsElapsed = maxOfflineSeconds;
                }

                float rawOfflineTaps = (float)((secondsElapsed / eggHatcher.autoHatchInterval) * eggHatcher.autoHatchCount);
                int offlineTaps = Mathf.FloorToInt(rawOfflineTaps * baseOfflineEfficiency);

                if (offlineTaps > 0)
                {
                    eggHatcher.AddOfflineProgressTaps(offlineTaps);
                }

                eggHatcher.UpdateUI();
                Debug.Log($"Loaded file data metrics cleanly.");
            }
            else
            {
                Debug.Log("No save file detected, initializing vanilla configurations...");
                eggHatcher.eggsHatched = 0;
                eggHatcher.autoHatchCount = 0;
                eggHatcher.eggsPerClick = 1;
                eggHatcher.autoHatchCost = 50;
                eggHatcher.autoHatchInterval = 5f;

                if (eggHatcher.eggController != null)
                {
                    eggHatcher.eggController.LoadEggState(0, 5, false);
                }

                UpgradeButton[] allButtons = Object.FindObjectsByType<UpgradeButton>(FindObjectsSortMode.None);
                foreach (UpgradeButton btn in allButtons)
                {
                    btn.currentLevel = 0;
                    btn.currentCost = btn.initialCost;
                    btn.UpdateButtonDisplay();
                }

                if (CreatureJournalManager.Instance != null)
                {
                    CreatureJournalManager.Instance.WipeCollection();
                }

                eggHatcher.UpdateUI();
            }

            // FIXED: This line notifies the Journal Manager that loading has finished, 
            // safely releasing the initialization popup block!
            if (CreatureJournalManager.Instance != null)
            {
                CreatureJournalManager.Instance.FinalizeInitializationLoad();
            }
        }

        private async Task<long> GetNetworkTimeTicksAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = System.TimeSpan.FromSeconds(3);
                try
                {
                    HttpResponseMessage response = await client.GetAsync(TimeApiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        TimeApiResponse timeData = JsonUtility.FromJson<TimeApiResponse>(jsonResponse);

                        if (timeData != null && timeData.unixtime > 0)
                        {
                            System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                            System.DateTime verifiedUtcTime = epoch.AddSeconds(timeData.unixtime);
                            return verifiedUtcTime.Ticks;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Secure web clock down ({ex.Message}). Machine clock fallback engaged.");
                }
            }
            return System.DateTime.UtcNow.Ticks;
        }
    }

    [System.Serializable]
    public class TimeApiResponse
    {
        public long unixtime;
    }
}
