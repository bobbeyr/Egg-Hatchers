using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class ButtonSoundEffect : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Drag your custom click sound clip here.")]
    [SerializeField] private AudioClip clickSound;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private Button button;
    private AudioSource audioSource;

    private void Awake()
    {
        // Fetch references from the GameObject
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();

        // Configure the audio source to behave properly for UI sounds
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // Forces 2D sound for UI
    }

    private void OnEnable()
    {
        // Hook up our play method to the button's click event
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void OnDisable()
    {
        // Unhook the event when disabled to prevent memory leaks
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        Debug.Log($"[ButtonSoundEffect] Click detected on: {gameObject.name}");

        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
            Debug.Log($"[ButtonSoundEffect] Played clip: {clickSound.name} at volume {volume}");
        }
        else
        {
            Debug.LogWarning($"[ButtonSoundEffect] Failed to play! Click Sound clip null: {clickSound == null}, AudioSource null: {audioSource == null}", gameObject);
        }
    }
}
