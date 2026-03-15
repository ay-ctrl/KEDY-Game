using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WaveMinigame : MonoBehaviour
{
    [Header("Panel")]
    public GameObject minigamePanel;

    [Header("Wave UI")]
    public RectTransform[] waveLines;      // 4 adet RectTransform — her dalga bir line
      // 4 adet hedef (düz çizgi pozisyonu)
    public Scrollbar tunerKnob;            // düðme (scrollbar olarak)

    [Header("Timer")]
    public Image timerBar;                 // süre dolunca kýrmýzýya döner
    public float maxTime = 15f;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI timerText;
    public GameObject successPanel;
    public GameObject roomDarkOverlay;     // karanlýk overlay — tamamlanýnca kapanýr

    [Header("Luck")]
    public float fastBonus = -5f;   // çok hýzlý bitirince þans azalýr
    public float normalBonus = +15f;  // normal sürede þans artar
    public float fastThreshold = 4f;  // kaç saniyede bitirirse "hýzlý" sayýlýr

    // Ýç state
    float[] waveOffsets;       // her dalganýn rastgele Y offset'i
    float[] waveAmplitudes;    // her dalganýn genliði
    float[] waveSpeeds;        // her dalganýn akýþ hýzý
    float currentTime = 0f;
    bool isActive = false;
    bool isSolved = false;
    float knobValue = 0f;   // 0..1 arasý
    float solveThreshold = 0.05f; // ne kadar yakýn olursa "düzleþti" sayýlýr

    System.Action<bool> onComplete;

    // Her dalganýn "hedef" knob deðeri — hepsi 0.5'te düz
    float targetKnobValue = 0.5f;

    // ????????????????????????????????????????
    public void StartMinigame(System.Action<bool> callback)
    {
        onComplete = callback;
        isActive = true;
        isSolved = false;
        currentTime = 0f;

        minigamePanel.SetActive(true);
        successPanel.SetActive(false);
        feedbackText.text = "";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Rastgele dalga parametreleri oluþtur
        waveOffsets = new float[4];
        waveAmplitudes = new float[4];
        waveSpeeds = new float[4];

        for (int i = 0; i < 4; i++)
        {
            waveOffsets[i] = Random.Range(-40f, 40f);
            waveAmplitudes[i] = Random.Range(20f, 50f);
            waveSpeeds[i] = Random.Range(0.5f, 2f);
        }

        // Knob'u ortaya al
        tunerKnob.value = 0.5f;
        knobValue = 0.5f;

        StartCoroutine(RunMinigame());
    }

    // ????????????????????????????????????????
    IEnumerator RunMinigame()
    {
        while (isActive && !isSolved)
        {
            currentTime += Time.deltaTime;
            UpdateTimerUI();
            UpdateWaves();
            CheckSolved();
            yield return null;
        }
    }

    // ????????????????????????????????????????
    void Update()
    {
        if (!isActive) return;
        knobValue = tunerKnob.value;
    }

    // ????????????????????????????????????????
    void UpdateWaves()
    {
        // knob 0.5'e ne kadar yakýnsa dalgalar o kadar düzleþir
        float flatness = 1f - Mathf.Abs(knobValue - targetKnobValue) / 0.5f;
        flatness = Mathf.Clamp01(flatness);

        for (int i = 0; i < waveLines.Length; i++)
        {
            float wave = Mathf.Sin(
                (Time.time * waveSpeeds[i]) + waveOffsets[i]
            ) * waveAmplitudes[i] * (1f - flatness);

            Vector2 pos = waveLines[i].anchoredPosition;
            pos.y = wave;
            waveLines[i].anchoredPosition = pos;
        }
    }

    // ????????????????????????????????????????
    void CheckSolved()
    {
        float diff = Mathf.Abs(knobValue - targetKnobValue);
        if (diff <= solveThreshold)
        {
            isSolved = true;
            StartCoroutine(FinishMinigame());
        }
    }

    // ????????????????????????????????????????
    void UpdateTimerUI()
    {
        float ratio = 1f - (currentTime / maxTime);
        ratio = Mathf.Clamp01(ratio);

        if (timerBar != null)
        {
            timerBar.fillAmount = ratio;
            timerBar.color = Color.Lerp(Color.red, Color.cyan, ratio);
        }

        if (timerText != null)
            timerText.text = $"{Mathf.Max(0, maxTime - currentTime):F1}s";

        // Süre doldu
        if (currentTime >= maxTime && !isSolved)
        {
            isActive = false;
            StartCoroutine(TimeOutFail());
        }
    }

    // ????????????????????????????????????????
    IEnumerator TimeOutFail()
    {
        LuckBarManager.Instance.ModifyLuck(-15f);
        yield return StartCoroutine(ShowFeedback("TIME OUT!", Color.red, 1f));
        // Sýfýrla ve tekrar baþlat
        currentTime = 0f;
        isActive = true;
        isSolved = false;
        tunerKnob.value = Random.Range(0f, 1f); // knob'u karýþtýr
        StartCoroutine(RunMinigame());
    }

    // ????????????????????????????????????????
    IEnumerator FinishMinigame()
    {
        isActive = false;

        if (currentTime <= fastThreshold)
        {
            LuckBarManager.Instance.ModifyLuck(fastBonus);
            yield return StartCoroutine(ShowFeedback("TOO FAST! LUCK DECREASED!", Color.red, 1.5f));
        }
        else
        {
            LuckBarManager.Instance.ModifyLuck(normalBonus);
            yield return StartCoroutine(ShowFeedback("SIGNAL STABILIZED!", Color.green, 1f));
        }

        // Null kontrolü ekle
        if (roomDarkOverlay != null)
            yield return StartCoroutine(FadeOutOverlay());
        else
            Debug.LogWarning("roomDarkOverlay baðlý deðil! Inspector'dan baðla.");

        if (successPanel != null)
            successPanel.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        onComplete?.Invoke(true);
    }
    IEnumerator FadeOutOverlay()
    {
        // SpriteRenderer ile fade
        SpriteRenderer sr = roomDarkOverlay.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            Color c = sr.color;
            while (c.a > 0f)
            {
                c.a -= Time.deltaTime * 0.8f;
                sr.color = c;
                yield return null;
            }
        }

        roomDarkOverlay.SetActive(false);

        // Mask'leri de kaldýr (artýk gerek yok)
        GameObject playerMask = GameObject.Find("PlayerLightMask");
        GameObject pcMask = GameObject.Find("PCLightMask");
        if (playerMask) playerMask.SetActive(false);
        if (pcMask) pcMask.SetActive(false);
    }

    // ????????????????????????????????????????
    IEnumerator ShowFeedback(string msg, Color color, float duration)
    {
        feedbackText.text = msg;
        feedbackText.color = color;
        yield return new WaitForSeconds(duration);
        feedbackText.text = "";
    }
}