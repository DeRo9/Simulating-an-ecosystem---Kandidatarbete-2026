using UnityEngine;
using UnityEngine.InputSystem.XR;

public class AnimalFOV : MonoBehaviour
{
    [Header("FOV Settings")]
    [SerializeField]
    private float viewRange;

    [SerializeField]
    [Range(0, 360)]
    private float viewAngle;

    public LayerMask viewLayer;
    public Transform animalprefab;

    void OnDrawGizmosSelected()
    {
        // Draw the view range (Sphere)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewRange);

        // Draw the view angle (Angle of degree infront)
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(animalprefab.position, animalprefab.position + leftBoundary * viewRange);
        Gizmos.DrawLine(animalprefab.position, animalprefab.position + rightBoundary * viewRange);
    }
}
