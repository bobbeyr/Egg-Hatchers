using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Upgrade Parameters")]
    public string upgradeName = "Tap Strength";
    public int initialCost = 50;
    public float costMultiplier = 1.5f;

    [TextArea(2, 4)]
    public string description = "Increases your manual taps by 2% each time you buy it.";

    [Header("Level / Cap Settings")]
    public bool hasMaxLevel = false;
    public int maxLevel = 10;

    [Header("Button Components")]
    public Button upgradeButton;
    public TextMeshProUGUI buttonText;

    [HideInInspector] public int currentCost;
    [HideInInspector] public int currentLevel;

    private EggHatcher eggHatcher;
    private bool isHovering = false;
    private string costSaveKey;
    private string levelSaveKey;

    void Awake()
    {
        // Generate the save keys immediately during Awake so SaveManager can access them safely
        costSaveKey = "Cost_" + upgradeName.Replace(" ", "");
        levelSaveKey = "Level_" + upgradeName.Replace(" ", "");
    }

    void Start()
    {
        eggHatcher = Object.FindFirstObjectByType<EggHatcher>();
        costSaveKey = "Cost_" + upgradeName.Replace(" ", "");
        levelSaveKey = "Level_" + upgradeName.Replace(" ", "");

        currentLevel = PlayerPrefs.GetInt(levelSaveKey, 0);
        currentCost = PlayerPrefs.GetInt(costSaveKey, initialCost);

        // FIXED: Safety sync to prevent SaveManager's old JSON file variables 
        // from dragging the core currency tracking engine down below the button's starting cost!
        if (upgradeName == "Auto Hatcher" && eggHatcher != null)
        {
            if (eggHatcher.autoHatchCost < currentCost)
            {
                eggHatcher.autoHatchCost = currentCost;
            }
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(BuyUpgrade);
        }
        UpdateButtonDisplay();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        ShowFormattedTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (UpgradeTooltipManager.Instance != null)
        {
            UpgradeTooltipManager.Instance.HideTooltip();
        }
    }

    public void ShowFormattedTooltip()
    {
        if (UpgradeTooltipManager.Instance == null) return;

        string levelDisplay = (hasMaxLevel && currentLevel >= maxLevel) ? "Lvl. MAX" : $"Lvl. {currentLevel}";
        string formattedDescription = $"<color=#FFA500>[{levelDisplay}]</color>\n{description}";

        UpgradeTooltipManager.Instance.ShowTooltip(upgradeName, currentCost, formattedDescription);
    }

    public void BuyUpgrade()
    {
        if (eggHatcher == null) return;
        if (hasMaxLevel && currentLevel >= maxLevel) return;

        if (eggHatcher.eggsHatched >= currentCost)
        {
            eggHatcher.eggsHatched -= currentCost;
            PlayerPrefs.SetInt("EggsCurrency", eggHatcher.eggsHatched);

            currentLevel++;

            if (upgradeName == "Tap Strength")
            {
                eggHatcher.eggsPerClick = Mathf.CeilToInt(eggHatcher.eggsPerClick * 1.02f);
                currentCost = Mathf.RoundToInt(currentCost * costMultiplier);
            }
            else if (upgradeName == "Auto Hatcher")
            {
                eggHatcher.autoHatchCount++;
                eggHatcher.autoHatchCost = Mathf.RoundToInt(eggHatcher.autoHatchCost * 2.5f);
                currentCost = eggHatcher.autoHatchCost;
            }
            else if (upgradeName == "Hatch Speed")
            {
                eggHatcher.autoHatchInterval *= 0.99f;
                if (eggHatcher.autoHatchInterval < 1.0f)
                {
                    eggHatcher.autoHatchInterval = 1.0f;
                }
                currentCost = Mathf.RoundToInt(currentCost * costMultiplier);
                PlayerPrefs.SetFloat("AutoHatchInterval", eggHatcher.autoHatchInterval);
            }

            // Save variables to memory immediately
            PlayerPrefs.SetInt(costSaveKey, currentCost);
            PlayerPrefs.SetInt(levelSaveKey, currentLevel);
            PlayerPrefs.Save();

            // CRUCIAL: Force instant UI face redraw
            UpdateButtonDisplay();

            if (isHovering)
            {
                ShowFormattedTooltip();
            }

            eggHatcher.UpdateUI();
        }
    }

    public void UpdateButtonDisplay()
    {
        if (buttonText != null)
        {
            if (hasMaxLevel && currentLevel >= maxLevel)
            {
                buttonText.text = $"{upgradeName}\n<color=red>MAXED</color>";
                if (upgradeButton != null) upgradeButton.interactable = false;
            }
            else
            {
                buttonText.text = $"{upgradeName}\nCost: {currentCost}";
                if (upgradeButton != null) upgradeButton.interactable = true;
            }
        }
    }
}
