using UnityEngine;
using UnityEngine.SceneManagement;
public class ExitRoom : MonoBehaviour
{
    public void TakeElevator()
    {
        SceneManager.LoadScene("Elevator");
    }
}