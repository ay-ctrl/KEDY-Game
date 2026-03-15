using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WaveMinigame : MonoBehaviour
{
    [Header("Panel")]
    public GameObject minigamePanel;

    [Header("Wave UI")]
    public RectTransform[] waveLines;   // 4 adet
    public Image[] waveHighlights;      
    public Scrollbar tunerKnob;

    [Header("Timer")]
    public Image timerBar;
    public float maxTime = 20f;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI waveIndexText; 
    public GameObject successPanel;
    public GameObject roomDarkOverlay;

    [Header("Luck")]
    public float fastBonus = -5f;
    public float normalBonus = +15f;
    public float fastThreshold = 6f;

    [Header("Zorluk")]
    public float solveThreshold = 0.008f;  // her dalga için eþik

    // Ýç state
    int currentWaveIndex = 0;  
    float[] waveOffsets;
    float[] waveAmplitudes;
    float[] waveSpeeds;
    float[] waveTargetKnob;       // her dalganýn hedef knob deðeri

    float currentTime = 0f;
    bool isActive = false;
    bool isFinishing = false;
    float knobValue = 0f;

    float[] baseY = { 80f, 27f, -27f, -80f };

    System.Action<bool> onComplete;

    
    public void StartMinigame(System.Action<bool> callback)
    {
        onComplete = callback;
        isActive = true;
        isFinishing = false;
        currentTime = 0f;
        currentWaveIndex = 0;

        minigamePanel.SetActive(true);
        successPanel.SetActive(false);
        feedbackText.text = "";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Dalga parametrelerini oluþtur
        waveOffsets = new float[4];
        waveAmplitudes = new float[4];
        waveSpeeds = new float[4];
        waveTargetKnob = new float[4];

        waveOffsets[0] = Random.Range(-80f, 80f);
        waveAmplitudes[0] = Random.Range(50f, 80f);
        waveSpeeds[0] = Random.Range(2f, 4f);

        waveOffsets[1] = Random.Range(-80f, 80f);
        waveAmplitudes[1] = Random.Range(30f, 60f);
        waveSpeeds[1] = Random.Range(0.5f, 1.5f);

        waveOffsets[2] = Random.Range(-80f, 80f);
        waveAmplitudes[2] = Random.Range(60f, 100f);
        waveSpeeds[2] = Random.Range(3f, 5f);

        waveOffsets[3] = Random.Range(-80f, 80f);
        waveAmplitudes[3] = Random.Range(40f, 70f);
        waveSpeeds[3] = Random.Range(1f, 3f);

        
        for (int i = 0; i < 4; i++)
            waveTargetKnob[i] = Random.Range(0.3f, 0.7f);

       
        SetKnobFarFrom(waveTargetKnob[0]);

        UpdateWaveVisuals();
        UpdateWaveIndexText();

        StartCoroutine(RunMinigame());
    }

   
    IEnumerator RunMinigame()
    {
        while (isActive)
        {
            currentTime += Time.deltaTime;
            UpdateTimerUI();
            UpdateWaves();
            CheckCurrentWaveSolved();
            yield return null;
        }
    }

    
    void Update()
    {
        if (!isActive) return;
        knobValue = tunerKnob.value;
    }

    
    void UpdateWaves()
    {
        if (waveLines == null) return;

        for (int i = 0; i < waveLines.Length; i++)
        {
            if (waveLines[i] == null) continue;

            Vector2 pos = waveLines[i].anchoredPosition;

            if (i < currentWaveIndex)
            {
                
                pos.y = baseY[i];
            }
            else if (i == currentWaveIndex)
            {
                
                float distanceFromTarget = Mathf.Abs(knobValue - waveTargetKnob[i]);
                float flatness = 1f - Mathf.Clamp01(distanceFromTarget / 0.5f);

                float wave1 = Mathf.Sin((Time.time * waveSpeeds[i]) + waveOffsets[i])
                              * waveAmplitudes[i];
                float wave2 = Mathf.Sin((Time.time * waveSpeeds[i] * 2.3f) + waveOffsets[i] * 1.7f)
                              * (waveAmplitudes[i] * 0.4f);

                float totalWave = (wave1 + wave2) * (1f - flatness);
                pos.y = baseY[i] + totalWave;
            }
            else
            {
                
                float wave1 = Mathf.Sin((Time.time * waveSpeeds[i]) + waveOffsets[i])
                              * waveAmplitudes[i];
                float wave2 = Mathf.Sin((Time.time * waveSpeeds[i] * 2.3f) + waveOffsets[i] * 1.7f)
                              * (waveAmplitudes[i] * 0.4f);
                pos.y = baseY[i] + (wave1 + wave2);
            }

            waveLines[i].anchoredPosition = pos;
        }
    }

    
    void CheckCurrentWaveSolved()
    {
        if (isFinishing) return;

        float diff = Mathf.Abs(knobValue - waveTargetKnob[currentWaveIndex]);
        if (diff <= solveThreshold)
        {
            StartCoroutine(WaveSolved());
        }
    }

    
    IEnumerator WaveSolved()
    {
        isFinishing = true;

        // Dalga düz kalsýn
        Vector2 pos = waveLines[currentWaveIndex].anchoredPosition;
        pos.y = baseY[currentWaveIndex];
        waveLines[currentWaveIndex].anchoredPosition = pos;

        yield return StartCoroutine(ShowFeedback(
            $"WAVE {currentWaveIndex + 1} STABILIZED!", Color.green, 0.8f));

        currentWaveIndex++;

        if (currentWaveIndex >= 4)
        {
            // Tüm dalgalar bitti
            StartCoroutine(FinishMinigame());
        }
        else
        {
            
            SetKnobFarFrom(waveTargetKnob[currentWaveIndex]);
            UpdateWaveIndexText();
            isFinishing = false;
        }
    }

    void UpdateWaveVisuals()
    {
        
        if (waveHighlights == null) return;
        for (int i = 0; i < waveHighlights.Length; i++)
        {
            if (waveHighlights[i] == null) continue;
            waveHighlights[i].color = (i == currentWaveIndex)
                ? new Color(1f, 1f, 0f, 0.3f)   
                : new Color(1f, 1f, 1f, 0f);    
        }
    }

    void UpdateWaveIndexText()
    {
        if (waveIndexText != null)
            waveIndexText.text = $"WAVE {currentWaveIndex + 1} / 4";
        UpdateWaveVisuals();
    }

    
    void SetKnobFarFrom(float target)
    {
        // Hedefin karþý tarafýndan baþlat
        float start;
        if (target > 0.5f)
            start = Random.Range(0f, 0.15f);     // hedef saðdaysa soldan baþla
        else
            start = Random.Range(0.85f, 1f);     // hedef soldaysa saðdan baþla

        tunerKnob.value = start;
        knobValue = start;
    }

    
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

        if (currentTime >= maxTime && !isFinishing)
        {
            isActive = false;
            isFinishing = true;
            StartCoroutine(TimeOutFail());
        }
    }

    
    IEnumerator TimeOutFail()
    {
        if (LuckBarManager.Instance != null)
            LuckBarManager.Instance.ModifyLuck(-15f);

        yield return StartCoroutine(ShowFeedback("TIME OUT!", Color.red, 1f));

        // Sýfýrla
        currentTime = 0f;
        currentWaveIndex = 0;
        isFinishing = false;
        isActive = true;

        for (int i = 0; i < 4; i++)
            waveTargetKnob[i] = Random.Range(0.3f, 0.7f);

        SetKnobFarFrom(waveTargetKnob[0]);
        UpdateWaveIndexText();

        StartCoroutine(RunMinigame());
    }

    IEnumerator FinishMinigame()
    {
        isActive = false;

        if (LuckBarManager.Instance != null)
        {
            // Kalan süre = maxTime - currentTime
            float timeLeft = maxTime - currentTime;
            float luckChange;
            string msg;
            Color msgColor;

            if (timeLeft > 10f)
            {
                // 10sn+ kala bitirdi — çok erken, ceza
                luckChange = -20f;
                msg = "TOO EARLY! -20 LUCK!";
                msgColor = Color.red;
            }
            else if (timeLeft > 7f)
            {
                // 7-10sn kala — erken, küçük ceza
                luckChange = -10f;
                msg = "A BIT EARLY! -10 LUCK!";
                msgColor = new Color(1f, 0.5f, 0f);
            }
            else if (timeLeft > 4f)
            {
                // 4-7sn kala — iyi
                luckChange = +10f;
                msg = "GOOD TIMING! +10 LUCK!";
                msgColor = Color.yellow;
            }
            else if (timeLeft > 1f)
            {
                // 1-4sn kala — mükemmel
                luckChange = +20f;
                msg = "PERFECT TIMING! +20 LUCK!";
                msgColor = Color.green;
            }
            else
            {
                // Son 1sn — tam gaz
                luckChange = +30f;
                msg = "LAST SECOND! +30 LUCK!";
                msgColor = Color.cyan;
            }

            yield return StartCoroutine(ShowFeedback(msg, msgColor, 1.5f));
            LuckBarManager.Instance.ModifyLuck(luckChange);
        }

        if (roomDarkOverlay != null)
            yield return StartCoroutine(FadeOutOverlay());

        if (successPanel != null)
            successPanel.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        onComplete?.Invoke(true);
    }

    IEnumerator FadeOutOverlay()
    {
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

        GameObject playerMask = GameObject.Find("PlayerLightMask");
        GameObject pcMask = GameObject.Find("PCLightMask");
        if (playerMask) playerMask.SetActive(false);
        if (pcMask) pcMask.SetActive(false);
    }

   
    IEnumerator ShowFeedback(string msg, Color color, float duration)
    {
        feedbackText.text = msg;
        feedbackText.color = color;
        yield return new WaitForSeconds(duration);
        feedbackText.text = "";
    }
}