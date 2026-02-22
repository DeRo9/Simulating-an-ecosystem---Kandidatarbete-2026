using UnityEngine;

public class Aging : MonoBehaviour
{
    [Header("Age Settings")]
    public float age = 0f;
    public float maxGrowthAge = 10f;
    public float oldAgeStart = 20f;
    public float agingSpeed = 1f;

    [Header("Scale Settings")]
    public float childScale = 0.5f;
    public float adultScale = 1f;

    private Vector3 originalScale;


    void Start()
    {
        originalScale = Vector3.one * childScale;
        transform.localScale = originalScale;
    }

    void Update()
    {
        AgeOverTime();
        UpdateGrowth();
        //UpdateOldAgeVisual();
    }

    void AgeOverTime()
    {
        age += Time.deltaTime * agingSpeed;
    }

    void UpdateGrowth()
    {
        if (age <= maxGrowthAge)
        {
            float growthPercent = age / maxGrowthAge;
            float currentScale = Mathf.Lerp(childScale, adultScale, growthPercent);
            transform.localScale = Vector3.one * currentScale;
        }
    }

    void UpdateOldAgeVisual()
    {
        
    }
}
