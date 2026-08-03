using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using EggClickerGame;

public class EggHatcher : MonoBehaviour
{
    // Egg Data
    public int eggsHatched = 0;
    public int eggsPerClick = 1;
    public int autoHatchCount = 0;

    // UI References
    public TMP_Text eggsText;
    public Button hatchButton;
    public Button autoHatchUpgradeButton;
    public TMP_Text autoHatchCostText;
    public Slider eggProgressBar;

    // Ripple Effect
    public GameObject ripplePrefab;
    public Canvas uiCanvas;

    // Audio
    public AudioSource clickSound;

    // Managers
    public SaveManager saveManager;
    public SettingsManager settingsManager;
    public EggController eggController;

    // Auto Hatcher Config
    public int autoHatchCost = 10;
    public float autoHatchInterval = 5f;
    private float autoHatchTimer = 0f;

    // Offline Notification UI
    [Header("Offline Notification UI")]
    public GameObject offlinePopupPanel;
    public TMP_Text offlinePopupText;
    public Button offlineCloseButton;
    public GameObject popupShadowMask;
    private CanvasGroup popupCanvasGroup;
    private CanvasGroup maskCanvasGroup;

    // Auto Save Timer
    private float autoSaveTimer = 0f;
    private const float AutoSaveInterval = 30f;

    void Start()
    {
        // Load saved eggs count
        eggsHatched = PlayerPrefs.GetInt("EggsCurrency", 0);
        // Load saved autoHatchInterval
        autoHatchInterval = PlayerPrefs.GetFloat("AutoHatchInterval", 5f);

        // Initialize CanvasGroups for popup
        if (offlinePopupPanel != null)
        {
            popupCanvasGroup = offlinePopupPanel.GetComponent<CanvasGroup>() ?? offlinePopupPanel.AddComponent<CanvasGroup>();
        }
        if (popupShadowMask != null)
        {
            maskCanvasGroup = popupShadowMask.GetComponent<CanvasGroup>() ?? popupShadowMask.AddComponent<CanvasGroup>();
        }

        // Setup button listeners
        if (offlineCloseButton != null)
        {
            offlineCloseButton.onClick.RemoveAllListeners();
            offlineCloseButton.onClick.AddListener(OnCloseButtonClicked);
        }
        if (saveManager != null) saveManager.LoadGame(this);
        if (hatchButton != null)
        {
            hatchButton.onClick.RemoveAllListeners();
            hatchButton.onClick.AddListener(OnEggClicked);
        }

        UpdateUI();
    }

    void Update()
    {
        // Handle Auto Hatching
        if (autoHatchCount > 0 && eggController != null && !eggController.IsBroken)
        {
            autoHatchTimer += Time.deltaTime;
            if (autoHatchTimer >= autoHatchInterval)
            {
                eggController.ProcessAutoHatch(autoHatchCount);
                AddAutoTaps(autoHatchCount);
                autoHatchTimer = 0f;
            }
        }

        // Auto Save Loop
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= AutoSaveInterval)
        {
            SaveGame();
            autoSaveTimer = 0f;
        }

