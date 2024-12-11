using System.IO;
using UnityEngine;

public class FlowerAutomata
{
    /*
    Cellular automata in Moore's neighborhood

    -5 to -1 -- Dead flower
    0 -- Empty space
    1 to 5 -- Non-pollinating (alive) flower
    6 to 9 -- Pollinating (alive) flower

    6 to 9
    Reduces by 1 with "alive reduction chance"

    1 to 5:
    If at least two neighbors are 6-9 and it's raining, foreach 6-9 neighbor becomes 9 with "pollination chance"
    Else, reduces by 1 with "alive reduction chance" (if is 1, becomes -1)

    0:
    If at least two neighbors are 6-9 and it's raining, foreach 6-9 neighbor becomes 5 with "seeding chance"

    -4 до -1:
    Reduces by 1 with "dead reduction chance"

    -5:
    Becomes 0

    Rule steps are fired periodically.
    Bees pollinate alive flowers between steps, turning flowers to 9 immediately.
    */


    private WeatherSystem weather;
    private Vector2Int chunksSize;
    private int chunkToAutomataLength;
    private int deadSteps;
    private int autPollenStart;
    private int autNonPollenStart;
    private float aliveReductionChance;
    private float deadReductionChance;
    private float pollinationChance;
    private float seedingChance;


    private int autDeadStart;
    private int autDeadLimit;
    private int autEmpty;
    private int autBlocked;

    private int[,] chunks;
    private int[,] automataBuffer;
    private Vector2Int automataSize;

    public int[,] automata;

    
    public FlowerAutomata(WeatherSystem weather, Vector2Int chunksSize, int chunkToAutomataLength, int deadSteps, int autPollenStart, int autNonPollenStart, float aliveReductionChance, float deadReductionChance, float pollinationChance, float seedingChance)
    {
        this.weather = weather;
        this.chunksSize = chunksSize;
        this.chunkToAutomataLength = chunkToAutomataLength;
        this.deadSteps = deadSteps;
        this.autPollenStart = autPollenStart;
        this.autNonPollenStart = autNonPollenStart;
        this.aliveReductionChance = aliveReductionChance;
        this.deadReductionChance = deadReductionChance;
        this.pollinationChance = pollinationChance;
        this.seedingChance = seedingChance;

        autDeadStart = -1;
        autDeadLimit = -deadSteps;
        autEmpty = 0;
        autBlocked = -8;

        if (autPollenStart <= autNonPollenStart) {
            Debug.LogError("");
            return;
        }

        automataSize = chunksSize * chunkToAutomataLength;
        ReadChunks("Assets/output.txt");
        InitializeAutomata();
    }

    public FlowerAutomata(WeatherSystem weather, int[,] chunks, Vector2Int chunksSize, int chunkToAutomataLength, int deadSteps, int autPollenStart, int autNonPollenStart, float aliveReductionChance, float deadReductionChance, float pollinationChance, float seedingChance)
    {
        this.weather = weather;
        this.chunks = chunks;
        this.chunksSize = chunksSize;
        this.chunkToAutomataLength = chunkToAutomataLength;
        this.deadSteps = deadSteps;
        this.autPollenStart = autPollenStart;
        this.autNonPollenStart = autNonPollenStart;
        this.aliveReductionChance = aliveReductionChance;
        this.deadReductionChance = deadReductionChance;
        this.pollinationChance = pollinationChance;
        this.seedingChance = seedingChance;

        autDeadStart = -1;
        autDeadLimit = -deadSteps;
        autEmpty = 0;
        autBlocked = -8;

        if (autPollenStart <= autNonPollenStart) {
            Debug.LogError("");
            return;
        }

        automataSize = chunksSize * chunkToAutomataLength;
        int t = automataSize.x;
        automataSize.x = automataSize.y;
        automataSize.y = t;
        InitializeAutomata();
    }

