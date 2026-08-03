using UnityEngine;

public class UIFloatingObject : MonoBehaviour
{
    [Header("Float Settings")]
    [Tooltip("How high and low the object will float.")]
    public float floatAmplitude = 15f;

    [Tooltip("How fast the object will float up and down.")]
    public float floatSpeed = 2f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Awake()
    {
        // Cache the RectTransform component of the UI image
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            startPosition = rectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        if (rectTransform == null) return;

        // Use a Sine wave based on time to calculate a smooth up and down offset
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // Apply the new position cleanly to the UI coordinates
        rectTransform.anchoredPosition = new Vector2(startPosition.x, newY);
    }
}
