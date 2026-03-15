using UnityEngine;

public class RoomLightController : MonoBehaviour
{
    // Sadece PC monitörü aydýnlýk baþlangýçta
    // Diðer tüm objeler DarkOverlay altýnda gizli

    public GameObject[] hiddenObjects; // overlay kalktýðýnda görünecekler
    // Bunlarý overlay kapandýktan sonra aktif etmek istersen kullanabilirsin
    // Þu an overlay yeterli
}