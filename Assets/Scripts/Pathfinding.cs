using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class Pathfinding
{
    public static List<Vector2Int> FindPath(Vector2Int startcoor, Vector2Int endcoor)
    {
        TerrainData terrainData = GameTerrain.terrainData;

        // A* setup
        List<Vector2Int> openSet = new List<Vector2Int> { startcoor };
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();

        gScore[startcoor] = 0f;
        fScore[startcoor] = Heuristic(startcoor, endcoor);

        // Main A* loop
        while (openSet.Count > 0)
        {
            // Find node in openSet with lowest fScore.
            Vector2Int current = openSet[0];
            float lowestF = fScore.ContainsKey(current) ? fScore[current] : Mathf.Infinity;
            foreach (var pos in openSet)
            {
                float score = fScore.ContainsKey(pos) ? fScore[pos] : Mathf.Infinity;
                if (score < lowestF)
                {
                    lowestF = score;
                    current = pos;
                }
            }
            if (current == endcoor)
            {
                // Reconstruct full node path from start to end.
                List<Vector2Int> nodePath = ReconstructPath(cameFrom, current);
                // Now convert node path to offset list.
                List<Vector2Int> offsetPath = new List<Vector2Int>();
                for (int i = 0; i < nodePath.Count - 1; i++)
                {
                    offsetPath.Add(nodePath[i + 1] - nodePath[i]);
                }
                Debug.Log("Node Path: " + string.Join(" -> ", nodePath.Select(v => $"({v.x},{v.y})")));
                Debug.Log("Offset Path: " + string.Join(", ", offsetPath.Select(v => $"({v.x},{v.y})")));
                return offsetPath;
            }
            openSet.Remove(current);
            closedSet.Add(current);
            // Get neighbors in all 8 directions.
            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;
                float tentativeG = gScore[current] + Distance(current, neighbor);
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeG >= (gScore.ContainsKey(neighbor) ? gScore[neighbor] : Mathf.Infinity))
                {
                    continue;
                }
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeG;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, endcoor);
            }
        }
        Debug.LogWarning("No path found!");
        return new List<Vector2Int>();

        // Helper: Manhattan heuristic.
        float Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        // Helper: Diagonal moves cost sqrt(2) and orthogonal moves cost 1.
        float Distance(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx == 1 && dy == 1) ? 1.41421f : 1f;
        }

        // Helper: Returns a list of neighbor coordinates that are in bounds and walkable.
        List<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    Vector2Int neighbor = new Vector2Int(pos.x + dx, pos.y + dy);
                    if (neighbor.x >= 0 && neighbor.x < terrainData.size &&
                        neighbor.y >= 0 && neighbor.y < terrainData.size)
                    {
                        if (terrainData.walkable[neighbor.x, neighbor.y] || neighbor.x == startcoor.x && neighbor.y == startcoor.y)
                            neighbors.Add(neighbor);
                    }
                }
            }
            return neighbors;
        }

        // Helper: Reconstructs the path by following cameFrom links.
        List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            List<Vector2Int> totalPath = new List<Vector2Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, current);
            }
            return totalPath;
        }
    }
}
