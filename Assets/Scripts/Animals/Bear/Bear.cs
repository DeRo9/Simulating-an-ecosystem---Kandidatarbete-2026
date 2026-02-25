using UnityEngine;

public class Bear : MonoBehaviour
{
    public float age = 0f;
    public float agingSpeed = 0.2f;
    public bool IsMale;

    private void Start()
    {
        IsMale = Random.value > 0.5f;
    }

    private void Update()
    {
        age += Time.deltaTime * agingSpeed;
    }
}
