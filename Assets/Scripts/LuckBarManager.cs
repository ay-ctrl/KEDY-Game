using UnityEngine;
using UnityEngine.UI;

public class LuckBarManager : MonoBehaviour
{
    public static LuckBarManager Instance;

    [Header("Luck Settings")]
    public float currentLuck = 50f;
    public float maxLuck = 100f;
    public float minLuck = 0f;

    Slider luckSlider;
    Image fillImage;

    private void Start()
    {
        UpdateUI();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ModifyLuck(float amount)
    {
        currentLuck = Mathf.Clamp(currentLuck + amount, minLuck, maxLuck);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (luckSlider != null)
        {
            luckSlider.value = currentLuck / maxLuck;

            if (fillImage != null)
                fillImage.color = Color.Lerp(Color.red, Color.green, currentLuck / maxLuck);
        }
    }
}