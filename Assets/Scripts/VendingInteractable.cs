using UnityEngine;
using UnityEngine.InputSystem;

public class VendingInteractable : MonoBehaviour
{
    public GameObject interactPrompt;
    public VendingMinigame vendingMinigame;
    public PlayerController player;
    public ExitDoor exitDoor;

    [Header("Ýçecek")]
    public GameObject drinkPrefab;
    public Transform drinkSpawnPoint; // otomatýn önü

    bool playerInRange = false;
    bool minigameDone = false;

    void Update()
    {
        if (!playerInRange || minigameDone) return;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            OpenMinigame();
    }

    void OpenMinigame()
    {
        if (interactPrompt) interactPrompt.SetActive(false);
        player.canMove = false;
        vendingMinigame.StartMinigame(OnMinigameComplete);
    }

    void OnMinigameComplete(bool success)
    {
        minigameDone = true;
        player.canMove = true;
        if (exitDoor != null) exitDoor.UnlockDoor();

        // Ýçeceði spawn et
        if (drinkPrefab != null && drinkSpawnPoint != null)
            Instantiate(drinkPrefab, drinkSpawnPoint.position, Quaternion.identity);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPrompt) interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPrompt) interactPrompt.SetActive(false);
        }
    }
}