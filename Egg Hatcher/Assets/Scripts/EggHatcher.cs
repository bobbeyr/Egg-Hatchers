using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections; // Required for Coroutines
using UnityEngine.InputSystem;
using EggClickerGame; // Assuming this is your namespace

public class EggHatcher : MonoBehaviour
{
    public int eggsHatched = 0;
    public int eggsPerClick = 1;
    public int autoHatchCount = 0;

    public TMP_Text eggsText;
    public Button hatchButton;
    public Button autoHatchUpgradeButton;
    public TMP_Text autoHatchCostText;
    public GameObject ripplePrefab;
    public Canvas uiCanvas;
    public AudioSource clickSound;
    public SaveManager saveManager;
    public SettingsManager settingsManager;
    public int autoHatchCost = 10;

    private float autoHatchInterval = 1f;
    private float autoHatchTimer = 0f;
    public EggController eggController;

    [Header("Offline Notification UI")]
    public GameObject offlinePopupPanel;
    public TMP_Text offlinePopupText;
    public Button offlineCloseButton;
    public GameObject popupShadowMask; // Assign full-screen shadow mask image here

    private CanvasGroup popupCanvasGroup;
    private CanvasGroup maskCanvasGroup; // Cached group for the backdrop fade

    void Start()
    {
        eggsHatched = PlayerPrefs.GetInt("EggsCurrency", 0);

        // Cache CanvasGroup for popup panel
        if (offlinePopupPanel != null)
        {
            popupCanvasGroup = offlinePopupPanel.GetComponent<CanvasGroup>();
            if (popupCanvasGroup == null)
                popupCanvasGroup = offlinePopupPanel.AddComponent<CanvasGroup>();
        }

        // Cache CanvasGroup for shadow mask
        if (popupShadowMask != null)
        {
            maskCanvasGroup = popupShadowMask.GetComponent<CanvasGroup>();
            if (maskCanvasGroup == null)
                maskCanvasGroup = popupShadowMask.AddComponent<CanvasGroup>();
        }

        // Setup close button listener
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

        if (autoHatchUpgradeButton != null)
        {
            autoHatchUpgradeButton.onClick.RemoveAllListeners();
            autoHatchUpgradeButton.onClick.AddListener(BuyAutoHatch);
        }

        UpdateUI();
    }

    void Update()
    {
        if (autoHatchCount > 0 && eggController != null && !eggController.IsBroken)
        {
            autoHatchTimer += Time.deltaTime;
            if (autoHatchTimer >= autoHatchInterval)
            {
                eggController.HatchEgg();
                AddAutoTaps(autoHatchCount * eggsPerClick);
                autoHatchTimer = 0f;
            }
        }
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
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(uiCanvas.transform as RectTransform, mousePosition, uiCanvas.worldCamera, out worldPos))
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

        int absoluteTotal = PlayerPrefs.GetInt("TotalTaps", 0);
        absoluteTotal += amount;
        PlayerPrefs.SetInt("TotalTaps", absoluteTotal);
        PlayerPrefs.Save();

        UpdateUI();

        if (settingsManager != null)
            settingsManager.UpdateTotalTapsText();
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
                bool didHatch = eggController.SimulateOfflineTap();
                if (didHatch)
                {
                    offlineHatchesCount++;
                }
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

            // Reset scales and alpha
            panelTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            if (popupCanvasGroup != null) popupCanvasGroup.alpha = 0f;

            // Activate mask backdrop
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
                if (popupCanvasGroup != null)
                    popupCanvasGroup.alpha = Mathf.Clamp01(normalizedTime * 1.5f);
                if (maskCanvasGroup != null)
                    maskCanvasGroup.alpha = Mathf.Clamp01(normalizedTime);
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

    public void BuyAutoHatch()
    {
        if (eggsHatched >= autoHatchCost)
        {
            eggsHatched -= autoHatchCost;
            PlayerPrefs.SetInt("EggsCurrency", eggsHatched);
            PlayerPrefs.Save();

            autoHatchCount++;
            autoHatchCost = Mathf.RoundToInt(autoHatchCost * 1.5f);
            UpdateUI();
        }
    }

    public void SaveGame()
    {
        if (saveManager != null)
            saveManager.SaveGame(this);
    }

    public void UpdateUI()
    {
        if (eggsText != null)
            eggsText.text = "Taps: " + eggsHatched;
        if (autoHatchCostText != null)
            autoHatchCostText.text = "Cost: " + autoHatchCost;
    }

    public void ResetGame()
    {
        eggsHatched = 0;
        autoHatchCount = 0;
        eggsPerClick = 1;
        autoHatchCost = 10;
        PlayerPrefs.SetInt("EggsCurrency", 0);
        PlayerPrefs.SetInt("TotalTaps", 0);
        PlayerPrefs.Save();
        UpdateUI();
        if (settingsManager != null)
            settingsManager.UpdateTotalTapsText();
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}