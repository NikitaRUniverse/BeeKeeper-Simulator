using UnityEngine;

public class FlowerCell : MonoBehaviour
{
    private FlowerAutomataController controller;
    private int x,y; // Automaton coordinates
    private Renderer flowerRenderer; // Renderer for color updates
    private Color innateColor; // Fully saturated flower color for material
    private Color deadColor = Color.grey; // Dead flower color for material

    public void Init(FlowerAutomataController c, int x, int y, Color color) {
        flowerRenderer = GetComponent<Renderer>();

        controller = c;
        this.x = x;
        this.y = y;
        innateColor = color;
        flowerRenderer.material.color = innateColor;
    }

    // Invoked after every alive cell update in automata
    public void UpdateAlive(int strength) {
        float t = 0.75f - strength/12f;
        flowerRenderer.material.color = Color.Lerp(innateColor, deadColor, t);
    }

    // Invoked after every dead cell update in automata
    public void UpdateDead() {
        flowerRenderer.material.color = deadColor;
    }

    // Invoked on bees entering the flower
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Bee")) {
            controller.PollinateFlower(x,y);
        }
    }
}
