using UnityEngine;

public class Animal : MonoBehaviour
{
    [Header("Biology")]
    public float age = 0f;
    public float agingSpeed = 0.2f;
    public bool IsMale;

    protected virtual void Start()
    {
        IsMale = Random.value > 0.5f;
    }

    protected virtual void Update()
    {
        age += Time.deltaTime * agingSpeed;
    }
}
