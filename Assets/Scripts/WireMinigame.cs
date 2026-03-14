using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;

public class WireMinigame : MonoBehaviour
{
    [Header("Panel")]
    public GameObject minigamePanel;

    [Header("Wire Setup")]
    public int wireCount = 4;
    public RectTransform[] leftPoints;
    public RectTransform[] rightPoints;

    [Header("Colors")]
    public Color[] wireColors;

    [Header("Feedback")]
    public GameObject wrongFeedback;
    public GameObject successPanel;

    // Ýç state
    int[] correctMapping;
    int[] currentMapping;
    int selectedLeft = -1;
    LineRenderer activeLine;
    List<LineRenderer> completedLines = new List<LineRenderer>();
    Action<bool> onComplete;

    bool isActive = false;
    Vector2 virtualMousePos;

    
    public void StartMinigame(Action<bool> callback)
    {
        onComplete = callback;
        isActive = true;
        minigamePanel.SetActive(true);
        successPanel.SetActive(false);
        if (wrongFeedback) wrongFeedback.SetActive(false);

        // Tüm eski line'larý temizle
        foreach (var l in completedLines)
            if (l != null) Destroy(l.gameObject);
        completedLines.Clear();

        correctMapping = GenerateRandomMapping(wireCount);
        currentMapping = new int[wireCount];
        for (int i = 0; i < wireCount; i++) currentMapping[i] = -1;

        virtualMousePos = new Vector2(Screen.width / 2f, Screen.height / 2f);


        // YERÝNE bunlarý yaz:
        Cursor.lockState = CursorLockMode.Confined;  // ekran dýþýna çýkmasýn
        Cursor.visible = true;                        // görünür kalsýn

        SetupWireUI();
    }

   
    int[] GenerateRandomMapping(int count)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < count; i++) available.Add(i);

        
        for (int i = count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

       
        for (int i = 0; i < count; i++)
            if (available[i] == i)
            {
                int swap = (i + 1) % count;
                (available[i], available[swap]) = (available[swap], available[i]);
            }

        return available.ToArray();
    }

    void SetupWireUI()
    {
        // Sol noktalarý renklendir
        for (int i = 0; i < wireCount; i++)
        {
            var img = leftPoints[i].GetComponent<Image>();
            if (img) img.color = wireColors[i];
        }

        // Sað noktalarý correctMapping'e göre renklendir
        for (int i = 0; i < wireCount; i++)
        {
            var img = rightPoints[correctMapping[i]].GetComponent<Image>();
            if (img) img.color = wireColors[i];
        }
    }

 
    void LateUpdate()
    {
        if (!isActive || selectedLeft == -1 || activeLine == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        // Delta'yý TERS uygula
        float dx = mouse.delta.x.ReadValue();
        float dy = mouse.delta.y.ReadValue();
        float sens = 8f;

        virtualMousePos.x -= dx * sens;   // saða gidince sola
        virtualMousePos.y -= dy * sens;   // yukarý gidince aþaðý
        virtualMousePos.x = Mathf.Clamp(virtualMousePos.x, 0, Screen.width);
        virtualMousePos.y = Mathf.Clamp(virtualMousePos.y, 0, Screen.height);

        
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(virtualMousePos.x, virtualMousePos.y, 10f));
        worldPos.z = 0f;
        activeLine.SetPosition(1, worldPos);
    }

    
    public void OnLeftPointClicked(int index)
    {
        if (!isActive) return;
        if (currentMapping[index] != -1) return; // zaten baðlý

        // Önceki seçimi iptal et
        if (activeLine != null)
        {
            Destroy(activeLine.gameObject);
            activeLine = null;
        }

        selectedLeft = index;
        activeLine = CreateLine(wireColors[index]);

        Vector3 start = leftPoints[index].position;
        start.z = 0f;
        activeLine.SetPosition(0, start);
        activeLine.SetPosition(1, start);

        virtualMousePos = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    public void OnRightPointClicked(int rightIndex)
    {
        if (!isActive || selectedLeft == -1) return;

        bool correct = (correctMapping[selectedLeft] == rightIndex);

        if (correct)
        {
            currentMapping[selectedLeft] = rightIndex;

            // Line'ý sað noktaya sabitle
            Vector3 end = rightPoints[rightIndex].position;
            end.z = 0f;
            activeLine.SetPosition(1, end);
            completedLines.Add(activeLine);
            activeLine = null;
            selectedLeft = -1;

            CheckAllConnected();
        }
        else
        {
            LuckBarManager.Instance.ModifyLuck(-5f);
            StartCoroutine(ShowWrongFeedback());
            Destroy(activeLine.gameObject);
            activeLine = null;
            selectedLeft = -1;
        }
    }

   
    IEnumerator ShowWrongFeedback()
    {
        if (wrongFeedback)
        {
            wrongFeedback.SetActive(true);
            yield return new WaitForSeconds(0.6f);
            wrongFeedback.SetActive(false);
        }
    }

    void CheckAllConnected()
    {
        for (int i = 0; i < wireCount; i++)
            if (currentMapping[i] == -1) return;

        StartCoroutine(CompleteMinigame());
    }

    IEnumerator CompleteMinigame()
    {
        successPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isActive = false;
        yield return new WaitForSeconds(1.5f);
        minigamePanel.SetActive(false);
        onComplete?.Invoke(true);
    }

    
    LineRenderer CreateLine(Color color)
    {
        GameObject go = new GameObject("WireLine");
        go.transform.SetParent(minigamePanel.transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;
        lr.sortingOrder = 10;
        lr.useWorldSpace = true;
        return lr;
    }
}