using UnityEngine;

public class Aging : MonoBehaviour
{
    public Animal animal; 

    private float grownUp;
    private float old;

    [Header("Scale Settings")]
    public float childScale = 0.5f;
    public float adultScale = 1f;

    private Vector3 originalScale;
    private float age;

    void Awake() {
        animal = GetComponent<Animal>();
    }

    void Start()
    {
        grownUp = animal.grownUpAge;
        old = animal.oldAge;
        originalScale = Vector3.one * childScale;
        transform.localScale = originalScale;
    }

    void Update()
    {
        age = animal.age;
        UpdateGrowth();
    }

    void UpdateGrowth()
    {
        if (age <= grownUp)
        {
            float growthPercent = age / grownUp;
            float currentScale = Mathf.Lerp(childScale, adultScale, growthPercent);
            transform.localScale = Vector3.one * currentScale;
        }
    }
}
