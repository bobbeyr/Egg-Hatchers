using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EggController : MonoBehaviour
{
    public Sprite initialSprite;          // The intact egg sprite
    public Sprite brokenSprite;           // The broken egg sprite
    public Sprite[] crackSprites;         // Visual crack sprites
    public TextMeshProUGUI popupText;     // Assign in inspector
    public AudioSource hatchSound;        // Assign in inspector for hatch sound

    private Image image;
    private int crackCount = 0;            // current crack progress
    private int cracksNeeded = 4;          // cracks needed before breaking
    private bool isBroken = false;

    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = initialSprite; // start with initial sprite
        if (popupText != null)
        {
            popupText.gameObject.SetActive(false);
        }
    }

    public void OnEggTapped()
    {
        if (!isBroken)
        {
            crackCount++;

            // Optional: update crack sprite for visual cracking
            if (crackSprites != null && crackSprites.Length > 0)
            {
                int spriteIndex = Mathf.Min(crackCount - 1, crackSprites.Length - 1);
                image.sprite = crackSprites[spriteIndex];
            }

            // When crackCount exceeds cracksNeeded, break the egg
            if (crackCount > cracksNeeded)
            {
                // Egg breaks
                isBroken = true;
                image.sprite = brokenSprite;
                StartCoroutine(ShowHatchPopup());
            }
        }
        else
        {
            // Reset after hatch
            ResetEgg();
            IncreaseCracksNeeded();
        }
    }

    IEnumerator ShowHatchPopup()
    {
        if (popupText != null)
        {
            popupText.text = "Congratulations, your egg has hatched!!!";
            popupText.gameObject.SetActive(true);
        }

        // Play hatch sound if assigned
        if (hatchSound != null)
        {
            hatchSound.Play();
        }

        yield return new WaitForSeconds(2f);

        if (popupText != null)
        {
            popupText.gameObject.SetActive(false);
        }
    }

    void ResetEgg()
    {
        crackCount = 0;
        isBroken = false;
        image.sprite = initialSprite; // reset to initial sprite
    }

    void IncreaseCracksNeeded()
    {
        // Increase the cracks needed for the next cycle to make cracking take longer
        if (cracksNeeded == 4)
            cracksNeeded = 10;
        else if (cracksNeeded == 10)
            cracksNeeded = 20;
        else if (cracksNeeded == 20)
            cracksNeeded = 50;
        else if (cracksNeeded == 50)
            cracksNeeded = 150;
        else
            cracksNeeded *= 3;
    }
}