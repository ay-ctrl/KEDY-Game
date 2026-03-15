using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    public string floorMinus1 = "Level0";
    public string floor0 = "Level1";
    public string floor1 = "Level2";
    public string floor2 = "Level3";

    public float luckThreshold = 80;

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
                scene = floorMinus1;
                break;

            case 0:
                scene = floor0;
                break;

            case 1:
                scene = floor1;
                break;

            case 2:
                scene = floor2;
                break;
        }

        SceneManager.LoadScene(scene);
    }

    void LoadRandomFloor()
    {
        string[] possibleFloors =
        {
            floor0,
            floor1,
            floor2
        };

        int randomIndex = Random.Range(0, possibleFloors.Length);
        SceneManager.LoadScene(possibleFloors[randomIndex]);
    }
}