        // Update Progress Bar
        UpdateProgressBar();
    }

    public void OnEggClicked()
    {
        if (eggController != null && !eggController.IsBroken)
        {
            eggController.HatchEgg();

            if (ripplePrefab != null && uiCanvas != null)
            {
                Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
                Vector3 worldPos;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        uiCanvas.transform as RectTransform, mousePosition, uiCanvas.worldCamera, out worldPos))
                {
                    GameObject ripple = Instantiate(ripplePrefab, worldPos, Quaternion.identity, uiCanvas.transform);
                    ripple.GetComponent<RippleEffect>().Play(worldPos);
                }
            }
            AddManualPlayerTaps(eggsPerClick);
        }
    }

    public void AddManualPlayerTaps(int amount)
    {
        eggsHatched += amount;
        PlayerPrefs.SetInt("EggsCurrency", eggsHatched);
        PlayerPrefs.SetInt("TotalTaps", PlayerPrefs.GetInt("TotalTaps", 0) + amount);
        PlayerPrefs.Save();
        UpdateUI();
        if (settingsManager != null) settingsManager.UpdateTotalTapsText();
    }

    public void AddAutoTaps(int amount)
    {
        eggsHatched += amount;
        PlayerPrefs.SetInt("EggsCurrency", eggsHatched);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void AddOfflineProgressTaps(int totalOfflineTaps)
    {
        eggsHatched += totalOfflineTaps;
        PlayerPrefs.SetInt("EggsCurrency", eggsHatched);
        PlayerPrefs.Save();

        int offlineHatchesCount = 0;
        if (eggController != null)
        {
            for (int i = 0; i < totalOfflineTaps; i++)
            {
                if (eggController.SimulateOfflineTap()) offlineHatchesCount++;
            }
            eggController.LoadEggState(
                eggController.GetTotalTapsInCurrentCycle(),
                eggController.GetCracksNeeded(),
                false
            );
        }

        if (totalOfflineTaps > 0)
        {
            ShowOfflineSummaryPopup(totalOfflineTaps, offlineHatchesCount);
        }
    }

    private void ShowOfflineSummaryPopup(int totalTapsGained, int totalHatchesGained)
    {
        if (offlinePopupPanel != null && offlinePopupText != null)
        {
            offlinePopupText.text = $"Welcome Back!\n\nWhile you were away, your auto-hatcher generated <color=green>+{totalTapsGained}</color> spendable taps, hatching your egg <color=yellow>{totalHatchesGained} times!</color>";
            StartCoroutine(FadeInPopupRoutine());
        }
    }

    private void OnCloseButtonClicked()
    {
        StartCoroutine(AnimateCloseButtonRoutine());
    }

    private IEnumerator FadeInPopupRoutine()
    {
        if (offlinePopupPanel != null)
        {
            Transform panelTransform = offlinePopupPanel.transform;
            panelTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            if (popupCanvasGroup != null) popupCanvasGroup.alpha = 0f;

            if (popupShadowMask != null)
            {
                if (maskCanvasGroup != null) maskCanvasGroup.alpha = 0f;
                popupShadowMask.SetActive(true);
            }
            offlinePopupPanel.SetActive(true);

            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / duration;
                if (popupCanvasGroup != null) popupCanvasGroup.alpha = Mathf.Clamp01(normalizedTime * 1.5f);
                if (maskCanvasGroup != null) maskCanvasGroup.alpha = Mathf.Clamp01(normalizedTime);
                float scaleMultiplier = Mathf.Sin(normalizedTime * Mathf.PI * 0.75f) * 1.08f;
                if (normalizedTime > 0.65f)
                    scaleMultiplier = Mathf.Lerp(scaleMultiplier, 1f, (normalizedTime - 0.65f) / 0.35f);
                panelTransform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
                yield return null;
            }
            panelTransform.localScale = Vector3.one;
            if (popupCanvasGroup != null) popupCanvasGroup.alpha = 1f;
            if (maskCanvasGroup != null) maskCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator AnimateCloseButtonRoutine()
    {
        if (offlineCloseButton != null)
        {
            Transform buttonTransform = offlineCloseButton.transform;
            Vector3 originalScale = Vector3.one;
            Vector3 targetScale = new Vector3(0.85f, 0.85f, 0.85f);
            float duration = 0.12f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                buttonTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                yield return null;
            }
            buttonTransform.localScale = targetScale;

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                buttonTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                yield return null;
            }
            buttonTransform.localScale = originalScale;
        }

        float fadeOutDuration = 0.2f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeOutDuration)
        {
            fadeElapsed += Time.deltaTime;
            float t = 1f - (fadeElapsed / fadeOutDuration);
            if (popupCanvasGroup != null) popupCanvasGroup.alpha = t;
            if (maskCanvasGroup != null) maskCanvasGroup.alpha = t;
            yield return null;
        }
        if (offlinePopupPanel != null) offlinePopupPanel.SetActive(false);
        if (popupShadowMask != null) popupShadowMask.SetActive(false);
    }

    public void SaveGame()
    {
        if (saveManager != null) saveManager.SaveGame(this);
    }

    public void UpdateUI()
    {
        if (eggsText != null) eggsText.text = "Taps: " + eggsHatched;
        if (autoHatchCostText != null) autoHatchCostText.text = "Cost: " + autoHatchCost;
        UpdateProgressBar();
    }

    public void UpdateProgressBar()
    {
        if (eggProgressBar != null && eggController != null)
        {
            float currentTaps = eggController.GetTotalTapsInCurrentCycle();
            float neededTaps = eggController.GetCracksNeeded();
            eggProgressBar.maxValue = neededTaps;
            eggProgressBar.value = currentTaps;
        }
    }

    public void ResetGame()
    {
        eggsHatched = 0;
        autoHatchCount = 0;
        eggsPerClick = 1;
        autoHatchCost = 10;
        autoHatchInterval = 5f;

        PlayerPrefs.SetInt("EggsCurrency", 0);
        PlayerPrefs.SetInt("TotalTaps", 0);

        // Clear upgrade keys
        PlayerPrefs.DeleteKey("Cost_TapStrength");
        PlayerPrefs.DeleteKey("Level_TapStrength");
        PlayerPrefs.DeleteKey("Cost_AutoHatcher");
        PlayerPrefs.DeleteKey("Level_AutoHatcher");
        PlayerPrefs.DeleteKey("Cost_HatchSpeed");
        PlayerPrefs.DeleteKey("Level_HatchSpeed");
        PlayerPrefs.DeleteKey("AutoHatchInterval");
        PlayerPrefs.Save();

        // FIXED: Changed FindObjectsOfType to FindObjectsByType to clear console warnings completely
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

        if (settingsManager != null) settingsManager.UpdateTotalTapsText();

        UpdateUI();

        // Reset egg controller state
        if (eggController != null)
        {
            eggController.ResetEgg();
            eggController.LoadEggState(0, 5, false);
        }
    }


    void OnApplicationQuit()
    {
        SaveGame();
    }
}