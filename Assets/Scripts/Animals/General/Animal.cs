using UnityEngine;

public class Animal : MonoBehaviour
{
    [Header("Biology")]
    public float age = 0f;
    public float agingSpeed = 0.1f;
    public bool IsMale;


    [Header("Life Stages")]
    public float grownUpAge = 10f;
    public float oldAge = 20f;

    protected virtual void Awake()
    {
        IsMale = Random.value > 0.5f;
    }

    protected virtual void Update()
    {
        age += Time.deltaTime * agingSpeed;
    }
}
