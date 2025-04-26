using Unity.Mathematics;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    [Header("Settings")]
    public static int size = 50;
    [Range(0, 1)] public float waterLevel = 0.5f;
    [Range(0, 1)] public float sandLevel = 0.6f;
    [Range(0, 1)] public float treeLevel = 0.45f;
    [Range(0, 1)] public float treeProbability = 0.05f;


    [Header("Prefabs")]
    public GameObject grassPrefab;
    public GameObject waterPrefab;
    public GameObject sandPrefab;
    public GameObject treePrefab;

    private float[,] noisemap = GenerateNoiseMap(size, size, 20f, 0f, 0f);
    public bool[,] walkable = new bool[size, size];
    public bool[,] shore = new bool[size, size];




    void Start()
    {
        GenerateEnviroment();
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
                    if (IsShore(x, y)) Debug.DrawLine(position, position + Vector3.up * 2, Color.yellow, 100f); // Draw a yellow line for shore tiles
                }
                else
                {
                    Instantiate(grassPrefab, position, Quaternion.identity);
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
                    Instantiate(treePrefab, position, Quaternion.identity);
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
                if (offsetX == 0 && offsetY == 0) continue; //avoid checking the water tile

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
}

