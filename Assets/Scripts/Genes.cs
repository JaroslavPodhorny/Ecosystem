using UnityEngine;
using UnityEngine.EventSystems;



public class AnimalGenes
{
    public float speed;
    public int visionRange;
    public float gestationTime;
    public bool isMale;
    public float reproductionurge;

    public AnimalGenes(float speed, int visionRange, float gestationTime, bool isMale, float reproductionurge)
    {
        this.speed = speed;
        this.visionRange = visionRange;
        this.gestationTime = gestationTime;
        this.isMale = isMale;
        this.reproductionurge = reproductionurge;
    }

    public static AnimalGenes GetRandomGenes()
    {
        // values are in meters
        float speed = Random.Range(0.25f, 3f); // m/s
        int visionRange = Random.Range(1, 10);
        float gestationTime = Random.Range(1f, 10f);
        bool isMale = Random.Range(0f, 1f) > 0.5f;
        float reproductionurge = Random.Range(0f, 0.5f);

        return new AnimalGenes(speed, visionRange, gestationTime, isMale, reproductionurge);
    }
}

public class PlantGenes
{
    public float size;
    public float nutrition;
    public float reproductionInterval;

    public PlantGenes(float size, float nutrition, float reproductionInterval)
    {
        this.size = size;
        this.nutrition = nutrition;
        this.reproductionInterval = reproductionInterval;
    }

    public static PlantGenes GetRandomGenes()
    {
        float size = Random.Range(0.5f, 1.5f);
        float nutrition = Random.Range(0.1f, 0.5f);
        float reproductionInterval = Random.Range(7f, 14f);

        return new PlantGenes(size, nutrition, reproductionInterval);
    }
}
