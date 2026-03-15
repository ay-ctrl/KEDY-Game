using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    public string floorMinus1 = "Level0"; // -1 kat
    public string floor0 = "Level1";      // 0 kat
    public string floor1 = "Level2";      // 1 kat
    public string floor2 = "Level3";      // 2 kat

    private float luckThreshold = 80;

    public void PressFloor(int floor)
    {
        float luck = LuckBarManager.Instance.currentLuck; // 0-100 arası varsayalım
        Debug.Log(luck);

        if (luck >= luckThreshold)
        {
            Debug.Log(luck);
            // Şans yüksek → hangi kata basarsan o kat
            LoadCorrectFloor(floor);
            Debug.Log(luck);
        }
        else
        {
            // Şans düşük → rastgele kat ama -1'e asla gitme
            LoadRandomFloorExcludingMinus1();
        }
    }

    void LoadCorrectFloor(int floor)
    {
        string scene = "";
        Debug.Log("correct");

        switch (floor)
        {
            case -1: scene = floorMinus1; break;
            case 0: scene = floor0; break;
            case 1: scene = floor1; break;
            case 2: scene = floor2; break;
        }

        SceneManager.LoadScene(scene);
    }

    void LoadRandomFloorExcludingMinus1()
    {
        Debug.Log("aaaa");
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