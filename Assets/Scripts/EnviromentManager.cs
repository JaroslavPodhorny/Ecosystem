using UnityEngine;

public class EnviromentManager : MonoBehaviour
{
    public GameObject animalPrefab;
    public GameObject plantPrefab;

    void Start()
    {
        InitialPopulation();
    }

    void InitialPopulation()
    {
        for (int i = 0; i < 50; i++)
        {
            Vector2Int randomPosition = new Vector2Int(Random.Range(0, GameTerrain.size), Random.Range(0, GameTerrain.size));
            if (GameTerrain.terrainData.walkable[randomPosition.x, randomPosition.y])
            {
                GameObject animal = Instantiate(animalPrefab, new Vector3(randomPosition.x, 0.7f, randomPosition.y), Quaternion.identity);
                animal.GetComponent<Animal>().position = randomPosition;
            }
        }

        //spawn some plants
        for (int i = 0; i < 3; i++)
        {
            Vector2Int randomPosition = new Vector2Int(Random.Range(0, GameTerrain.size), Random.Range(0, GameTerrain.size));
            if (GameTerrain.terrainData.walkable[randomPosition.x, randomPosition.y])
            {
                GameObject plant = Instantiate(plantPrefab, new Vector3(randomPosition.x, 0, randomPosition.y), Quaternion.identity);
                plant.GetComponent<Plant>().position = randomPosition;
            }
        }

    }
}
