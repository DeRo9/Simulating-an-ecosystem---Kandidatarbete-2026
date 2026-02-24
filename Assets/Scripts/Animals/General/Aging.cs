using UnityEngine;

public class Aging : MonoBehaviour
{
    private Moose moose; 

    [Header("Age Settings")]
    public float maxGrowthAge = 10f;
    public float oldAgeStart = 20f;
    public float agingSpeed = 1f;

    [Header("Scale Settings")]
    public float childScale = 0.5f;
    public float adultScale = 1f;

    private Vector3 originalScale;
    private float age;


    void Start()
    {
        moose = GetComponent<Moose>();
        originalScale = Vector3.one * childScale;
        transform.localScale = originalScale;
    }

    void Update()
    {
        age = moose.age;
        UpdateGrowth();
        //UpdateOldAgeVisual();
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
