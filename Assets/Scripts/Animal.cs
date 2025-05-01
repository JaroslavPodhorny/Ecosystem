using Unity.Multiplayer.Center.Common.Analytics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using System.Collections.Generic;
using System.Linq;
using System;


public class Animal : Entity
{
    // Animal stats
    AnimalGenes animalGenes;

    // Desires
    public float hunger = 0f;
    public float thirst = 0.01f;
    public float reproductionUrge;
    //future use: plant/animal coexistence
    protected float plantSizeTreshold = 0.5f;

    private State currentState;

    // Pathfinding data
    List<Vector2Int> path = new List<Vector2Int>();
    int currentPathIndex = 0;
    Vector2Int targetPosition;

    bool isMoving = false;
    bool interacting = false;

    static TerrainData terrainData;
    Plant nearestPlant = null;



    void Start()
    {
        terrainData = GameTerrain.terrainData;

        terrainData.walkable[position.x, position.y] = false;

        animalGenes = AnimalGenes.GetRandomGenes();
        EntityManager.Instance.RegisterEntity(this);
        position = new Vector2Int((int)transform.position.x, (int)transform.position.z);

        // debug values
        reproductionUrge = 0.01f;
    }
    void Update()
    {
        //1 % every second
        hunger += Time.deltaTime * 0.01f;
        thirst += Time.deltaTime * 0.01f;

        if (hunger >= 1f || thirst >= 1f)
        {
            Die();
        }
        else if (isMoving)
        {
            Move();
        }
        else if (interacting)
        {
            Interact();
        }
        else
        {
            ChooseAction();
            ExecuteAction();
        }
    }

