using UnityEngine;
using UnityEngine.InputSystem;

public class DrinkItem : MonoBehaviour
{
    public float luckBonus = 10f;
    public GameObject pickupPrompt;

    bool playerInRange = false;

    void Update()
    {
        if (!playerInRange) return;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            PickUp();
    }

    void PickUp()
    {
        LuckBarManager.Instance.ModifyLuck(luckBonus);
        if (pickupPrompt) pickupPrompt.SetActive(false);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
            if (pickupPrompt) pickupPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = false;
            if (pickupPrompt) pickupPrompt.SetActive(false);
        }
    }
}