    private void ReadChunks(string filePath)
    {
        chunks = new int[chunksSize.x, chunksSize.y];
        // Read the grid from the file
        string[] lines = File.ReadAllLines(filePath);

        for (int i = 0; i < lines.Length; i++) {
            string[] values = lines[i].Split(' ');
            for (int j = 0; j < values.Length; j++) {
                chunks[j, i] = int.Parse(values[j]);
            }
        }
    }

    private void InitializeAutomata() {
        automata = new int[automataSize.x, automataSize.y];
        
        for (int chx = 0; chx < chunksSize.x; chx++) for (int chy = 0; chy < chunksSize.y; chy++) {
            bool blocked = chunks[chx, chy] == 1;
            for (int dx = 0; dx < chunkToAutomataLength; dx++) for (int dy = 0; dy < chunkToAutomataLength; dy++) {
                int y = chx*chunkToAutomataLength + dx, x = chy*chunkToAutomataLength + dy;
                if (blocked) {
                    automata[x,automataSize.y-1-y] = autBlocked;
                    continue;
                }
            }
        }
    }

    public void Step() {
        automataBuffer = new int[automataSize.x, automataSize.y];
        
        switch (weather.currentWeather) {
            case WeatherSystem.WeatherType.Sunny:
                pollinationChance = 0.01f;
                seedingChance = 0.02f;
                break;
            case WeatherSystem.WeatherType.Cloudy:
                pollinationChance = 0.01f;
                seedingChance = 0.02f;
                break;
            case WeatherSystem.WeatherType.Rainy:
                pollinationChance = 0.03f;
                seedingChance = 0.05f;
                break;
        }
        
        for (int x = 1; x < automataSize.x-1; x++) for (int y = 1; y < automataSize.y-1; y++) {
            // Blocked
            if (automata[x,y] == autBlocked) {
                goto AutomataPlainCopy;
            }

            // About to go empty
            if (automata[x,y] == autDeadLimit) {
                automataBuffer[x,y] = autEmpty;
                continue;
            }

            bool change;

            // Dying
            if (automata[x,y] <= autDeadStart) {
                change = Random.value < deadReductionChance;
                if (change) {
                    automataBuffer[x,y] = automata[x,y]-1;
                    continue;
                }
                goto AutomataPlainCopy;
            }

            // Getting neighbors for other states
            int pollenAround = 0;
            for (int dx = x-1; dx < x+2; dx++) for (int dy = y-1; dy < y+2; dy++) {
                if (dx == x && dy == y) continue;
                
                if (automata[dx,dy] > autNonPollenStart) {
                    pollenAround++;
                }
            }

            // Empty
            if (automata[x,y] == autEmpty) {
                if (pollenAround > 1) {
                    change = Random.value > Mathf.Pow(1-seedingChance, pollenAround);
                    if (change) {
                        automataBuffer[x,y] = 5;
                        continue;
                    }
                }
                goto AutomataPlainCopy;
            }

            // Alive (pollinating or not)
            if (automata[x,y] <= autPollenStart) {
                if (pollenAround > 1 && automata[x,y] <= autNonPollenStart) {
                    change = Random.value > Mathf.Pow(1-pollinationChance, pollenAround);
                    if (change) {
                        automataBuffer[x,y] = 9;
                        continue;
                    }
                }

                change = Random.value < aliveReductionChance;
                if (change) {
                    if (automata[x,y] == 1) automataBuffer[x,y] = autDeadStart;
                    else automataBuffer[x,y] = automata[x,y]-1;
                    continue;
                }
                goto AutomataPlainCopy;
            }

            AutomataPlainCopy:
            automataBuffer[x,y] = automata[x,y];
        }
        automata = automataBuffer;
    }

    public bool IsEmpty(int x, int y) {
        return automata[x,y] == autEmpty;
    }

    public bool IsBlocked(int x, int y) {
        return automata[x,y] == autBlocked;
    }

    public bool IsAlive(int x, int y) {
        return automata[x,y] > autEmpty;
    }

    public bool IsDead(int x, int y) {
        return automata[x,y] <= autDeadStart && automata[x,y] >= autDeadLimit;
    }
}
