using UnityEngine;
using UnityEngine.InputSystem;

public class ExitDoor : MonoBehaviour
{
    public string targetScene = "ElevatorRoom";
    public bool requireMinigameDone = true;
    [HideInInspector] public bool minigameDone = false;

    public GameObject lockedIndicator;
    public GameObject unlockedIndicator;

    bool playerInRange = false;

    void Start() => UpdateVisual();

    public void UnlockDoor()
    {
        minigameDone = true;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (lockedIndicator) lockedIndicator.SetActive(!minigameDone);
        if (unlockedIndicator) unlockedIndicator.SetActive(minigameDone);
    }

    void Update()
    {
        if (!playerInRange || !minigameDone) return;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            SceneTransitionManager.Instance.LoadScene(targetScene);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player")) playerInRange = false;
    }
}