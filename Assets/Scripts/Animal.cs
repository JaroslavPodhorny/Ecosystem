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

    [SerializeField] private State currentState;

    // Pathfinding data
    List<Vector2Int> path = new List<Vector2Int>();
    int currentPathIndex = 0;
    [SerializeField] Vector2Int targetPosition;
    Vector2Int dir = Vector2Int.up;

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
        if (terrainData.reserved[targetPosition.x, targetPosition.y])
        {
            isMoving = false;
            Debug.Log("Tile is not walkable or already reserved.");
            return;
        }
        // Reserve the tile for this frame
        terrainData.reserved[targetPosition.x, targetPosition.y] = true;
        // Smoothly rotate towards the target position
        Vector3 targetPosition3D = new Vector3(targetPosition.x, transform.position.y, targetPosition.y);
        Vector3 directionToTarget = (targetPosition3D - transform.position).normalized;
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            targetRotation *= Quaternion.Euler(0, 90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        //calculate how much the animal should move
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 direction = (targetPosition - currentPosition).normalized;
        Vector3 movement = new Vector3(direction.x, 0, direction.y) * animalGenes.speed * Time.deltaTime;

        // update grid position and walkable map
        terrainData.walkable[position.x, position.y] = true;
        terrainData.walkable[targetPosition.x, targetPosition.y] = false;
        position = new Vector2Int(targetPosition.x, targetPosition.y);

        //update the animal position
        float distance = Vector2.Distance(currentPosition, targetPosition);
        if (distance > movement.magnitude + 0.001f)//small threshold to avoid floating point errors
        {
            transform.position += movement;
        }
        else
        {
            //step was finished
            transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.y);
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
                Vector2Int foodBorder = FindFoodBorder();
                if (foodBorder != new Vector2Int(-1, -1))
                {
                    if (foodBorder == position) interacting = true;
                    else if (path.Count == 0)
                    {
                        path = Pathfinding.FindPath(position, foodBorder);
                    }
                    currentState = State.GoingForFood;
                }
                else
                {
                    currentState = State.Exploring;
                }
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
                break;

            case "reproductionUrge":
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
                //if the path has been traversed start interacting and clear the path
                if (currentPathIndex >= path.Count)
                {
                    interacting = true;
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
                if (neighbours.Count == 0)
                {
                    Debug.LogError("stuck");
                    return;//stuck
                }

                //chance that its going to move towards the same direction
                if (UnityEngine.Random.Range(0f, 1f) <= 0.8f)
                {
                    for (int i = 0; i < neighbours.Count; i++)
                    {
                        if (neighbours[i] - position == dir)
                        {
                            //found dir
                            targetPosition = neighbours[i];
                            Move();
                            isMoving = true;
                            return;
                        }
                    }
                }

                int rnd = UnityEngine.Random.Range(0, neighbours.Count - 1);
                dir = neighbours[rnd] - position;
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
        return closestShore;
    }

    Vector2Int FindFoodBorder()
    {
        //get the nearest neighboring tiles of a plant
        nearestPlant = EntityManager.Instance.queryPlant(position);
        List<Vector2Int> plantNeighbours = GameTerrain.WalkableNeighbours(nearestPlant.position);
        float distance = Vector2.Distance(nearestPlant.position, position);

        if (plantNeighbours.Count > 0 && distance < animalGenes.visionRange)
        {
            // choose random tile around the plant
            int rnd = UnityEngine.Random.Range(0, plantNeighbours.Count - 1);
            Vector2Int targetTile = plantNeighbours[rnd];
            return targetTile;
        }
        else if (distance < 2f)
        {
            return position;
        }
        return new Vector2Int(-1, -1); // No food found
    }
    void Interact()
    {
        switch (currentState)
        {
            case State.GoingForWater:
                // Implement interaction logic for water here
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
                if (nearestPlant == null) break;

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


