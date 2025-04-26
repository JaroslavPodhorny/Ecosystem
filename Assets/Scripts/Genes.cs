using UnityEngine;

public class Genes
{
    public float speed;
    public float visionRange;
    public float gestationTime;
    public bool isMale;
    public float fov;
    public float reproductionurge;

    public Genes(float speed, float visionRange, float gestationTime, bool isMale, float fov, float reproductionurge)
    {
        this.speed = speed;
        this.visionRange = visionRange;
        this.gestationTime = gestationTime;
        this.isMale = isMale;
        this.fov = fov;
        this.reproductionurge = reproductionurge;
    }

    public Genes GetRandomGenes()
    {
        // values are in meters
        speed = Random.Range(0.25f, 3f);
        visionRange = Random.Range(1f, 10f);
        gestationTime = Random.Range(1f, 10f);
        isMale = Random.Range(0f, 1f) > 0.5f;
        fov = Random.Range(20f, 342f);
        reproductionurge = Random.Range(0f, 1f);

        return new Genes(speed, visionRange, gestationTime, isMale, fov, reproductionurge);

    }


}
