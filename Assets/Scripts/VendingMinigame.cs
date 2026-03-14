using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class VendingMinigame : MonoBehaviour
{
    [Header("Panel")]
    public GameObject minigamePanel;

    [Header("Butonlar")]
    public Button[] keyButtons;
    public Image[] keyButtonImages;

    [Header("Renkler")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color litColor = Color.yellow;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public Color promptColor = new Color(1f, 0.5f, 0f, 1f);

    [Header("Feedback UI")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI feedbackText;
    public GameObject successPanel;

    [Header("Gecikme")]
    public float mouseClickDelay = 0.4f;

    // Ýç state
    int[] sequence;
    int[] expectedOrder;
    int currentStep = 0;
    int currentRound = 0;
    bool isActive = false;
    bool waitingForInput = false;
    bool clickBlocked = false;

    System.Action<bool> onComplete;

    public void StartMinigame(System.Action<bool> callback)
    {
        onComplete = callback;
        isActive = true;
        currentRound = 0;
        currentStep = 0;

        minigamePanel.SetActive(true);
        successPanel.SetActive(false);
        feedbackText.text = "";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ResetAllColors();
        StartCoroutine(StartRound());
    }

   
    IEnumerator StartRound()
    {
        waitingForInput = false;
        clickBlocked = false;
        currentStep = 0;

        // Round ve talimat yazýsý
        bool isReverse = (currentRound % 2 == 0); 
        roundText.text = $"ROUND {currentRound + 1} / 4";
        instructionText.text = isReverse ? "TERSÝNDEN TUÞLA!" : "SIRAYA GÖRE TUÞLA!";
        feedbackText.text = "";

        ResetAllColors();
        yield return new WaitForSeconds(0.8f);

        // Sýra üret
        sequence = GenerateSequence(4);
        expectedOrder = BuildExpectedOrder(sequence, isReverse);

     
        Debug.Log($"Round {currentRound + 1} | Ters:{isReverse}");
        Debug.Log("Gösterilen sýra : " + string.Join(",", sequence));
        Debug.Log("Beklenen sýra   : " + string.Join(",", expectedOrder));

        // Sýrayý göster
        yield return StartCoroutine(ShowSequence(sequence));

        // Giriþi bekle
        waitingForInput = true;
        ShowPrompt(); 
    }

    
    IEnumerator ShowSequence(int[] seq)
    {
        ResetAllColors();
        yield return new WaitForSeconds(0.3f);

        foreach (int idx in seq)
        {
            keyButtonImages[idx].color = litColor;
            yield return new WaitForSeconds(0.5f);
            keyButtonImages[idx].color = normalColor;
            yield return new WaitForSeconds(0.25f);
        }

       
        yield return new WaitForSeconds(0.5f);
        ResetAllColors();
    }

   
    public void OnKeyPressed(int buttonIndex)
    {
        if (!isActive || !waitingForInput || clickBlocked) return;
        StartCoroutine(HandlePress(buttonIndex));
    }

    IEnumerator HandlePress(int buttonIndex)
    {
        clickBlocked = true;

        int expected = expectedOrder[currentStep];

        if (buttonIndex == expected)
        {
            
            keyButtonImages[buttonIndex].color = correctColor;
            yield return new WaitForSeconds(0.2f);
            keyButtonImages[buttonIndex].color = normalColor;

            currentStep++;

            if (currentStep >= expectedOrder.Length)
            {
                // Round tamamlandý
                ResetAllColors();
                currentRound++;

                if (currentRound >= 4)
                {
                    yield return StartCoroutine(FinishMinigame());
                }
                else
                {
                    yield return new WaitForSeconds(0.5f);
                    StartCoroutine(StartRound());
                }
            }
            else
            {
                // Sonraki adýmý vurgula
                ShowPrompt();
            }
        }
        else
        {
            
            keyButtonImages[buttonIndex].color = wrongColor;
            LuckBarManager.Instance.ModifyLuck(-8f);
            yield return new WaitForSeconds(0.2f);
            keyButtonImages[buttonIndex].color = normalColor;

            yield return StartCoroutine(ShowFeedback("YANLIÞ!", Color.red, 0.7f));

            // Ayný round'u sýfýrdan baþlat
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(StartRound());
            yield break; 
        }

        yield return new WaitForSeconds(mouseClickDelay);
        clickBlocked = false;
    }

    
    IEnumerator FinishMinigame()
    {
        waitingForInput = false;
        LuckBarManager.Instance.ModifyLuck(+20f);
        yield return StartCoroutine(ShowFeedback("TAMAMLANDI!", Color.green, 1f));
        successPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        minigamePanel.SetActive(false);
        isActive = false;
        onComplete?.Invoke(true);
    }


    void ShowPrompt()
    {
        ResetAllColors();
    }

    int[] BuildExpectedOrder(int[] seq, bool reverse)
    {
        int len = seq.Length;
        int[] result = new int[len];

        if (!reverse)
        {
            
            for (int i = 0; i < len; i++)
                result[i] = seq[i];
        }
        else
        {
            
            for (int i = 0; i < len; i++)
                result[i] = seq[len - 1 - i];
        }
        return result;
    }

    int[] GenerateSequence(int length)
    {
        List<int> pool = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        int[] result = new int[length];
        for (int i = 0; i < length; i++)
        {
            int pick = Random.Range(0, pool.Count);
            result[i] = pool[pick];
            pool.RemoveAt(pick);
        }
        return result;
    }

    IEnumerator ShowFeedback(string msg, Color color, float duration)
    {
        feedbackText.text = msg;
        feedbackText.color = color;
        yield return new WaitForSeconds(duration);
        feedbackText.text = "";
    }

    void ResetAllColors()
    {
        foreach (var img in keyButtonImages)
            if (img) img.color = normalColor;
    }
}