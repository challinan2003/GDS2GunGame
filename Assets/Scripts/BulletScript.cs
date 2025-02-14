using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float survivalTime; //How long the bullet will be allowed to fly before it deletes itself

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject); // Destroy bullet on impact with anything with a collider
    }

    void Start()
    {
        Destroy(gameObject, survivalTime); // Automatically destroy bullet after survivalTime is up
    }

    // test again
}
