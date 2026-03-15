using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LuckBarManager : MonoBehaviour
{
    public static LuckBarManager Instance;

    [Header("Luck Settings")]
    public float currentLuck = 50f;
    public float maxLuck = 100f;
    public float minLuck = 0f;

    Slider luckSlider;
    Image fillImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUI();
        UpdateUI();
    }

    void FindUI()
    {
        luckSlider = FindFirstObjectByType<Slider>();

        if (luckSlider != null)
            fillImage = luckSlider.fillRect.GetComponent<Image>();
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