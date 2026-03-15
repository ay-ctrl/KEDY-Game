using UnityEngine;
using UnityEngine.SceneManagement;
public class NextScene : MonoBehaviour
{

    public Animator elevatorAnimator;
    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void StartButton()
    {
        elevatorAnimator.SetTrigger("openelevator");
        Invoke("LoadNextScene", 2f); // 2 saniye sonra çalýþýr
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // Editörde çalýþýyorsa durdurur
        #else
                Application.Quit(); // Build edilmiþ oyunda çýkýþ yapar
        #endif
    }
}