using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class AutoHatcherPurchaser : MonoBehaviour
{
    [Header("Linked Managers")]
    [SerializeField] private EggHatcher eggHatcher;

    [Header("UI Fields")]
    [SerializeField] private TextMeshProUGUI costText;

    [Header("Audio")]
    [Tooltip("Drag your upgrade purchase sound effect clip here.")]
    [SerializeField] private AudioClip purchaseSuccessSound;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    private Button customButton;
    private AudioSource dedicatedAudio;

    private void Awake()
    {
        customButton = GetComponent<Button>();
        dedicatedAudio = GetComponent<AudioSource>();

        // Configure audio settings to guarantee it bypasses 3D distance issues
        dedicatedAudio.playOnAwake = false;
        dedicatedAudio.loop = false;
        dedicatedAudio.spatialBlend = 0f; // Force 2D sound
    }

    private void OnEnable()
    {
        if (customButton != null)
        {
            customButton.onClick.AddListener(AttemptPurchase);
        }
    }

    private void OnDisable()
    {
        if (customButton != null)
        {
            customButton.onClick.RemoveListener(AttemptPurchase);
        }
    }

    private void Start()
    {
        if (eggHatcher == null)
        {
            eggHatcher = Object.FindFirstObjectByType<EggHatcher>();
        }

        RefreshCostDisplay();
    }

    private void Update()
    {
        // Press the Spacebar on your keyboard while playing the game to force-buy it
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[AutoHatcherPurchaser] Spacebar detected! Forcing purchase audio test...");
            AttemptPurchase();
        }
    }

    public void AttemptPurchase()
    {
        if (eggHatcher == null)
        {
            Debug.LogError("[AutoHatcherPurchaser] Cannot purchase! EggHatcher reference is missing.");
            return;
        }

        // Direct currency check using the current cost directly from EggHatcher
        if (eggHatcher.eggsHatched >= eggHatcher.autoHatchCost)
        {
            // 1. Play the sound IMMEDIATELY before any complex UI data saving runs
            if (dedicatedAudio != null && purchaseSuccessSound != null)
            {
                dedicatedAudio.PlayOneShot(purchaseSuccessSound, volume);
                Debug.Log($"[AutoHatcherPurchaser] PlayOneShot fired clip: {purchaseSuccessSound.name}");
            }

            // 2. Process the actual Upgrade purchase math
            eggHatcher.eggsHatched -= eggHatcher.autoHatchCost;
            eggHatcher.autoHatchCount++;

            // Scaled cost calculation matching your balance structure
            eggHatcher.autoHatchCost = Mathf.RoundToInt(eggHatcher.autoHatchCost * 2.5f);

            // 3. Save directly to memory registry keys
            PlayerPrefs.SetInt("EggsCurrency", eggHatcher.eggsHatched);
            PlayerPrefs.SetInt("Level_AutoHatcher", eggHatcher.autoHatchCount);
            PlayerPrefs.SetInt("Cost_AutoHatcher", eggHatcher.autoHatchCost);
            PlayerPrefs.Save();

            // 4. Force global game systems to sync graphics
            RefreshCostDisplay();
            eggHatcher.UpdateUI();

            // Force the auto-timer to reset so it doesn't instantly click on the same frame
            eggHatcher.ResetAutoHatchTimer();
        }
        else
        {
            Debug.LogWarning($"[AutoHatcherPurchaser] Not enough currency! Needs: {eggHatcher.autoHatchCost}, Has: {eggHatcher.eggsHatched}");
        }
    }

    private void RefreshCostDisplay()
    {
        if (costText != null && eggHatcher != null)
        {
            costText.text = $"Auto Hatcher\nCost: {eggHatcher.autoHatchCost}";
        }
    }
}
