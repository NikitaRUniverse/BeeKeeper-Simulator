using System.Collections;
using UnityEngine;

public class FlowerAutomataController : MonoBehaviour
{
    /*
    Spawns and updates flowers based on FlowerAutomata rules
    */

    public GameObject flowerPrefab;
    public UIFlowerCounter counter;
    public WeatherSystem weather;
    public int flowerStartSeeds;
    public int flowerStartSpreadIters;
    public float flowerStartSpreadChance;
    public int flowerStartLoopIters;
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
    private FlowerCell[,] flowers;

    
    private Color color1 = new Color(1f, 0.95f, 0.2f); // Yellow-ish
    private Color color2 = new Color(1f, 0.4f, 0.2f); // Red
    private Color color3 = new Color(1f, 0.75f, 0.1f); // Orange-ish


    public void Init(int[,] chunks)
    {
        automata = new FlowerAutomata(
            weather,
            chunks,
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
        flowers = new FlowerCell[automata.automata.GetLength(0), automata.automata.GetLength(1)];
        FillFlowers();
        // for (int i = 0; i < flowerStartSeeds; i++) {
        //     SpawnFlowerRandomly();
        // }
        // for (int i = 0; i < flowerStartSpreadIters; i++) {
        //     SpreadFlowersRandomly();
        // }
        for (int i = 0; i < flowerStartLoopIters; i++) {
            AutomataStep();
        }
        ClearDeadFlowers();
        StartCoroutine(StartAutomataLoop());
    }

    void FillFlowers()
    {
        for (int x = 0; x < automata.automata.GetLength(0); x++) for (int y = 0; y < automata.automata.GetLength(1); y++) {
            if (automata.IsEmpty(x,y)) SpawnFlower(x,y,9);
        }
    }

    // Unused: Tries to pick a random location for a new flower, then spawns
    void SpawnFlowerRandomly()
    {
        int x,y;
        while (true) {
            x = Random.Range(0, automata.automata.GetLength(0));
            y = Random.Range(0, automata.automata.GetLength(1));
            if (automata.automata[x,y] == 0) break;
        }

        SpawnFlower(x,y,9);
    }

    // Initializes a living flower in the automata and the scene
    void SpawnFlower(int x, int y, int automatonValue)
    {
        automata.automata[x,y] = automatonValue;
        float randomAngle = Random.Range(-25, 25);
        Vector3 randomOffset = Random.insideUnitSphere * 0.2f;
        Vector3 pos = new Vector3(x+randomOffset.x, 0, y+randomOffset.z) * flowerCellScale;

        FlowerCell flower = Instantiate(flowerPrefab, transform.position+pos, Quaternion.Euler(0, randomAngle, 0), transform).GetComponent<FlowerCell>();
        Color randomColor = NewFlowerColor();
        flower.Init(this, x, y, randomColor);
        flowers[x,y] = flower;
    }

    // Unused: Makes a flowerbed in Moore's neighborhood around all alive flowers
    void SpreadFlowersRandomly() {
        for (int x = 1; x < automata.automata.GetLength(0)-1; x++) for (int y = 1; y < automata.automata.GetLength(1)-1; y++) {
            if (!automata.IsEmpty(x,y)) continue;
            for (int dx = x-1; dx < x+2; dx++) for (int dy = y-1; dy < y+2; dy++) {
                if (automata.IsAlive(dx,dy)) {
                    bool spawn = Random.value < flowerStartSpreadChance;
                    if (spawn) SpawnFlower(dx,dy,9);
                }
            }
        }
    }

    void ClearDeadFlowers() {
        for (int x = 1; x < automata.automata.GetLength(0)-1; x++) for (int y = 1; y < automata.automata.GetLength(1)-1; y++) {
            if (automata.IsDead(x,y)) {
                automata.automata[x,y] = 0;
            }
        }
        UpdateFlowers();
    }

    Color NewFlowerColor() {
        int randomIndex = Random.Range(0, 3); 
        switch (randomIndex) {
            case 0:
                return color1;
            case 1:
                return color2;
            case 2:
                return color3;
            default:
                return color1;
        }
    }

    private IEnumerator StartAutomataLoop() {
        while (active) {
            AutomataStep();
            yield return new WaitForSeconds(stepPeriodSeconds);
        }
    }

    private void AutomataStep() {
        automata.Step();
        UpdateFlowers();
    }

    // Updates flower objects based on automata
    private void UpdateFlowers() {
        int alive = 0;

        for (int x = 0; x < automata.automata.GetLength(0); x++) for (int y = 0; y < automata.automata.GetLength(1); y++) {
            if (automata.IsAlive(x, y)) {
                if (flowers[x,y] == null) {
                    SpawnFlower(x, y, automata.automata[x,y]);
                }
                alive++;
                flowers[x,y].UpdateAlive(automata.automata[x,y]);
            } else if (automata.IsDead(x,y)) {
                flowers[x,y].UpdateDead();
            } else if (automata.IsEmpty(x,y) && flowers[x,y] != null) {
                Destroy(flowers[x,y].gameObject);
                flowers[x,y] = null;
            }
        }
        counter.UpdateFlowerCount(alive);
    }

    // Updates a flower that was just visited by bees
    public void PollinateFlower(int x, int y) {
        if (!automata.IsAlive(x,y)) return;
        automata.automata[x,y] = 9;
        flowers[x,y].UpdateAlive(9);
    }
}
