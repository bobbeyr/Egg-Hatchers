using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using EggClickerGame;

public class EggController : MonoBehaviour
{
    public Sprite initialSprite;
    public Sprite brokenSprite;
    public Sprite[] crackSprites;
    public TextMeshProUGUI popupText;
    public AudioSource hatchSound;
    public Button eggButton;

    [Header("Audio Clips Configuration")]
    public AudioClip hatchCelebrationClip;
    public AudioClip[] crackSounds;

    private Image image;
    private int totalTapsInCurrentCycle = 0;
    private int cracksNeeded = 5;
    private bool canClick = true;
    private int lastPlayedStageIndex = -1;
    public bool IsBroken { get; private set; } = false;

    private EggHatcher eggHatcher;

    // Tracking variables for reset timing
    private bool isWaitingToReset = false;
    private float resetTimer = 0f;
    private const float HatchPopupDuration = 2f;

    void Awake()
    {
        image = GetComponent<Image>();
        if (image.sprite == null) image.sprite = initialSprite;
    }

    void Start()
    {
        if (popupText != null) popupText.gameObject.SetActive(false);
        if (eggButton != null) eggButton.interactable = !IsBroken;
        eggHatcher = Object.FindFirstObjectByType<EggHatcher>();
    }

    void Update()
    {
        if (isWaitingToReset)
        {
            resetTimer += Time.deltaTime;
            if (resetTimer >= HatchPopupDuration)
            {
                FinishHatchAndReset();
            }
        }
    }

    public int GetTotalTapsInCurrentCycle() => totalTapsInCurrentCycle;
    public int GetCracksNeeded() => cracksNeeded;

    public void LoadEggState(int savedTaps, int savedNeeded, bool savedIsBroken)
    {
        totalTapsInCurrentCycle = savedTaps;
        cracksNeeded = savedNeeded;
        IsBroken = savedIsBroken;
        canClick = !savedIsBroken;

        if (cracksNeeded > 0)
        {
            float progress = (float)totalTapsInCurrentCycle / cracksNeeded;
            lastPlayedStageIndex = Mathf.FloorToInt(progress * crackSprites.Length);
            lastPlayedStageIndex = Mathf.Clamp(lastPlayedStageIndex, -1, crackSprites.Length - 1);
        }
        if (image == null) image = GetComponent<Image>();
        UpdateVisuals();

        if (eggHatcher != null) eggHatcher.UpdateProgressBar();
    }

    private void UpdateVisuals()
    {
        if (IsBroken)
        {
            image.sprite = brokenSprite;
            if (eggButton != null) eggButton.interactable = false;
            return;
        }

        if (totalTapsInCurrentCycle == 0)
        {
            image.sprite = initialSprite;
            if (eggButton != null) eggButton.interactable = true;
            return;
        }

        float progress = (float)totalTapsInCurrentCycle / cracksNeeded;
        int stageIndex = Mathf.FloorToInt(progress * crackSprites.Length);
        stageIndex = Mathf.Clamp(stageIndex, 0, crackSprites.Length - 1);
        image.sprite = crackSprites[stageIndex];
        if (eggButton != null) eggButton.interactable = true;
    }

    public void HatchEgg(int tapAmount = 1)
    {
        if (!canClick || IsBroken || isWaitingToReset) return;

        totalTapsInCurrentCycle += tapAmount;

        if (eggHatcher != null) eggHatcher.UpdateProgressBar();

        if (totalTapsInCurrentCycle >= cracksNeeded)
        {
            TriggerEggHatchSequence();
        }
        else
        {
            ProcessCrackVisuals();
        }
    }

    public void ProcessAutoHatch(int autoHatchCount)
    {
        if (!canClick || IsBroken || isWaitingToReset) return;

        int stagesCount = crackSprites.Length > 0 ? crackSprites.Length : 5;
        int tapsPerStage = Mathf.Max(1, cracksNeeded / stagesCount);
        int allowedTaps = Mathf.Min(autoHatchCount, tapsPerStage);

        HatchEgg(allowedTaps);
    }

    private void TriggerEggHatchSequence()
    {
        canClick = false;
        IsBroken = true;
        isWaitingToReset = true;
        resetTimer = 0f;

        if (eggButton != null)
        {
            eggButton.interactable = false;
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == eggButton.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        image.sprite = brokenSprite;

        if (popupText != null)
        {
            popupText.text = "Congratulations, your egg has hatched!!!";
            popupText.gameObject.SetActive(true);
        }

        if (hatchSound != null && hatchCelebrationClip != null)
        {
            hatchSound.clip = hatchCelebrationClip;
            hatchSound.Play();
        }
    }

    private void ProcessCrackVisuals()
    {
        float progress = (float)totalTapsInCurrentCycle / cracksNeeded;
        int stageIndex = Mathf.FloorToInt(progress * crackSprites.Length);
        stageIndex = Mathf.Clamp(stageIndex, 0, crackSprites.Length - 1);
        image.sprite = crackSprites[stageIndex];

        if (stageIndex != lastPlayedStageIndex)
        {
            if (crackSounds != null && crackSounds.Length > 0 && stageIndex < crackSounds.Length)
            {
                if (hatchSound != null)
                {
                    hatchSound.clip = crackSounds[stageIndex];
                    hatchSound.Play();
                }
            }
            lastPlayedStageIndex = stageIndex;
        }
    }

    private void FinishHatchAndReset()
    {
        isWaitingToReset = false;
        if (popupText != null) popupText.gameObject.SetActive(false);
        cracksNeeded = Mathf.CeilToInt(cracksNeeded * 1.5f);
        ResetEgg();

        // Trigger the new visual roll whenever an egg fully cracks open!
        if (CreatureJournalManager.Instance != null)
        {
            Creature randomDrop = CreatureJournalManager.Instance.RollRandomCreature();
            CreatureJournalManager.Instance.AddCreatureToCollection(randomDrop);
        }
    }

    public bool SimulateOfflineTap()
    {
        totalTapsInCurrentCycle++;
        if (totalTapsInCurrentCycle >= cracksNeeded)
        {
            FinishHatchAndReset();
            return true;
        }
        return false;
    }

    public void ResetEgg()
    {
        totalTapsInCurrentCycle = 0;
        IsBroken = false;
        isWaitingToReset = false;
        lastPlayedStageIndex = -1;
        image.sprite = initialSprite;
        canClick = true;
        if (eggButton != null) eggButton.interactable = true;
        if (popupText != null) popupText.gameObject.SetActive(false);
        if (eggHatcher != null) eggHatcher.UpdateProgressBar();
    }
}