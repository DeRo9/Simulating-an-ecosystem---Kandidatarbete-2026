using UnityEngine;
using UnityEngine.InputSystem.XR;

public class AnimalFOV : MonoBehaviour
{
    private Animal animal;

    [Header("FOV Settings")]

    [SerializeField]
    [Range(0, 360)]
    private float viewAngle;

    public LayerMask viewLayer;

    // Visualize the FOV
    void OnDrawGizmosSelected()
    {
        animal = GetComponent<Animal>();

        // Draw the view range (Sphere)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, animal.sightRange);

        // Draw the view angle (Angle of degree infront)
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * animal.sightRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * animal.sightRange);
    }

    public bool IsInFOV(Transform target)
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f; 

        Vector3 forward  = transform.forward;
        forward.y = 0f;

        float angle = Vector3.Angle(forward, directionToTarget.normalized);
        return angle < viewAngle / 2;
    }

}
