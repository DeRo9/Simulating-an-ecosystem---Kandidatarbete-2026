using UnityEngine;
using UnityEngine.AI;

public class Area : MonoBehaviour
{
    // This script defines an area in the simulation world where random points can be generated.
    public float Radius = 20f;

    // Draw a wire sphere in the editor to visualize the area of effect.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }

    // Generates random point on the navmesh
    public Vector3 GetRandomPoints()
    {
        Vector3 randomDirection = Random.insideUnitSphere * Radius;
        randomDirection.y = 0f;
        Vector3 randomPoint = transform.position + randomDirection;

        NavMeshHit navMeshHit;
        Vector3 finalPosition = transform.position;

        if (NavMesh.SamplePosition(randomPoint, out navMeshHit, Radius, NavMesh.AllAreas))
        {
            finalPosition = navMeshHit.position;
        }

        return finalPosition;
    }
}
