using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class EggController : MonoBehaviour
{
    public Sprite initialSprite;
    public Sprite brokenSprite;
    public Sprite[] crackSprites;
    public TextMeshProUGUI popupText;
    public AudioSource hatchSound; // The shared audio engine component player
    public Button eggButton;

    [Header("Audio Clips Configuration")]
    public AudioClip hatchCelebrationClip; // FIX: New explicit asset slot for the final hatch sound
    public AudioClip[] crackSounds;

    private Image image;
    private int totalTapsInCurrentCycle = 0;
    private int cracksNeeded = 5;
    private bool canClick = true;
    private int lastPlayedStageIndex = -1;

    public bool IsBroken { get; private set; } = false;

    void Awake()
    {
        image = GetComponent<Image>();
        if (image.sprite == null) image.sprite = initialSprite;
    }

    void Start()
    {
        if (popupText != null)
            popupText.gameObject.SetActive(false);

        if (eggButton != null)
            eggButton.interactable = !IsBroken;
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

    public void HatchEgg()
    {
        if (!canClick || IsBroken) return;

        totalTapsInCurrentCycle++;

        if (totalTapsInCurrentCycle >= cracksNeeded)
        {
            canClick = false;
            IsBroken = true;

            if (eggButton != null)
            {
                eggButton.interactable = false;
                if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == eggButton.gameObject)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }

        StartCoroutine(HatchRoutine());
    }

    // NEW: Simulates a single quick click from offline progress, returning true if it caused a hatch
    public bool SimulateOfflineTap()
    {
        totalTapsInCurrentCycle++;

        if (totalTapsInCurrentCycle >= cracksNeeded)
        {
            // Increase difficulty for the next cycle immediately
            cracksNeeded = Mathf.CeilToInt(cracksNeeded * 1.5f);

            // Reset counters for the next egg cycle
            totalTapsInCurrentCycle = 0;
            lastPlayedStageIndex = -1;

            return true; // The egg successfully hatched!
        }
        return false; // Egg cracked, but didn't hatch yet
    }


    private IEnumerator HatchRoutine()
    {
        float progress = (float)totalTapsInCurrentCycle / cracksNeeded;

        if (totalTapsInCurrentCycle >= cracksNeeded)
        {
            image.sprite = brokenSprite;
            yield return StartCoroutine(ShowHatchPopup());

            cracksNeeded = Mathf.CeilToInt(cracksNeeded * 1.5f);
            ResetEgg();
        }
        else
        {
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
    }

    IEnumerator ShowHatchPopup()
    {
        if (popupText != null)
        {
            popupText.text = "Congratulations, your egg has hatched!!!";
            popupText.gameObject.SetActive(true);
        }

        // FIX: Force the clip engine to load the targeted hatch sound asset explicitly 
        if (hatchSound != null && hatchCelebrationClip != null)
        {
            hatchSound.clip = hatchCelebrationClip;
            hatchSound.Play();
        }

        yield return new WaitForSeconds(2f);

        if (popupText != null)
            popupText.gameObject.SetActive(false);
    }

    public void ResetEgg()
    {
        totalTapsInCurrentCycle = 0;
        IsBroken = false;
        lastPlayedStageIndex = -1;
        image.sprite = initialSprite;
        canClick = true;
        if (eggButton != null) eggButton.interactable = true;
    }
}
