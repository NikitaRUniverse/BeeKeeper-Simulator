using System;
using UnityEngine;

public class PheromoneMap : MonoBehaviour
{
    public float evaporationTime;
    public int numPheromoneTypes;

    public Vector2Int mapSize;
    public float mapCellLength;

    private float mapCellLengthInverse;

    private Pheromone[,,] map;

    void Start() {
        mapCellLengthInverse = 1/mapCellLength;
        map = new Pheromone[numPheromoneTypes, mapSize.x, mapSize.y];
    }


    public void CollectAroundCenter(Pheromone[] targetArray, Vector2 center, PheromoneType type)
    {
        int typeIdx = (int) type;
        float earliestCreationTime = Time.time - evaporationTime;

        Vector2Int cellCoord = CellCoordFromPos(center);
        int i = 0;
		
        for (int dy = -1; dy <= 1; dy++) {
            for (int dx = -1; dx <= 1; dx++)
            {
                int cellX = cellCoord.x + dx;
                int cellY = cellCoord.y + dy;
                if (cellX < 0 || cellX >= mapSize.x || cellY < 0 || cellY >= mapSize.y) {
                    continue;
                }

                Pheromone cell = map[typeIdx, cellX, cellY];
                if (cell == null) continue;
                if (cell.creationTime < earliestCreationTime) continue;

                targetArray[i] = cell;
                i++;
            }
        }

        while (i < targetArray.Length) {
            targetArray[i] = null;
            i++;
        }
    }
    
    private Vector2Int CellCoordFromPos(Vector2 center) {
        int x = (int) ((center.x - transform.position.x) * mapCellLengthInverse);
		int y = (int) ((center.y - transform.position.y) * mapCellLengthInverse);
		return new Vector2Int (Math.Clamp(x, 0, mapSize.x - 1), Math.Clamp(y, 0, mapSize.y - 1));
    }
}
