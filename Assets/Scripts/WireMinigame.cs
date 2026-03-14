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

    [Header("Canvas")]
    public Canvas minigameCanvas;  // MinigameCanvas buraya baðla

    int[] correctMapping;
    int[] currentMapping;
    int selectedLeft = -1;
    LineRenderer activeLine;
    List<LineRenderer> completedLines = new List<LineRenderer>();
    Action<bool> onComplete;
    public bool isActive = false;
    Vector2 virtualMousePos;

    // Buton týklama yerine mouse hover ile seçim için
    float clickRadius = 30f; // pixel cinsinden yakýnlýk eþiði

    public void StartMinigame(Action<bool> callback)
    {
        onComplete = callback;
        isActive = true;
        minigamePanel.SetActive(true);
        successPanel.SetActive(false);
        if (wrongFeedback) wrongFeedback.SetActive(false);

        foreach (var l in completedLines)
            if (l != null) Destroy(l.gameObject);
        completedLines.Clear();

        correctMapping = GenerateRandomMapping(wireCount);
        currentMapping = new int[wireCount];
        for (int i = 0; i < wireCount; i++) currentMapping[i] = -1;

        virtualMousePos = new Vector2(Screen.width / 2f, Screen.height / 2f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetupWireUI();
    }

    int[] GenerateRandomMapping(int count)
    {
        int[] map = new int[count];
        for (int i = 0; i < count; i++)
            map[i] = i;
        return map;
    }

   
    void SetupWireUI()
    {
        for (int i = 0; i < wireCount; i++)
        {
            // Sol — gerçek renk
            var imgL = leftPoints[i].GetComponent<Image>();
            if (imgL) { Color c = wireColors[i]; c.a = 1f; imgL.color = c; }

            // Sað — correctMapping'e göre renk
            var imgR = rightPoints[correctMapping[i]].GetComponent<Image>();
            if (imgR) { Color c = wireColors[i]; c.a = 1f; imgR.color = c; }

            // Sol buton üstüne "gerçek index" yaz (hangi kablo olduðu)
            var leftTmp = leftPoints[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (leftTmp) leftTmp.text = (i + 1).ToString();

            // Sað buton üstüne de numara yaz
            var rightTmp = rightPoints[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (rightTmp) rightTmp.text = (i + 1).ToString();
        }
    }

    void Update()
    {
        if (!isActive) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mouseScreenPos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            // SOL NOKTALARA TIKLA
            // i'ye týklamak için (i+1)%count butonuna basýlmasý lazým
            for (int i = 0; i < wireCount; i++)
            {
                if (currentMapping[i] != -1) continue;

                // i'yi seçmek için gerçekte hangi butona basýlmasý gerekiyor?
                int requiredButton = (i + 1) % wireCount;
                Vector2 screenPoint = RectToScreen(leftPoints[requiredButton]);

                if (Vector2.Distance(mouseScreenPos, screenPoint) < clickRadius)
                {
                    SelectLeft(i); // aslýnda i'yi seçiyoruz
                    return;
                }
            }

            // SAÐ NOKTALARA TIKLA (sol seçiliyse)
            if (selectedLeft != -1)
            {
                for (int i = 0; i < wireCount; i++)
                {
                    // i'ye baðlamak için (i+1)%count butonuna basýlmasý lazým
                    int requiredButton = (i + 1) % wireCount;
                    Vector2 screenPoint = RectToScreen(rightPoints[requiredButton]);

                    if (Vector2.Distance(mouseScreenPos, screenPoint) < clickRadius)
                    {
                        TryConnect(i); // aslýnda i'ye baðlýyoruz
                        return;
                    }
                }
            }
        }
    }

    void LateUpdate()
    {
        if (!isActive || selectedLeft == -1 || activeLine == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        // Ters mouse: delta'yý ters uygula
        Vector2 currentMousePos = mouse.position.ReadValue();
        Vector2 delta = currentMousePos - virtualMousePos;
        virtualMousePos = virtualMousePos - delta * 0.5f;
        virtualMousePos.x = Mathf.Clamp(virtualMousePos.x, 0, Screen.width);
        virtualMousePos.y = Mathf.Clamp(virtualMousePos.y, 0, Screen.height);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(virtualMousePos.x, virtualMousePos.y, 10f));
        worldPos.z = 0f;
        activeLine.SetPosition(1, worldPos);
    }

    // RectTransform'un ekran pozisyonunu al
    Vector2 RectToScreen(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        // Köþelerin ortasý
        Vector3 center = (corners[0] + corners[2]) / 2f;

        if (minigameCanvas != null && minigameCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            return Camera.main.WorldToScreenPoint(center);

        return new Vector2(center.x, center.y);
    }

    void SelectLeft(int index)
    {
        if (activeLine != null)
        {
            Destroy(activeLine.gameObject);
            activeLine = null;
        }

        selectedLeft = index;
        activeLine = CreateLine(wireColors[index]);

        Vector3 start = Camera.main.ScreenToWorldPoint(
            new Vector3(RectToScreen(leftPoints[index]).x,
                        RectToScreen(leftPoints[index]).y, 10f));
        start.z = 0f;
        activeLine.SetPosition(0, start);
        activeLine.SetPosition(1, start);

        virtualMousePos = Mouse.current.position.ReadValue();
    }

    void TryConnect(int rightIndex)
    {
        bool correct = (correctMapping[selectedLeft] == rightIndex);

        if (correct)
        {
            currentMapping[selectedLeft] = rightIndex;

            Vector3 end = Camera.main.ScreenToWorldPoint(
                new Vector3(RectToScreen(rightPoints[rightIndex]).x,
                            RectToScreen(rightPoints[rightIndex]).y, 10f));
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