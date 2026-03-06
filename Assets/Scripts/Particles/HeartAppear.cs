using UnityEngine;

public class HeartAppear : MonoBehaviour
{
    public float floatSpeed = 1.5f;
    public float lifetime = 2f;

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        lifetime -= Time.deltaTime;

        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
