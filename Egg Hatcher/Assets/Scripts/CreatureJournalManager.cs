using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace EggClickerGame
{
    public class CreatureJournalManager : MonoBehaviour
    {
        public static CreatureJournalManager Instance { get; private set; }

        [Header("Creature Database Pool")]
        public List<Creature> allCreatures = new List<Creature>();

        [Header("Journal UI Panels Layout")]
        public GameObject journalPanel;
        public Button openJournalButton;
        public Button closeJournalButton;
        public Transform gridContainer;
        public GameObject journalSlotPrefab;

        [Header("Hatch Popup Window Layout")]
        public GameObject hatchPopupPanel;
        public TMP_Text hatchPopupText;
        public Image hatchPopupCreatureImage;
        public Button hatchPopupCloseButton;

        private Dictionary<string, int> ownedCreatures = new Dictionary<string, int>();
        private Queue<Creature> offlineHatchQueue = new Queue<Creature>();

        // FIXED: This state tracker flag blocks popups from opening while the game is reading your files
        private bool isGameLoading = false;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            if (openJournalButton != null)
            {
                openJournalButton.onClick.RemoveAllListeners();
                openJournalButton.onClick.AddListener(OpenJournal);
            }
            if (closeJournalButton != null)
            {
                closeJournalButton.onClick.RemoveAllListeners();
                closeJournalButton.onClick.AddListener(CloseJournal);
            }
            if (hatchPopupCloseButton != null)
            {
                hatchPopupCloseButton.onClick.RemoveAllListeners();
                hatchPopupCloseButton.onClick.AddListener(CloseHatchPopup);
            }

            if (journalPanel != null) journalPanel.SetActive(false);
            if (hatchPopupPanel != null) hatchPopupPanel.SetActive(false);
        }

        public void OpenJournal()
        {
            Debug.Log("<color=cyan>[JOURNAL MANAGER]</color> OpenJournal Button Clicked!");
            if (journalPanel != null) journalPanel.SetActive(true);

            SettingsManager settings = Object.FindFirstObjectByType<SettingsManager>();
            if (settings != null && settings.openJournalButton != null)
            {
                settings.openJournalButton.gameObject.SetActive(false);
            }

            ToggleCoreGameUI(false);
            PopulateJournalGrid();
            ForceCleanLayoutSync();
        }

        public void CloseJournal()
        {
            if (journalPanel != null) journalPanel.SetActive(false);

            SettingsManager settings = Object.FindFirstObjectByType<SettingsManager>();
            if (settings != null && settings.openJournalButton != null)
            {
                settings.openJournalButton.gameObject.SetActive(true);
            }

            ToggleCoreGameUI(true);
        }

        public void CloseHatchPopup()
        {
            if (hatchPopupPanel != null) hatchPopupPanel.SetActive(false);
            CheckHatchQueue();
        }

        private void ToggleCoreGameUI(bool show)
        {
            EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();
            if (hatcher != null)
            {
                if (hatcher.eggProgressBar != null) hatcher.eggProgressBar.gameObject.SetActive(show);
                if (hatcher.eggController != null) hatcher.eggController.gameObject.SetActive(show);
            }
        }

        public Creature RollRandomCreature()
        {
            if (allCreatures == null || allCreatures.Count == 0) return null;
            int totalWeight = 0;
            foreach (var creature in allCreatures) totalWeight += creature.rarityWeight;
            int randomValue = Random.Range(0, totalWeight);
            int currentWeightSum = 0;
            foreach (var creature in allCreatures)
            {
                currentWeightSum += creature.rarityWeight;
                if (randomValue < currentWeightSum) return creature;
            }
            return allCreatures[0];
        }

        public void AddCreatureToCollection(Creature creature)
        {
            if (creature == null) return;

            if (ownedCreatures.ContainsKey(creature.creatureID)) ownedCreatures[creature.creatureID]++;
            else ownedCreatures.Add(creature.creatureID, 1);

            Debug.Log($"Hatched: {creature.creatureName}! Count: {ownedCreatures[creature.creatureID]}");

            EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();
            bool isOfflinePanelOpen = hatcher != null && hatcher.offlinePopupPanel != null && hatcher.offlinePopupPanel.activeSelf;

            // FIXED: If the file system is currently loading OR the offline summary UI panel is visible,
            // force the creature data card straight into the queue so it cannot overlap!
            if (isGameLoading || isOfflinePanelOpen)
            {
                offlineHatchQueue.Enqueue(creature);
                Debug.Log($"[QUEUE DETENTION] Cached load roll: '{creature.creatureName}' locked safely in queue.");
                return;
            }

            // Otherwise, if the popup is already active on screen from active play, queue it up too
            if (hatchPopupPanel != null && hatchPopupPanel.activeSelf)
            {
                offlineHatchQueue.Enqueue(creature);
                return;
            }

            ShowHatchPopupVisuals(creature);
        }

        private void ShowHatchPopupVisuals(Creature creature)
        {
            if (hatchPopupPanel != null && hatchPopupText != null && hatchPopupCreatureImage != null)
            {
                hatchPopupText.text = $"You unlocked <color=yellow>{creature.creatureName}</color>!\nCheck the journal";
                hatchPopupCreatureImage.sprite = creature.creatureSprite;
                hatchPopupPanel.SetActive(true);
            }
            Object.FindFirstObjectByType<EggHatcher>()?.SaveGame();
        }

        public void CheckHatchQueue()
        {
            EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();
            if (hatcher != null && hatcher.offlinePopupPanel != null && hatcher.offlinePopupPanel.activeSelf) return;

            if (offlineHatchQueue.Count > 0)
            {
                Creature nextCreature = offlineHatchQueue.Dequeue();
                ShowHatchPopupVisuals(nextCreature);
            }
        }

        private void PopulateJournalGrid()
        {
            if (gridContainer == null || journalSlotPrefab == null) return;
            foreach (Transform child in gridContainer) Destroy(child.gameObject);

            foreach (var creature in allCreatures)
            {
                GameObject newSlot = Instantiate(journalSlotPrefab, gridContainer);
                Image creatureImg = newSlot.transform.Find("CreatureImage")?.GetComponent<Image>() ?? newSlot.GetComponentInChildren<Image>();
                TMP_Text countTxt = newSlot.transform.Find("CountText")?.GetComponent<TMP_Text>() ?? newSlot.GetComponentInChildren<TMP_Text>();

                bool hasUnlocked = ownedCreatures.ContainsKey(creature.creatureID);

                if (creatureImg != null)
                {
                    creatureImg.sprite = creature.creatureSprite;
                    creatureImg.color = hasUnlocked ? Color.white : Color.black;
                }

                if (countTxt != null)
                {
                    if (hasUnlocked && ownedCreatures[creature.creatureID] > 1)
                    {
                        countTxt.text = ownedCreatures[creature.creatureID].ToString();
                        countTxt.gameObject.SetActive(true);
                    }
                    else
                    {
                        countTxt.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void ForceCleanLayoutSync()
        {
            if (gridContainer == null) return;

            RectTransform gridRect = gridContainer.GetComponent<RectTransform>();
            if (gridRect != null)
            {
                // FIXED: Changed from Vector3.one (1,1,1) to Vector3(0.5f, 0.5f, 1f) 
                // to forcefully lock your desired 0.5 scale size choice directly through code!
                gridRect.localScale = new Vector3(0.5f, 0.5f, 1f);

                gridRect.SetAsLastSibling();
            }

            foreach (Transform child in gridContainer)
            {
                child.gameObject.SetActive(true);
                RectTransform childRect = child.GetComponent<RectTransform>();

                // FIXED: Also lock the internal slot children to normal size scale boundaries
                if (childRect != null) childRect.localScale = Vector3.one;
            }
        }


        public List<string> GetSaveIDs() => new List<string>(ownedCreatures.Keys);
        public List<int> GetSaveCounts() => new List<int>(ownedCreatures.Values);

        // FIXED: Activates our file safety tracker lock when reading the save structure
        public void LoadFromSave(List<string> ids, List<int> counts)
        {
            isGameLoading = true;
            ownedCreatures.Clear();
            if (ids == null || counts == null)
            {
                isGameLoading = false;
                return;
            }
            for (int i = 0; i < ids.Count; i++)
            {
                if (i < counts.Count) ownedCreatures.Add(ids[i], counts[i]);
            }
        }

        // FIXED: Safely unlocks the popup system only after SaveManager has completely finished loading
        public void FinalizeInitializationLoad()
        {
            isGameLoading = false;
            Debug.Log("[LOAD SYSTEM] File reading completed. Safety lock released.");
        }

        public void WipeCollection()
        {
            ownedCreatures.Clear();
            offlineHatchQueue.Clear();
            isGameLoading = false;
        }
    }
}
