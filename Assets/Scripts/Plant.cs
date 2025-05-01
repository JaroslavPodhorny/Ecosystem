using UnityEngine;

public class Plant : Entity
{

    PlantGenes plantGenes;
    private float reproductionTime;

    TerrainData terrainData;

    public static float referencescale = 0.45f;


    void Start()
    {
        EntityManager.Instance.RegisterEntity(this);

        position = new Vector2Int((int)transform.position.x, (int)transform.position.z);

        plantGenes = PlantGenes.GetRandomGenes();
        reproductionTime = plantGenes.reproductionInterval;

        terrainData = GameTerrain.terrainData;
        terrainData.walkable[position.x, position.y] = false;

        transform.localScale = Vector3.one * referencescale * plantGenes.size;
        transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

    }

    void Update()
    {
        if (reproductionTime <= 0f)
        {
            Reproduce();
            reproductionTime = plantGenes.reproductionInterval;
        }
        else
        {
            reproductionTime -= Time.deltaTime;
        }
    }
    void Reproduce()
    {
        Vector2Int randomNeighbor = new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
        Vector2Int newPosition = new Vector2Int(position.x + randomNeighbor.x, position.y + randomNeighbor.y);

        // Check if the new position is valid 
        bool walkable = terrainData.walkable[newPosition.x, newPosition.y];
        bool withinBounds = newPosition.x >= 0 && newPosition.x < terrainData.size &&
        newPosition.y >= 0 && newPosition.y < terrainData.size;

        if (walkable && withinBounds)
        {
            Plant newPlant = Instantiate(gameObject, new Vector3(newPosition.x, 0, newPosition.y), Quaternion.identity).GetComponent<Plant>();
            newPlant.plantGenes = PlantGenes.GetRandomGenes();
            newPlant.position = position;

        }
    }
}
