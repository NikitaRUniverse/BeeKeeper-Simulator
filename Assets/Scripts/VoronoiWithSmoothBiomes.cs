using UnityEngine;
using System.Text; 
using System.IO; 


public class VoronoiWithSmoothBiomes : MonoBehaviour
{
    public int randomSeed = 0;
    public int textureWidth = 1920; 
    public int textureHeight = 1080; 
    public int numPoints = 50; // Number of random points
    public float noiseScale = 0.01f; // Noise scale
    public Color biomeColor1 = Color.green; 
    public Color biomeColor2 = Color.yellow; 
    public float transitionSharpness = 10.0f; // Controls the sharpness of the transition

    System.Random random;
    private Texture2D texture;
    private Vector2[] points; // Random points
    private Color[] regionColors; // Colors of Voronoi segments
    private float[,] noiseMap; // Noise map

    void Start()
    {
        UnityEngine.Random.InitState(randomSeed);
        random = new System.Random(randomSeed);
        GenerateNoiseMap();
        GenerateRandomPoints();
        AssignBiomesToRegions();
        ApplySmoothVoronoi();
        SaveTextureToFile(); 
        SaveMapToFile(get_map()); 
    }

    void GenerateNoiseMap()
{
    int gridSize = 16; // Grid size for Value Noise
    int gridX = Mathf.CeilToInt(textureWidth / (float)gridSize);
    int gridY = Mathf.CeilToInt(textureHeight / (float)gridSize);

    // Grid of random values
    float[,] gridValues = new float[gridX + 1, gridY + 1];
    noiseMap = new float[textureWidth, textureHeight];

    // Generate random values for grid nodes
    for (int x = 0; x <= gridX; x++)
    {
        for (int y = 0; y <= gridY; y++)
        {
            gridValues[x, y] = UnityEngine.Random.value;
        }
    }

    // Interpolate values for each pixel
    for (int y = 0; y < textureHeight; y++)
    {
        for (int x = 0; x < textureWidth; x++)
        {
            float gx = x / (float)gridSize;
            float gy = y / (float)gridSize;

            int x0 = Mathf.FloorToInt(gx);
            int x1 = x0 + 1;
            int y0 = Mathf.FloorToInt(gy);
            int y1 = y0 + 1;

            float dx = gx - x0;
            float dy = gy - y0;

            float v00 = gridValues[x0, y0];
            float v10 = gridValues[x1, y0];
            float v01 = gridValues[x0, y1];
            float v11 = gridValues[x1, y1];

            float v0 = Mathf.Lerp(v00, v10, dx);
            float v1 = Mathf.Lerp(v01, v11, dx);
            float noiseValue = Mathf.Lerp(v0, v1, dy);

            // Brightening the center of the map to create a flield
            Vector2 center = new Vector2(textureWidth / 2f, textureHeight / 2f);
            float distanceToCenter = Vector2.Distance(new Vector2(x, y), center) / (textureWidth / 2f);
            distanceToCenter = Mathf.Clamp01(distanceToCenter);
            noiseValue += (1 - distanceToCenter) * 0.5f;

            noiseMap[x, y] = Mathf.Clamp01(noiseValue);
        }
    }
}


    void GenerateRandomPoints()
    {
        points = new Vector2[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float x = random.Next(0, textureWidth);
            float y = random.Next(0, textureHeight);
            points[i] = new Vector2(x, y);
        }
    }

    void AssignBiomesToRegions()
    {
        regionColors = new Color[numPoints];

        for (int i = 0; i < points.Length; i++)
        {
            float averageNoise = GetAverageNoiseAroundPoint(points[i]);

            // Assign color based on average noise value
            regionColors[i] = averageNoise > 0.5f ? biomeColor2 : biomeColor1;
        }
    }

    float GetAverageNoiseAroundPoint(Vector2 point)
    {
        int radius = 5; // Radius of the region for calculating the average noise
        float sum = 0f;
        int count = 0;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int sampleX = Mathf.Clamp((int)point.x + x, 0, textureWidth - 1);
                int sampleY = Mathf.Clamp((int)point.y + y, 0, textureHeight - 1);

                sum += noiseMap[sampleX, sampleY];
                count++;
            }
        }

        return sum / count; 
    }

    void ApplySmoothVoronoi()
    {
        texture = new Texture2D(textureWidth, textureHeight);

        // For each pixel we calculate the weighted color
        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                Vector2 currentPixel = new Vector2(x, y);

                float totalWeight = 0f;
                Color blendedColor = Color.black;

                for (int i = 0; i < points.Length; i++)
                {
                    float distance = Vector2.Distance(currentPixel, points[i]);
                    float weight = Mathf.Pow(1 / (distance + 1), transitionSharpness); 

                    blendedColor += regionColors[i] * weight;
                    totalWeight += weight;
                }

                // Normalize the color
                blendedColor /= totalWeight;
                texture.SetPixel(x, y, blendedColor);
            }
        }

        texture.Apply();

        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.position = Vector3.zero;
            plane.transform.localScale = new Vector3(16, 1, 9);
            plane.transform.Rotate(0.0f, 180.0f, 0.0f);
            plane.name = "VoronoiWithSmoothBiomesPlane";

            renderer = plane.GetComponent<Renderer>();
        }

        if (renderer.material == null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
        }

        renderer.material.mainTexture = texture;
    }

    //Save texture to file
    void SaveTextureToFile()
    {
        byte[] bytes = texture.EncodeToPNG();
        string filePath = Path.Combine(Application.dataPath, "GeneratedTexture.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Texture saved to: {filePath}");
    }

    int[,] get_map()
{
    int cellWidth = 60;
    int cellHeight = 60;

    int cols = textureWidth / cellWidth;
    int rows = textureHeight / cellHeight;

    int[,] mapMatrix = new int[rows, cols];
    // Here I create a matrix with chanks for future work of my groupmates. 
    // 0 - chank is free and you can place here what you need(hive, flower)
    for (int row = 0; row < rows; row++)
    {
        for (int col = 0; col < cols; col++)
        {
            bool hasBiome2 = false;

            int invertedRow = rows - 1 - row;

            for (int y = invertedRow * cellHeight; y < (invertedRow + 1) * cellHeight; y++)
            {
                for (int x = col * cellWidth; x < (col + 1) * cellWidth; x++)
                {
                    Color pixelColor = texture.GetPixel(x, y);

                    // If the color is close to the color of the second biome, we mark the presence
                    if (IsColorSimilar(pixelColor, biomeColor2))
                    {
                        hasBiome2 = true;
                        break;
                    }
                }
                if (hasBiome2) break; // Stop checking if biome is found
            }

            mapMatrix[row, col] = hasBiome2 ? 0 : 1;
        }
    }

    return mapMatrix;
}
    //Save matrix to file 
    void SaveMapToFile(int[,] mapMatrix)
    {
        StringBuilder sb = new StringBuilder();
        int rows = mapMatrix.GetLength(0);
        int cols = mapMatrix.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                sb.Append(mapMatrix[row, col]);
                if (col < cols - 1) sb.Append(" ");
            }
            sb.AppendLine();
        }

        string filePath = Path.Combine(Application.dataPath, "MapMatrix.txt");
        File.WriteAllText(filePath, sb.ToString());

        Debug.Log($"Map matrix saved to: {filePath}");
    }

    bool IsColorSimilar(Color a, Color b, float tolerance = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}
