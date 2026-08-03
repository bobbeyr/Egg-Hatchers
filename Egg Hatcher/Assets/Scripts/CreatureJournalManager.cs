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

        void Awake()
        {
            Instance = this;

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
        }

        void Start()
        {
            if (journalPanel != null) journalPanel.SetActive(false);
            if (hatchPopupPanel != null) hatchPopupPanel.SetActive(false);
        }

        public void OpenJournal()
        {
            Debug.Log("<color=cyan>[JOURNAL MANAGER]</color> OpenJournal Button Clicked!");
            if (journalPanel != null) journalPanel.SetActive(true);

            // Hide core game UI elements
            ToggleCoreGameUI(false);

            // Populate journal grid
            PopulateJournalGrid();

            // Force layout sync for debugging
            ForceCleanLayoutSync();
        }

        public void CloseJournal()
        {
            if (journalPanel != null) journalPanel.SetActive(false);
            ToggleCoreGameUI(true);
        }

        private void CloseHatchPopup()
        {
            if (hatchPopupPanel != null) hatchPopupPanel.SetActive(false);
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
            if (allCreatures.Count == 0) return null;
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

            if (hatchPopupPanel != null && hatchPopupText != null && hatchPopupCreatureImage != null)
            {
                hatchPopupText.text = $"You unlocked <color=yellow>{creature.creatureName}</color>!\nCheck the journal";
                hatchPopupCreatureImage.sprite = creature.creatureSprite;
                hatchPopupPanel.SetActive(true);
            }

            Object.FindFirstObjectByType<EggHatcher>()?.SaveGame();
        }

        private void PopulateJournalGrid()
        {
            if (gridContainer == null || journalSlotPrefab == null)
            {
                Debug.LogError("Journal Grid Container or Slot Prefab is unassigned in the Inspector!");
                return;
            }

            // Clear old slots
            foreach (Transform child in gridContainer)
                Destroy(child.gameObject);

            // Instantiate new slots
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
            Debug.Log($"[DIAGNOSTIC] Successfully spawned {allCreatures.Count} creature blocks inside the container panel layout.");
        }

        public List<string> GetSaveIDs() => new List<string>(ownedCreatures.Keys);
        public List<int> GetSaveCounts() => new List<int>(ownedCreatures.Values);

        public void LoadFromSave(List<string> ids, List<int> counts)
        {
            ownedCreatures.Clear();
            if (ids == null || counts == null) return;
            for (int i = 0; i < ids.Count; i++)
            {
                if (i < counts.Count) ownedCreatures.Add(ids[i], counts[i]);
            }
        }

        public void WipeCollection()
        {
            ownedCreatures.Clear();
        }

        /// <summary>
        /// Debug function: forces the layout to reset and aligns it properly.
        /// </summary>
        private void ForceCleanLayoutSync()
        {
            if (gridContainer == null) return;

            RectTransform gridRect = gridContainer.GetComponent<RectTransform>();
            if (gridRect != null)
            {
                Debug.Log("<color=yellow>[VISUAL DEBUGGER]</color> Actively seizing Grid Container dimensions via code to fix positional drifting...");
                gridRect.anchorMin = new Vector2(0.5f, 0.5f);
                gridRect.anchorMax = new Vector2(0.5f, 0.5f);
                gridRect.pivot = new Vector2(0.5f, 0.5f);
                gridRect.anchoredPosition = Vector2.zero;
                gridRect.localPosition = Vector3.zero;
                gridRect.localScale = Vector3.one;
                gridRect.SetAsLastSibling();
            }

            foreach (Transform child in gridContainer)
            {
                child.gameObject.SetActive(true);
                RectTransform childRect = child.GetComponent<RectTransform>();
                if (childRect != null) childRect.localScale = Vector3.one;
            }
        }
    }
}