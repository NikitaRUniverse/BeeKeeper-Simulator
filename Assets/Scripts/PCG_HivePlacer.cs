using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HiveGenerator : MonoBehaviour
{
    // Customizable parameters
    public float ax = 1f; // Multiplier for x coordinates
    public float az = 1f; // Multiplier for y coordinates
    public float wx = 1f; // Multiplier for x coordinates for width
    public float hz = 1f; // Multiplier for y coordinates for heigth
    public float bx = 0f; // Bias for x coordinates
    public float bz = 0f; // Bias for y coordinates
    public GameObject hivePrefab; // Hive prefab
    public int randomSeed = 0; // Random seed for deterministic generation
    public int maxHives = 10; // Maximum number of hives
    public int minDistanceBetweenHives = 5; // Minimum distance between hives in grid cells
    public string inputFilePath = "Assets/MapMatrix.txt"; // Input file path
    public string outputFilePath = "Assets/output.txt"; // Output file path
    public int hivePositionOffset = 0;
    public float yCoord;

    private int width;
    private int height;

    private void Start()
    {
        // Initialize random seed
        UnityEngine.Random.InitState(randomSeed);

        // Read grid from input file
        int[,] grid = ReadGridFromFile(inputFilePath);

        // Generate hive positions using Poisson Disk Sampling
        List<Vector2> hivePositions = GenerateHivePositions(grid);

        // Instantiate Hive prefab at generated positions
        InstantiateHives(hivePositions);

        // Write grid with occupied cells to output file
        WriteGridToFile(grid, outputFilePath);
    }

    private int[,] ReadGridFromFile(string filePath)
    {
        // Read the grid from the file
        string[] lines = File.ReadAllLines(filePath);
        int[,] grid = new int[lines.Length, lines[0].Split(' ').Length];
        width = grid.GetLength(0);
        height = grid.GetLength(1);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(' ');
            for (int j = 0; j < values.Length; j++)
            {
                grid[i, j] = int.Parse(values[j]);
            }
        }

        return grid;
    }

    private void WriteGridToFile(int[,] grid, string filePath)
    {
        // Create a new grid to store the modified values
        int[,] newGrid = new int[grid.GetLength(0), grid.GetLength(1)];

        // Copy the original grid to the new grid
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                newGrid[i, j] = grid[i, j];
            }
        }

        // Iterate through the grid to find all '1' and mark neighbors in the new grid
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] == 1)
                {
                    // Update neighbors in the new grid
                    if (i > 0 && grid[i - 1, j] == 0) newGrid[i - 1, j] = 1; // Top
                    if (i < grid.GetLength(0) - 1 && grid[i + 1, j] == 0) newGrid[i + 1, j] = 1; // Bottom
                    if (j > 0 && grid[i, j - 1] == 0) newGrid[i, j - 1] = 1; // Left
                    if (j < grid.GetLength(1) - 1 && grid[i, j + 1] == 0) newGrid[i, j + 1] = 1; // Right
                }
            }
        }

        // Write the new grid to the file
        string output = "";

        for (int i = 0; i < newGrid.GetLength(0); i++)
        {
            for (int j = 0; j < newGrid.GetLength(1); j++)
            {
                output += newGrid[i, j] + " ";
            }
            output += Environment.NewLine;
        }

        File.WriteAllText(filePath, output);
    }



    private List<Vector2> GenerateHivePositions(int[,] grid)
    {
        // Initialize the list of hive positions
        List<Vector2> hivePositions = new List<Vector2>();

        // Fool check
        if (hivePositionOffset >= grid.GetLength(0) || hivePositionOffset >=  grid.GetLength(1))
        {
            throw new Exception("Hive position offset can't be bigger or equal than any of grid dimensions!");
        }

        if (maxHives == 1)
        {
            // Spawn on center in case of 1 hive to spawn
            int center_x = height / 2;
            int center_y = width / 2;
            hivePositions.Add(new Vector2(center_x, center_y));
            grid[center_x, center_y] = 1;
        } else
        {
            // Iterate over the grid to find potential hive positions
            for (int x = hivePositionOffset; x < grid.GetLength(0) - hivePositionOffset; x++)
            {
                for (int y = hivePositionOffset; y < grid.GetLength(1) - hivePositionOffset; y++)
                {
                    // Check if the current cell is a potential hive position (i.e., it has a value of 0)
                    if (grid[x, y] == 0)
                    {
                        // Check if the maximum number of hives has been reached
                        if (hivePositions.Count >= maxHives)
                        {
                            return hivePositions;
                        }

                        // Check if the current cell is far enough from existing hive positions
                        if (IsFarEnoughFromExistingHives(hivePositions, new Vector2(y, x)))
                        {
                            // Add the current cell to the list of hive positions
                            hivePositions.Add(new Vector2(y, x));

                            // Mark the current cell as occupied (i.e., set its value to 1)
                            grid[x, y] = 1;
                        }
                    }
                }
            }
        }

        return hivePositions;
    }

    private bool IsFarEnoughFromExistingHives(List<Vector2> hivePositions, Vector2 position)
    {
        // Calculate the minimum distance between the current position and existing hive positions
        float minDistance = Mathf.Infinity;

        // Iterate over existing hive positions
        foreach (Vector2 hivePosition in hivePositions)
        {
            // Calculate the distance between the current position and the existing hive position
            float distance = CalculateDistance(position, hivePosition);

            // Update the minimum distance if the current distance is smaller
            if (distance < minDistance)
            {
                minDistance = distance;
                if (minDistance < minDistanceBetweenHives) break;
            }
        }

        // Check if the minimum distance is greater than or equal to the minimum distance between hives
        return minDistance >= minDistanceBetweenHives;
    }

    private float CalculateDistance(Vector2 position1, Vector2 position2)
    {
        // Calculate the distance between two positions
        return Mathf.Sqrt(Mathf.Pow(position1.x - position2.x, 2) + Mathf.Pow(position1.y - position2.y, 2));
    }

    private void InstantiateHives(List<Vector2> hivePositions)
    {
        // Iterate over the hive positions
        foreach (Vector2 hivePosition in hivePositions)
        {
            // Calculate the instantiation coordinates
            float instantiationX = (hivePosition.x * ax) + bx + height * hz;
            float instantiationZ = (hivePosition.y * az) + bz + width * wx;
            
            // Instantiate the Hive prefab at the calculated coordinates
            GameObject hive = Instantiate(hivePrefab, new Vector3(instantiationX, yCoord, instantiationZ), Quaternion.identity);
        }
    }
}