    void Move()
    {
        // update walkable map
        terrainData.walkable[position.x, position.y] = true;
        terrainData.walkable[targetPosition.x, targetPosition.y] = false;

        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
        float distance = Vector2.Distance(currentPosition, targetPosition);
        if (distance > 0.1f)
        {
            Vector2 direction = (targetPosition - currentPosition).normalized;
            Vector3 movement = new Vector3(direction.x, 0, direction.y) * animalGenes.speed * Time.deltaTime;
            transform.position += movement;
        }
        else
        {
            //step was finished
            transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.y);
            position = new Vector2Int((int)targetPosition.x, (int)targetPosition.y);
            isMoving = false;
        }
    }


    void ChooseAction()
    {

        var needs = new Dictionary<string, float>
        {
            { "hunger", hunger },
            { "thirst", thirst },
            { "reproductionUrge", reproductionUrge }
        };

        string highestNeed = needs.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

        switch (highestNeed)
        {
            case "hunger":
                nearestPlant = EntityManager.Instance.queryPlant(position);
                if (nearestPlant != null)
                {
                    float distance = Vector2.Distance(nearestPlant.position, position);
                    Debug.Log($"Distance to nearest plant: {distance}");
                    if (distance < animalGenes.visionRange)
                    {
                        List<Vector2Int> plantNeighbours = GameTerrain.WalkableNeighbours(nearestPlant.position);
                        if (plantNeighbours.Count == 0) plantNeighbours.Add(position);
                        int rnd = UnityEngine.Random.Range(0, plantNeighbours.Count - 1);
                        Vector2Int targetTile = plantNeighbours[rnd];

                        if (path.Count == 0)
                        {
                            path = Pathfinding.FindPath(position, targetTile);
                        }
                        currentState = State.GoingForFood;
                        Debug.Log("Going for food.");
                    }
                    else
                    {
                        Debug.Log("No food found within vision range, exploring.");
                        currentState = State.Exploring;
                    }
                }
                else
                {
                    Debug.Log("No plant found, exploring.");
                    currentState = State.Exploring;
                }
                Debug.Log("Hunger is the highest need.");
                break;

            case "thirst":
                if (FindWater() != new Vector2Int(-1, -1))
                {
                    if (path.Count == 0)
                    {
                        path = Pathfinding.FindPath(position, FindWater());
                    }
                    currentState = State.GoingForWater;
                }
                else
                {
                    currentState = State.Exploring;
                }
                Debug.Log("Thirst is the highest need.");
                break;

            case "reproductionUrge":
                Debug.Log("Reproduction is the highest need.");
                currentState = State.FindingMate;
                break;
        }
    }

    void ExecuteAction()
    {
        switch (currentState)
        {
            case State.GoingForFood:
                if (currentPathIndex >= path.Count)
                {
                    interacting = true;
                    Debug.Log("Reached the food source.");
                    path.Clear();
                    currentPathIndex = 0;
                }
                else
                {
                    targetPosition = position + path[currentPathIndex];
                    isMoving = true;
                    Move();
                    currentPathIndex++;
                }
                break;
            case State.GoingForWater:
                Debug.Log("Going for water");
                //if the path has been traversed start interacting and clear the path
                if (currentPathIndex >= path.Count)
                {
                    interacting = true;
                    Debug.Log("Reached the water source.");
                    path.Clear();
                    currentPathIndex = 0;
                }
                else
                {
                    targetPosition = position + path[currentPathIndex];
                    isMoving = true;
                    Move();
                    currentPathIndex++;
                }
                break;

            case State.FindingMate:
                // Implement finding mate logic here
                break;
            case State.AvoidingPredator:
                // Implement avoiding predator logic here
                break;

            case State.Exploring:
                List<Vector2Int> neighbours = GameTerrain.WalkableNeighbours(position);
                int rnd = UnityEngine.Random.Range(0, neighbours.Count - 1);
                targetPosition = neighbours[rnd];
                Move();
                isMoving = true;
                break;
        }
    }

    Vector2Int FindWater()
    {
        bool[,] shoremap = terrainData.shore;
        int visionRange = animalGenes.visionRange;
        Vector2Int currentPosition = new Vector2Int((int)transform.position.x, (int)transform.position.z);

        Vector2Int closestShore = new Vector2Int(-1, -1); //This value means no shore been found
        float closestDistance = float.MaxValue;

        for (int i = -visionRange; i <= visionRange; i++)
        {
            for (int j = -visionRange; j <= visionRange; j++)
            {
                int x = currentPosition.x + i;
                int y = currentPosition.y + j;

                // Check if the position is within the bounds of the map
                if (x >= 0 && x < shoremap.GetLength(0) && y >= 0 && y < shoremap.GetLength(1))
                {
                    if (shoremap[x, y])
                    {
                        float distance = Vector2Int.Distance(currentPosition, new Vector2Int(x, y));
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestShore = new Vector2Int(x, y);
                        }
                    }
                }
            }
        }
        Debug.Log($"Closest shore found at: {closestShore}" + $" with distance: {closestDistance}");
        return closestShore;
    }
    void Interact()
    {
        switch (currentState)
        {
            case State.GoingForWater:
                // Implement interaction logic for water here
                Debug.Log("Drinking water.");
                if (thirst > 0)
                {
                    thirst -= Time.deltaTime * 0.1f;
                }
                else
                {
                    thirst = 0f;
                    interacting = false;
                }
                break;

            case State.GoingForFood:
                // Implement interaction logic for food here
                Debug.Log("Eating food.");
                float plantsize = nearestPlant.transform.localScale.x;
                if (plantsize > 0f && hunger > 0)
                {
                    plantsize -= Time.deltaTime * 0.1f;
                    nearestPlant.transform.localScale = new Vector3(plantsize, plantsize, plantsize);
                    hunger -= Time.deltaTime * 0.1f;
                }
                else
                {
                    if (plantsize < 0) nearestPlant.Die();
                    interacting = false;
                }
                break;

            case State.FindingMate:
                //reproduction logic
                interacting = false;
                break;

            default:
                break;
        }

    }
}


