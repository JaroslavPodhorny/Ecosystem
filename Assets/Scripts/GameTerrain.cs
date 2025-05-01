using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class GameTerrain : MonoBehaviour
{
    public static TerrainData terrainData;

    [Header("Settings")]
    public static int size = 50;
    public static float offsetX, offsetY = 0f;

    //subheader
    [Header("Noise Settings")]
    [Range(0, 1)] public float waterLevel = 0.5f;
    [Range(0, 1)] public float sandLevel = 0.6f;
    [Range(0, 1)] public float treeLevel = 0.45f;
    [Range(0, 1)] public float treeProbability = 0.05f;

    [Header("Prefabs")]
    public GameObject grassPrefab;
    public GameObject waterPrefab;
    public GameObject sandPrefab;
    public GameObject treePrefab;
    public GameObject treePrefab2;

    private float[,] noisemap = GenerateNoiseMap(size, size, 20f, offsetX, offsetY);
    public static bool[,] walkable = new bool[size, size];
    public bool[,] shore = new bool[size, size];

    void Awake()
    {
        GenerateEnviroment();
        terrainData = new TerrainData(size, walkable, shore);
    }

    public static float[,] GenerateNoiseMap(int width, int height, float scale, float offsetX, float offsetY)
    {
        float[,] noiseMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sampleX = (x + offsetX) / scale;
                float sampleY = (y + offsetY) / scale;

                float noiseValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = noiseValue;
            }
        }

        return noiseMap;
    }

    void GenerateEnviroment()
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float value = noisemap[x, y];
                Vector3 position = new Vector3(x, 0, y);

                if (value < waterLevel)
                {
                    Instantiate(waterPrefab, position, Quaternion.identity);
                    walkable[x, y] = false;
                }
                else if (value < sandLevel)
                {
                    Instantiate(sandPrefab, position, Quaternion.identity);
                    shore[x, y] = IsShore(x, y);

                    walkable[x, y] = true;
                }
                else
                {
                    Instantiate(grassPrefab, position, Quaternion.identity);
                    // Mark grass tiles as walkable.
                    walkable[x, y] = true;
                }
            }
        }
        placeTrees(noisemap);
    }

    void placeTrees(float[,] noisemap)
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float value = noisemap[x, y];
                Vector3 position = new Vector3(x, 0, y);

                if (value > treeLevel && UnityEngine.Random.Range(0f, 1f) < 0.1f)
                {
                    GameObject treeToSpawn = UnityEngine.Random.Range(0f, 1f) < 0.5f ? treePrefab : treePrefab2;
                    Quaternion randomRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
                    Instantiate(treeToSpawn, position, randomRotation);

                    walkable[x, y] = false;
                }
            }
        }
    }

    bool IsShore(int x, int y)
    {
        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0) continue; //avoid checking the sand tile itself

                //get the offseted values
                int nx = x + offsetX;
                int ny = y + offsetY;

                if (nx >= 0 && nx < size && ny >= 0 && ny < size)
                {
                    if (noisemap[nx, ny] <= waterLevel)
                    {
                        return true;
                    }

                }
            }
        }
        return false;
    }


    void OnDrawGizmos()
    {
        if (walkable == null || shore == null)
            return;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector3 pos = new Vector3(x, 0.1f, y);

                Gizmos.color = walkable[x, y] ? Color.green : Color.red;
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
                Gizmos.DrawCube(pos, new Vector3(1, 0.01f, 1));

                // draw outlines o cubes
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(pos, new Vector3(1, 0.01f, 1));

                if (shore[x, y])
                {
                    Gizmos.color = new Color(0, 0, 1, 0.5f);
                    Gizmos.DrawCube(pos + Vector3.up * 0.05f, new Vector3(0.6f, 0.01f, 0.6f));
                }
            }
        }
    }

    public static List<Vector2Int> WalkableNeighbours(Vector2Int position)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip the current position

                Vector2Int neighbour = new Vector2Int(position.x + dx, position.y + dy);

                if (neighbour.x >= 0 && neighbour.x < size && neighbour.y >= 0 && neighbour.y < size)
                {
                    if (terrainData.walkable[neighbour.x, neighbour.y])
                    {
                        neighbours.Add(neighbour);
                    }
                }
            }
        }
        return neighbours;
    }
}


public struct TerrainData
{
    public bool[,] walkable;
    public bool[,] shore;
    public int size;

    public TerrainData(int size, bool[,] walkable, bool[,] shore)
    {
        this.size = size;
        this.walkable = walkable;
        this.shore = shore;
    }
}

