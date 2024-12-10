using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFlowerCounter : MonoBehaviour
{
    // Displaying the amount of flowers

    public void UpdateFlowerCount(int flowerCount, int initialFlowers)
    {
        GetComponent<TextMeshProUGUI>().text = "Total: " + flowerCount + "/" + initialFlowers;
    }
}
