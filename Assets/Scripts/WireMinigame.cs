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
    public Canvas minigameCanvas;

    public ExitDoor exitDoor; // sahnede ExitDoor'u inspector'dan baðla

    int[] correctMapping;
    int[] currentMapping;
    int selectedLeft = -1;
    LineRenderer activeLine;
    List<LineRenderer> completedLines = new List<LineRenderer>();
    Action<bool> onComplete;
    public bool isActive = false;

    float clickRadius = 30f;
    int currentStep = 0;
    int[] solutionOrder = { 3,0,2,1 };

    public void StartMinigame(Action<bool> callback)
    {
        onComplete = callback;
        isActive = true;
        minigamePanel.SetActive(true);
        successPanel.SetActive(false);
        if (wrongFeedback) wrongFeedback.SetActive(false);
        currentStep = 0;

        foreach (var l in completedLines)
            if (l != null) Destroy(l.gameObject);
        completedLines.Clear();

        correctMapping = GenerateRandomMapping(wireCount);
        currentMapping = new int[wireCount];
        for (int i = 0; i < wireCount; i++) currentMapping[i] = -1;

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
            var imgL = leftPoints[i].GetComponent<Image>();
            if (imgL)
            {
                Color c = wireColors[i];
                imgL.color = c;
            }

            var imgR = rightPoints[i].GetComponent<Image>();
            if (imgR)
            {
                Color c = wireColors[i];
                c.a = 1f;
                imgR.color = c;
            }

            var leftTmp = leftPoints[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (leftTmp) leftTmp.text = (i + 1).ToString();

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
            for (int i = 0; i < wireCount; i++)
            {
                if (currentMapping[i] != -1) continue; // zaten baðlanmýþsa atla

                int requiredButton = i % wireCount;
                Vector2 screenPoint = RectToScreen(leftPoints[requiredButton]);

                if (Vector2.Distance(mouseScreenPos, screenPoint) < clickRadius)
                {
                    SelectLeft(i);
                    return;
                }
            }

            // SAÐ NOKTALARA TIKLA (sol seçiliyse)
            if (selectedLeft != -1)
            {
                for (int i = 0; i < wireCount; i++)
                {
                    // zaten baðlanmýþsa atla
                    bool isTaken = false;
                    for (int j = 0; j < wireCount; j++)
                        if (currentMapping[j] == i) { isTaken = true; break; }
                    if (isTaken) continue;

                    int requiredButton = i % wireCount;
                    Vector2 screenPoint = RectToScreen(rightPoints[requiredButton]);

                    if (Vector2.Distance(mouseScreenPos, screenPoint) < clickRadius)
                    {
                        TryConnect(i);
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

        Vector2 mousePos = mouse.position.ReadValue();

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, 10f));
        worldPos.z = 0f;

        activeLine.SetPosition(1, worldPos);
    }

    Vector2 RectToScreen(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

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

        int requiredButton = index % wireCount;

        Vector3 start = Camera.main.ScreenToWorldPoint(
            new Vector3(RectToScreen(leftPoints[requiredButton]).x,
                        RectToScreen(leftPoints[requiredButton]).y, 10f));
        start.z = 0f;

        activeLine.SetPosition(0, start);
        activeLine.SetPosition(1, start);
    }

    void TryConnect(int rightIndex)
    {
        int expectedLeft = solutionOrder[currentStep];

        // yanlýþ sýradaki kablo seçildi
        if (selectedLeft != expectedLeft)
        {
            LuckBarManager.Instance.ModifyLuck(-5f);
            StartCoroutine(ShowWrongFeedback());

            Destroy(activeLine.gameObject);
            activeLine = null;
            selectedLeft = -1;
            return;
        }

        bool correct = (correctMapping[selectedLeft] == rightIndex);

        if (correct)
        {
            currentMapping[selectedLeft] = rightIndex;

            int rightButton = rightIndex % wireCount;

            Vector3 end = Camera.main.ScreenToWorldPoint(
                new Vector3(RectToScreen(rightPoints[rightButton]).x,
                            RectToScreen(rightPoints[rightButton]).y, 10f));
            end.z = 0f;

            activeLine.SetPosition(1, end);

            Image rightImg = rightPoints[rightButton].GetComponent<Image>();

            if (rightImg)
            {
                Color c = rightImg.color;
                c.a = 0.2f;
                rightImg.color = c;
            }

            completedLines.Add(activeLine);
            activeLine = null;
            selectedLeft = -1;

            // sýradaki adýma geç
            currentStep++;

            // hepsi çözüldü
            if (currentStep >= solutionOrder.Length)
            {
                LuckBarManager.Instance.ModifyLuck(+10f);
                exitDoor.UnlockDoor();
                StartCoroutine(CompleteMinigame());
            }
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