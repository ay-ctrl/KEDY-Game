using UnityEngine;
using UnityEngine.InputSystem;

public class ExitDoor : MonoBehaviour
{
    public string targetScene = "Elevator";
    public bool minigameDone = false;

    // Görev tamamlanmadýysa uyarý göstermek için
    public GameObject taskNotDonePanel; // UI panel veya mesaj objesi

    bool playerInRange = false;

    public void UnlockDoor()
    {
        minigameDone = true;
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!minigameDone)
            {
                // Görev tamamlanmadý uyarýsý
                if (taskNotDonePanel) StartCoroutine(ShowTaskNotDone());
                else Debug.Log("Görevi tamamlamadan asansörü çalýþtýramazsýn!");
                return;
            }

            // Görev tamamlandýysa sahneyi deðiþtir
            SceneTransitionManager.Instance.LoadScene(targetScene);
        }
    }

    System.Collections.IEnumerator ShowTaskNotDone()
    {
        taskNotDonePanel.SetActive(true);
        yield return new WaitForSeconds(2f); // 2 saniye göster
        taskNotDonePanel.SetActive(false);
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