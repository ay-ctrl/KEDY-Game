using UnityEngine;
using UnityEngine.InputSystem;

public class ElectricBoxInteractable : MonoBehaviour
{
    public GameObject interactPrompt;
    public WireMinigame wireMinigame;
    public PlayerController player;
    public ExitDoor exitDoor;

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
        wireMinigame.StartMinigame(OnMinigameComplete);
    }

    void OnMinigameComplete(bool success)
    {
        minigameDone = true;
        player.canMove = true;
        if (success) LuckBarManager.Instance.ModifyLuck(+15f);
        if (exitDoor != null) exitDoor.UnlockDoor();
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