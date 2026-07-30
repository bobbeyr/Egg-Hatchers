using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RippleEffect : MonoBehaviour
{
    public float duration = 0.5f; // how long the ripple lasts
    public float maxSize = 100f;  // maximum size of ripple

    private Image rippleImage;

    void Awake()
    {
        rippleImage = GetComponent<Image>();
    }

    public void Play(Vector3 position)
    {
        // Set initial state
        rippleImage.rectTransform.sizeDelta = Vector2.zero;   // start small
        rippleImage.color = new Color(rippleImage.color.r, rippleImage.color.g, rippleImage.color.b, 1f); // full alpha
        // Animate ripple
        StartCoroutine(AnimateRipple());
    }

    private IEnumerator AnimateRipple()
    {
        float timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;
            // Expand size
            float size = Mathf.Lerp(0, maxSize, t);
            rippleImage.rectTransform.sizeDelta = new Vector2(size, size);
            // Fade out
            rippleImage.color = new Color(rippleImage.color.r, rippleImage.color.g, rippleImage.color.b, 1 - t);
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}