using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace EggClickerGame
{
    // Restored enum for rarity categories
    public enum CreatureRarity
    {
        Common,
        Rare,
        Epic,
        Legendary,
        Mythical,
        Secret
    }

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

        [Header("Prefabs Configuration")]
        public GameObject journalSlotPrefab;
        [Tooltip("Make sure this prefab has a Horizontal Layout Group set to Upper Center!")]
        public GameObject rarityHeaderPrefab;

        [Header("Grid Row Layout Tuning")]
        [Tooltip("Set the exact pixel width and height for your item boxes.")]
        public Vector2 cellSize = new Vector2(100f, 100f);
        [Tooltip("The X (horizontal) and Y (vertical) gap spacing between item slots.")]
        public Vector2 cellSpacing = new Vector2(15f, 15f);
        [Tooltip("The left padding margin for shifting creature squares right.")]
        public int creatureGridLeftPadding = 23; // Only one declaration

        [Header("Dynamic Layout Settings")]
        public int bannerLeftPadding = 60; // full span header padding

        [Header("Hatch Popup Window Layout")]
        public GameObject hatchPopupPanel;
        public TMP_Text hatchPopupText;
        public Image hatchPopupCreatureImage;
        public Button hatchPopupCloseButton;

        private Dictionary<string, int> ownedCreatures = new Dictionary<string, int>();
        private Queue<Creature> offlineHatchQueue = new Queue<Creature>();
        private bool isGameLoading = false;

        private readonly Dictionary<CreatureRarity, int> rarityWeights = new Dictionary<CreatureRarity, int>()
        {
            { CreatureRarity.Common, 100 },
            { CreatureRarity.Rare, 80 },
            { CreatureRarity.Epic, 60 },
            { CreatureRarity.Legendary, 35 },
            { CreatureRarity.Mythical, 10 },
            { CreatureRarity.Secret, 1 }
        };

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
            if (journalPanel != null) journalPanel.SetActive(true);
            SettingsManager settings = Object.FindFirstObjectByType<SettingsManager>();
            if (settings != null && settings.openJournalButton != null)
                settings.openJournalButton.gameObject.SetActive(false);

            ToggleCoreGameUI(false);
            PopulateJournalGrid();
            ForceCleanLayoutSync();
        }

        public void CloseJournal()
        {
            if (journalPanel != null) journalPanel.SetActive(false);
            SettingsManager settings = Object.FindFirstObjectByType<SettingsManager>();
            if (settings != null && settings.openJournalButton != null)
                settings.openJournalButton.gameObject.SetActive(true);
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
            foreach (var creature in allCreatures)
            {
                totalWeight += GetRarityWeight(creature.rarity);
            }

            int rand = Random.Range(0, totalWeight);
            int sum = 0;
            foreach (var creature in allCreatures)
            {
                sum += GetRarityWeight(creature.rarity);
                if (rand < sum) return creature;
            }
            return allCreatures[0];
        }

        private int GetRarityWeight(CreatureRarity rarity)
        {
            if (rarityWeights.TryGetValue(rarity, out int weight))
                return weight;
            return 100;
        }

        public void AddCreatureToCollection(Creature creature)
        {
            if (creature == null) return;

            if (ownedCreatures.ContainsKey(creature.creatureID))
                ownedCreatures[creature.creatureID]++;
            else
                ownedCreatures.Add(creature.creatureID, 1);

            EggHatcher hatcher = Object.FindFirstObjectByType<EggHatcher>();
            bool isOfflinePanelOpen = hatcher != null && hatcher.offlinePopupPanel != null && hatcher.offlinePopupPanel.activeSelf;

            if (isGameLoading || isOfflinePanelOpen)
            {
                offlineHatchQueue.Enqueue(creature);
                return;
            }

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
            if (gridContainer == null || journalSlotPrefab == null || rarityHeaderPrefab == null) return;

            // Clear previous content
            foreach (Transform child in gridContainer) Destroy(child.gameObject);

            // Categorize creatures by rarity
            Dictionary<CreatureRarity, List<Creature>> categorizedCreatures = new Dictionary<CreatureRarity, List<Creature>>();
            foreach (CreatureRarity rarityType in System.Enum.GetValues(typeof(CreatureRarity)))
            {
                categorizedCreatures[rarityType] = new List<Creature>();
            }
            foreach (var creature in allCreatures)
            {
                categorizedCreatures[creature.rarity].Add(creature);
            }

            // Loop through each rarity category
            foreach (CreatureRarity rarityType in System.Enum.GetValues(typeof(CreatureRarity)))
            {
                List<Creature> creaturesInThisTier = categorizedCreatures[rarityType];
                if (creaturesInThisTier.Count == 0) continue;

                // 1. Instantiate header banner (full span, zero padding)
                GameObject headerObj = Instantiate(rarityHeaderPrefab, gridContainer);
                HorizontalLayoutGroup headerGroup = headerObj.GetComponent<HorizontalLayoutGroup>() ?? headerObj.AddComponent<HorizontalLayoutGroup>();
                headerGroup.padding = new RectOffset(0, 0, 0, 0);
                headerGroup.childControlWidth = true;
                headerGroup.childForceExpandWidth = false;

                TMP_Text headerTxt = headerObj.GetComponentInChildren<TMP_Text>();
                if (headerTxt != null)
                {
                    headerTxt.text = GetColoredRarityName(rarityType);
                    headerTxt.alignment = TextAlignmentOptions.Center;
                }

                // 2. Create the creature grid row with configurable left padding
                GameObject rowGo = new GameObject($"{rarityType}_GridRow", typeof(RectTransform));
                rowGo.transform.SetParent(gridContainer, false);

                GridLayoutGroup gridGroup = rowGo.AddComponent<GridLayoutGroup>();
                gridGroup.padding = new RectOffset(this.creatureGridLeftPadding, 0, 0, 0);
                gridGroup.cellSize = cellSize;
                gridGroup.spacing = cellSpacing;
                gridGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
                gridGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                gridGroup.childAlignment = TextAnchor.UpperLeft;

                // Grow with content
                ContentSizeFitter rowFitter = rowGo.AddComponent<ContentSizeFitter>();
                rowFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                rowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // Populate creature slots
                foreach (var creature in creaturesInThisTier)
                {
                    GameObject newSlot = Instantiate(journalSlotPrefab, rowGo.transform);
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
        }

        private string GetColoredRarityName(CreatureRarity rarity)
        {
            switch (rarity)
            {
                case CreatureRarity.Common:
                    return "<color=#A0A0A0>Common</color>";
                case CreatureRarity.Epic:
                    return "<color=#A335EE>Epic</color>";
                case CreatureRarity.Rare:
                    return "<color=#0070DD>Rare</color>";
                case CreatureRarity.Legendary:
                    return "<color=#FF8000>Legendary</color>";
                case CreatureRarity.Mythical:
                    return "<color=#FF0000>Mythical</color>";
                case CreatureRarity.Secret:
                    return "<color=#00FFCC>Secret</color>";
                default:
                    return rarity.ToString();
            }
        }

        private void ForceCleanLayoutSync()
        {
            if (gridContainer == null) return;
            RectTransform rectTransform = gridContainer.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
                rectTransform.SetAsLastSibling();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        public List<string> GetSaveIDs() => new List<string>(ownedCreatures.Keys);
        public List<int> GetSaveCounts() => new List<int>(ownedCreatures.Values);

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
                if (i < counts.Count)
                {
                    ownedCreatures.Add(ids[i], counts[i]);
                }
            }
        }

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