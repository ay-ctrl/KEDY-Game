using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    public string levelMinus1 = "Level0";
    public string level0 = "Level1";
    public string level1 = "Level2";
    public string level2 = "Level3";

    public float luckThreshold = 0.8f;

    public void PressFloor(int floor)
    {
        float luck = LuckBarManager.Instance.currentLuck; // 0-1 arası olduğunu varsayıyorum

        if (luck >= luckThreshold)
        {
            // Şans yüksek → doğru kata git
            LoadCorrectFloor(floor);
        }
        else
        {
            // Şans düşük → rastgele kat ama 2 yok
            LoadRandomFloor();
        }
    }

    void LoadCorrectFloor(int floor)
    {
        string scene = "";

        switch (floor)
        {
            case -1:
                scene = levelMinus1;
                break;

            case 0:
                scene = level0;
                break;

            case 1:
                scene = level1;
                break;

            case 2:
                scene = level2;
                break;
        }

        SceneManager.LoadScene(scene);
    }

    void LoadRandomFloor()
    {
        string[] possibleFloors =
        {
            levelMinus1,
            level0,
            level1
        };

        int randomIndex = Random.Range(0, possibleFloors.Length);
        SceneManager.LoadScene(possibleFloors[randomIndex]);
    }
}