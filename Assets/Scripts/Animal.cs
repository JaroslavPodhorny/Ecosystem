using Unity.Multiplayer.Center.Common.Analytics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using System.Collections.Generic;
using System.Linq;

public class Animal : MonoBehaviour
{
    // Animal stats
    Genes animalGenes;

    // Desires
    protected float hunger = 0f;
    protected float thirst = 0f;
    protected float rest = 0f;
    protected float reproductionUrge;
    protected float age = 0f;
    private State currentState;

    bool isMoving = false;
    Vector2Int TargetPosition;
    Vector2Int CurrentPosition;

    void Start()
    {
        reproductionUrge = animalGenes.reproductionurge;
    }
    void Update()
    {
        //1 % every second
        hunger += Time.deltaTime * 0.01f;
        thirst += Time.deltaTime * 0.01f;

        if (hunger >= 1f || thirst >= 1f || age >= 1f)
        {
            //register death
            Destroy(gameObject);

        }
        else if isMoving{
            Move();
        }
        else
        {
            Interact();
            ChooseAction();
        }
    }

    void Move()
    {
        // Implement movement logic here

    }

    void ChooseAction()
    {
        var needs = new Dictionary<string, float>
        {
            { "hunger", hunger },
            { "thirst", thirst },
            { "rest", rest },
            { "reproductionUrge", reproductionUrge }
        };

        string highestNeed = needs.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

        switch (highestNeed)
        {
            case "hunger":
                // Handle hunger logic
                break;
            case "thirst":

                break;
            case "rest":
                // Handle rest logic
                break;
            case "reproductionUrge":
                // Handle reproduction logic
                break;
        }
    }




}
