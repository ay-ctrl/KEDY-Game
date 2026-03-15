using UnityEngine;

public class PhoneInteraction : MonoBehaviour
{
    public SimpleDialogue dialogue;

    void OnMouseDown()
    {
        Debug.Log("a");
        dialogue.StartDialogue();
    }
}