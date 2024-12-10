using System.Collections;
using UnityEngine;

public class FlowerAutomataController : MonoBehaviour
{
    public GameObject flowerPrefab;
    public int flowerStartSeeds;
    public int flowerStartSpreadIters;
    public float flowerStartSpreadChance;
    public float flowerCellScale;

    public Vector2Int chunksSize;
    public int chunkToAutomataLength;
    public int deadSteps;
    public int autPollenStart;
    public int autNonPollenStart;
    public float aliveReductionChance;
    public float deadReductionChance;
    public float pollinationChance;
    public float seedingChance;

    public bool active;
    public float stepPeriodSeconds;

    private FlowerAutomata automata;

    
    private Color color1 = new Color(1f, 0.95f, 0.8f);
    private Color color2 = new Color(0.7f, 0.7f, 0.75f);
    private Color color3 = new Color(0.5f, 0.5f, 0.55f);
    private Color color4 = new Color(0.7f, 0.3f, 0.3f);
    private Color color5 = new Color(0.2f, 0.6f, 0.4f);


    public void Init() {
        automata = new FlowerAutomata(
            chunksSize,
            chunkToAutomataLength,
            deadSteps,
            autPollenStart,
            autNonPollenStart,
            aliveReductionChance,
            deadReductionChance,
            pollinationChance,
            seedingChance
        );
        for (int i = 0; i < flowerStartSeeds; i++) {
            SpawnFlowerRandomly(); 
        }
        for (int i = 0; i < flowerStartSpreadIters; i++) {
            SpreadFlowersRandomly();
        }
        // StartCoroutine(AutomataLoop());
    }

    void SpawnFlowerRandomly() {
        int x,y;
        while (true) {
            x = Random.Range(0, automata.automata.GetLength(0));
            y = Random.Range(0, automata.automata.GetLength(1));
            if (automata.automata[x,y] == 0) break;
        }

        SpawnFlower(x,y,9);
    }

    void SpawnFlower(int x, int y, int automatonValue) {
        automata.automata[x,y] = automatonValue;

        // TODO random spawn offsets
        Vector3 pos = new Vector3(x, 0, y) * flowerCellScale;

        GameObject newFlower = Instantiate(flowerPrefab, pos, Quaternion.identity);
        Color randomColor = NewFlowerColor();
        Renderer flowerRenderer = newFlower.GetComponent<Renderer>();
        if (flowerRenderer != null)
        {
            flowerRenderer.material.color = randomColor;
        }    
    }

    void SpreadFlowersRandomly() {
        for (int x = 1; x < automata.automata.GetLength(0)-1; x++) for (int y = 1; y < automata.automata.GetLength(1)-1; y++) {
            if (!automata.IsEmpty(x,y)) continue;
            for (int dx = x-1; dx < x+2; dx++) for (int dy = y-1; dy < y+2; dy++) {
                if (automata.IsAlive(x,y)) {
                    bool spawn = Random.value < flowerStartSpreadChance;
                    if (spawn) SpawnFlower(x,y,9);
                }
            }
        }
    }

    Color NewFlowerColor() {
        int randomIndex = Random.Range(0, 5); 
        switch (randomIndex) {
            case 0:
                return color1;
            case 1:
                return color2;
            case 2:
                return color3;
            case 3:
                return color4;
            case 4:
                return color5;
            default:
                return color1;
        }
    }

    IEnumerator AutomataLoop() {
        while (active) {
            automata.Step();
            // TODO update flowers
            yield return new WaitForSeconds(stepPeriodSeconds);
        }
    }

    void PollinateFlower(int x, int y) {
        automata.automata[x,y] = 9;
        // TODO do something cool to the flower
    }
}
