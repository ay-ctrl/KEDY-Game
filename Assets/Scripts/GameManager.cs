using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // sahneler arasýnda kalýr
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // GameManager içindeki diðer metodlar...
}