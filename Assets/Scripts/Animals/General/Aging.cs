using UnityEngine;

public class Aging : MonoBehaviour
{
    private Animal animal; 

    private float grownUp;
    private float old;

    [Header("Scale Settings")]
    public float childScale = 0.5f;
    public float adultScale = 1f;

    private Vector3 originalScale;
    private float age;

    void Start()
    {
        animal = GetComponent<Animal>();
        grownUp = animal.grownUpAge;

        float baseScale = childScale * animal.size;
        originalScale = Vector3.one * baseScale;
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
            transform.localScale = Vector3.one * currentScale * animal.size;
        }
    }
}
