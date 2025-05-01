using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public List<Animal> animals = new List<Animal>();
    public List<Plant> plants = new List<Plant>();
    public List<Animal> predators = new List<Animal>();

    public void RegisterEntity(Entity entity)
    {
        if (entity is Animal animal)
        {
            animals.Add(animal);
        }
        else if (entity is Plant plant)
        {
            plants.Add(plant);
        }
    }

    public void UnregisterEntity(Entity entity)
    {
        if (entity is Animal animal)
        {
            animals.Remove(animal);
            predators.Remove(animal); // If it's a predator, remove it from predators as well
        }
        else if (entity is Plant plant)
        {
            plants.Remove(plant);
        }
    }

    public Plant queryPlant(Vector2Int position)
    {
        float shortestDistance = 1000f;
        Plant closestPlant = null;
        foreach (var plant in plants)
        {
            float distance = Vector2Int.Distance(plant.position, position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestPlant = plant;
            }
        }
        return closestPlant;//returns null if no plant has been found
    }
}