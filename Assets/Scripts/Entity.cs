using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public Vector2Int position;
    public float health = 1f;


    public void Die()
    {
        //EnviromentManager.RegisterDeath(this);
        Destroy(gameObject);
    }

}
