using UnityEngine;
using UnityEngine.SceneManagement;
public class NextScene : MonoBehaviour
{

    public Animator elevatorAnimator;
    public GameObject playButton;
    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void StartButton()
    {
        elevatorAnimator.SetTrigger("openelevator");
        playButton.SetActive(false);
        Invoke("LoadNextScene", 2f); // 2 saniye sonra çalýþýr
    }
}