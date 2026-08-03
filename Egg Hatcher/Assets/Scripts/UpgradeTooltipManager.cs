using UnityEngine;
using TMPro;

public class UpgradeTooltipManager : MonoBehaviour
{
    // Singleton pattern allows any button to access this manager easily
    public static UpgradeTooltipManager Instance { get; private set; }

    [Header("UI Component Linkages")]
    public RectTransform tooltipPanel;
    public TextMeshProUGUI tooltipText;

    [Header("Position Tuning")]
    public Vector2 cursorOffset = new Vector2(15f, -15f);

    private bool IsActive => tooltipPanel != null && tooltipPanel.gameObject.activeSelf;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HideTooltip();
    }

    void Update()
    {
        // Automatically follow the cursor when active
        if (IsActive)
        {
            Vector2 mousePos = Input.mousePosition;
            tooltipPanel.position = mousePos + cursorOffset;
        }
    }

    /// <summary>
    /// Updates and reveals the single shared tooltip instance.
    /// </summary>
    public void ShowTooltip(string title, int cost, string message)
    {
        if (tooltipText != null)
        {
            tooltipText.text = $"<b>{title}</b>\nCost: {cost}\n{message}";
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Shuts off the shared visual elements.
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
        if (tooltipText != null)
        {
            tooltipText.text = "";
        }
    }
}
