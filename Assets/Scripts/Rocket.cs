using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [Header("Rocket Config")]
    public float launchForce;
    public float radius;
    public float playerLaunchForce;

    public LayerMask layers;

    private Vector3 explosionPos;

    private void Start()
    {
        Vector3 euler = new Vector3(90f, 0f, 0f);

        transform.rotation = Quaternion.Euler(euler);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Player Mesh")
        {
            Destroy(gameObject); // Delay destruction so gizmo is visible for a short time
        }

        explosionPos = other.contacts[0].point;

        //only check for player and enemies layers for collisions
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius, layers);

        foreach (Collider hit in colliders)
        {
            if (hit.name == "Player Mesh")
            {
                Rigidbody rb = hit.GetComponentInParent<Rigidbody>();
                Vector3 playerPos = hit.gameObject.transform.position;
                float distanceToExplosion = Vector3.Distance(playerPos, explosionPos);

                //check if the player is 2 units away from the explosion point
                if (distanceToExplosion < 2.5f)
                {
                    Debug.Log("Player Exploded");
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                    rb.AddForce(hit.gameObject.transform.up * playerLaunchForce, ForceMode.Impulse);
                }
            }
            else if (hit.name == "Enemy")
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                rb.AddExplosionForce(launchForce, explosionPos, radius);
            }
        }
        Rigidbody rocketRigidBody = gameObject.GetComponent<Rigidbody>();

        // Now stop the rocket's physics interactions
        //rocketRigidBody.linearVelocity = Vector3.zero;
        //rocketRigidBody.angularVelocity = Vector3.zero;

        Destroy(gameObject); // Delay destruction so gizmo is visible for a short time
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f); // Red with transparency
        Gizmos.DrawSphere(explosionPos, radius);
    }
}
