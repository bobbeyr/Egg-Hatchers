using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI; // For Button
using UnityEngine.InputSystem; // For new Input System

public class EggHatcher : MonoBehaviour
{
    public int eggsHatched = 0;                // Total eggs hatched (currency)
    public int eggsPerClick = 1;                // Eggs gained per click

    public TMP_Text eggsText;                   // TMP Text to display eggs count
    public Button hatchButton;                  // Button to hatch eggs
    public Button autoHatchUpgradeButton;       // Button to buy auto hatch
    public TMP_Text autoHatchCostText;          // TMP Text to display auto hatch cost

    public GameObject ripplePrefab;             // Ripple effect prefab
    public Canvas uiCanvas;                     // Canvas for ripple positioning

    public AudioSource clickSound;              // AudioSource for click sound

    private int autoHatchCost = 10;             // Initial cost for auto hatch
    private int autoHatchCount = 0;             // Number of auto hatches bought
    private float autoHatchInterval = 1f;       // Time between auto hatches
    private float autoHatchTimer = 0f;          // Timer for auto hatching

    void Start()
    {
        UpdateUI();

        // Add listeners to buttons
        if (hatchButton != null)
        {
            hatchButton.onClick.RemoveAllListeners();
            hatchButton.onClick.AddListener(HatchEgg);
        }
        if (autoHatchUpgradeButton != null)
        {
            autoHatchUpgradeButton.onClick.RemoveAllListeners();
            autoHatchUpgradeButton.onClick.AddListener(BuyAutoHatch);
        }
    }

    void Update()
    {
        if (autoHatchCount > 0)
        {
            autoHatchTimer += Time.deltaTime;
            if (autoHatchTimer >= autoHatchInterval)
            {
                HatchEgg(autoHatchCount);
                autoHatchTimer = 0f;
            }
        }
    }

    // Method called when clicking the egg
    public void HatchEgg()
    {
        Debug.Log("HatchEgg called");
        eggsHatched += eggsPerClick;
        UpdateUI();

        // Play click sound
        if (clickSound != null)
        {
            clickSound.Play();
        }

        // Get mouse position using new Input System
        Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        // Convert screen point to local point in the Canvas
        Vector2 clickPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiCanvas.transform as RectTransform,
                mousePosition,
                uiCanvas.worldCamera,
                out clickPos))
        {
            // Convert local point to world position
            Vector3 worldPos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    uiCanvas.transform as RectTransform,
                    mousePosition,
                    uiCanvas.worldCamera,
                    out worldPos))
            {
                if (ripplePrefab != null)
                {
                    GameObject ripple = Instantiate(ripplePrefab, worldPos, Quaternion.identity, uiCanvas.transform);
                    ripple.GetComponent<RippleEffect>().Play(worldPos);
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
            autoHatchCost = Mathf.RoundToInt(autoHatchCost * 1.5f); // Increase cost each purchase
            UpdateUI();
        }
    }

    private void HatchEgg(int count)
    {
        eggsHatched += eggsPerClick * count;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (eggsText != null)
            eggsText.text = "Eggs: " + eggsHatched;
        if (autoHatchCostText != null)
            autoHatchCostText.text = "Cost: " + autoHatchCost;
    }
}