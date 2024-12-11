using UnityEngine;

public class FlowerCell : MonoBehaviour
{
    private FlowerAutomataController controller;
    private int x,y;
    private Renderer flowerRenderer; // Renderer for color updates
    private Color innateColor;
    private Color deadColor = Color.grey;

    public void Init(FlowerAutomataController c, int x, int y, Color color) {
        flowerRenderer = GetComponent<Renderer>();

        controller = c;
        this.x = x;
        this.y = y;
        innateColor = color;
        flowerRenderer.material.color = innateColor;
    }

    public void UpdateAlive(int strength) {
        float t = 0.75f - strength/12f;
        flowerRenderer.material.color = Color.Lerp(innateColor, deadColor, t);
    }

    public void UpdateDead() {
        flowerRenderer.material.color = deadColor;
    }

    // Check for bees entering the flower's area
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Bee")) {
            controller.PollinateFlower(x,y);
        }
    }
}
