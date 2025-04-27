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
    protected float hunger = 0f;
    protected float thirst = 0.01f;
    protected float tiredness = 0f;
    protected float reproductionUrge;
    protected float plantSizeTreshold = 0.5f;

    private State currentState;

    // Pathfinding data
    List<Vector2Int> path = new List<Vector2Int>();
    int currentPathIndex = 0;
    Vector2Int targetPosition;

    bool isMoving = false;
    bool interacting = false;


    void Start()
    {
        animalGenes = AnimalGenes.GetRandomGenes();
        EntityManager.Instance.RegisterAnimal(this);
        position = new Vector2Int((int)transform.position.x, (int)transform.position.z);

        // debug values
        reproductionUrge = 0.05f;
        animalGenes.visionRange = 10;

    }
    void Update()
    {
        //1 % every second
        hunger += Time.deltaTime * 0.01f;
        thirst += Time.deltaTime * 0.01f;

        if (hunger >= 1f || thirst >= 1f)
        {
            //register death
            Destroy(gameObject);

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
            { "rest", tiredness },
            { "reproductionUrge", reproductionUrge }
        };

        string highestNeed = needs.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

        switch (highestNeed)
        {
            case "hunger":
                if (EntityManager.Instance.queryPlant(position) != null)
                {
                    if (path.Count == 0)
                    {
                        path = Pathfinding.FindPath(position, EntityManager.Instance.queryPlant(position).position);
                        Debug.Log(EntityManager.Instance.queryPlant(position).position);
                        Debug.Log(position);
                    }
                    Debug.Log($"Path found: {path.Count}");
                    currentState = State.GoingForFood;
                }
                else
                {
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
                    Debug.Log($"Path found: {path.Count}");
                    currentState = State.GoingForWater;
                }
                else
                {
                    currentState = State.Exploring;
                }

                Debug.Log("Thirst is the highest need.");
                break;

            case "rest":

                Debug.Log("Rest is the highest need.");
                currentState = State.Resting;
                interacting = true;
                Interact();
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
            case State.Exploring:

                break;
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
                    Vector2Int currentPosition = new Vector2Int((int)transform.position.x, (int)transform.position.z);
                    targetPosition = currentPosition + path[currentPathIndex];
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
        }
    }

    Vector2Int FindWater()
    {
        bool[,] shoremap = FindFirstObjectByType<GameTerrain>().shore;
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

            case State.Resting:
                if (tiredness > 0)
                {
                    tiredness -= Time.deltaTime * 0.1f;
                }
                else
                {
                    tiredness = 0f;
                    interacting = false;
                }
                break;

            case State.GoingForFood:
                // Implement interaction logic for food here

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


