using UnityEngine;
using TMPro;

public class SimpleDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    string[] lines =
    {
        "Anne: Luz nerde kaldýn?",
        "Luz: Çok yorgunum...",
        "Luz: -1'deki odama gitmem lazým."
    };

    int index = 0;

    public void StartDialogue()
    {
        dialoguePanel.SetActive(true);
        index = 0;
        dialogueText.text = lines[index];
    }

    void Update()
    {
        if (!dialoguePanel.activeInHierarchy) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            index++;

            if (index < lines.Length)
            {
                dialogueText.text = lines[index];
            }
            else
            {
                dialoguePanel.SetActive(false);
            }
        }
    }
}