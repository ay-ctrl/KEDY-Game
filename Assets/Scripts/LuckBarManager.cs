// LuckBarManager.cs
using UnityEngine;
using UnityEngine.UI;

public class LuckBarManager : MonoBehaviour
{
    public static LuckBarManager Instance;

    [Header("Luck Settings")]
    public float currentLuck = 50f;
    public float maxLuck = 100f;
    public float minLuck = 0f;

    [Header("UI")]
    public Slider luckSlider;
    public Image fillImage;
    public Color lowLuckColor = Color.red;
    public Color highLuckColor = Color.green;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start() => UpdateUI();

    public void ModifyLuck(float amount)
    {
        currentLuck = Mathf.Clamp(currentLuck + amount, minLuck, maxLuck);
        UpdateUI();
        Debug.Log($"Luck: {currentLuck} ({(amount >= 0 ? "+" : "")}{amount})");
    }

    void UpdateUI()
    {
        if (luckSlider != null)
        {
            luckSlider.value = currentLuck / maxLuck;
            if (fillImage != null)
                fillImage.color = Color.Lerp(lowLuckColor, highLuckColor, currentLuck / maxLuck);
        }
    }
}