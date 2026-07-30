using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EggHatcher : MonoBehaviour
{
    public int eggsHatched = 0; // internally still as eggsHatched, but represents taps now
    public int eggsPerClick = 1; // tapsPerClick
    public int autoHatchCount = 0;

    public TMP_Text eggsText;
    public Button hatchButton;
    public Button autoHatchUpgradeButton;
    public TMP_Text autoHatchCostText;

    public GameObject ripplePrefab;
    public Canvas uiCanvas;

    public AudioSource clickSound;

    public SaveManager saveManager;

    public int autoHatchCost = 10; // Starting cost

    private float autoHatchInterval = 1f;
    private float autoHatchTimer = 0f;

    // Reference to SettingsManager, assign in inspector
    public SettingsManager settingsManager;

    void Start()
    {
        if (saveManager != null)
        {
            saveManager.LoadGame(this);
        }

        // Assign button callbacks
        if (hatchButton != null)
        {
            hatchButton.onClick.RemoveAllListeners();
            // When clicked, hatch 1 tap and update total taps
            hatchButton.onClick.AddListener(() =>
            {
                HatchEgg(1, true);
                if (settingsManager != null)
                {
                    settingsManager.AddTaps(eggsPerClick);
                }
            });
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
        if (autoHatchCount > 0)
        {
            autoHatchTimer += Time.deltaTime;
            if (autoHatchTimer >= autoHatchInterval)
            {
                // Auto hatch with total taps from all auto hatcher
                HatchEgg(autoHatchCount, false);
                autoHatchTimer = 0f;
            }
        }
    }

    public void HatchEgg(int count, bool isClick)
    {
        eggsHatched += eggsPerClick * count; // taps count
        UpdateUI();

        if (clickSound != null)
            clickSound.Play();

        if (isClick)
        {
            Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    uiCanvas.transform as RectTransform,
                    mousePosition,
                    uiCanvas.worldCamera,
                    out Vector2 clickPos))
            {
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        uiCanvas.transform as RectTransform,
                        mousePosition,
                        uiCanvas.worldCamera,
                        out Vector3 worldPos))
                {
                    if (ripplePrefab != null)
                    {
                        GameObject ripple = Instantiate(ripplePrefab, worldPos, Quaternion.identity, uiCanvas.transform);
                        ripple.GetComponent<RippleEffect>().Play(worldPos);
                    }
                }
            }
        }
    }

    public void BuyAutoHatch()
    {
        if (eggsHatched >= autoHatchCost)
        {
            eggsHatched -= autoHatchCost;
            autoHatchCount++;
            autoHatchCost = Mathf.RoundToInt(autoHatchCost * 1.5f);
            UpdateUI();
        }
    }

    public void SaveGame()
    {
        if (saveManager != null)
        {
            saveManager.SaveGame(this);
        }
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
        UpdateUI();
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}