using UnityEngine;

public class FoodTree : MonoBehaviour, IsEdible
{
    [SerializeField]
    public float nutritionValue = 100f;
    [SerializeField]
    private Species[] allowedSpecies = {Species.moose};
    private MushroomSpawner spawner;

    [SerializeField]
    private float regrowCooldown = 60f;

    private float cooldownTimer = 0f;
    private Renderer rend;
    private Collider col;


    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                //if (rend != null) rend.enabled = true; testing
                if (col != null) col.enabled = true;
            }
        }
    }
    public float Consume()
    {
        if (cooldownTimer > 0f){
            return 0f;
        } 
        cooldownTimer = regrowCooldown;

        //if (rend != null) rend.enabled = false; here for testing
        if(col != null) col.enabled = false;
        return nutritionValue;
    }

    public bool CanBeEatenBy(Species species)
    {
        if (cooldownTimer > 0f ) return false;
        foreach (Species allowed in allowedSpecies)
        {
            if (allowed == species)
            {
                return true;
            }
        }
        return false;
    }
}
