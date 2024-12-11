using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFlowerCounter : MonoBehaviour
{
    // Displaying the amount of flowers

    public void UpdateFlowerCount(int flowerCount)
    {
        GetComponent<TextMeshProUGUI>().text = "Flowers: " + flowerCount;
    }
}
