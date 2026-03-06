using UnityEngine;

public class Mating : MonoBehaviour
{
    public Animal animal;
    public GameObject animalPrefab;
    public GameObject heartPrefab;
    public float heartHeight = 2f;

    public float matingRange = 5f;
    public float matingCooldown = 30f;
    private float cooldownTimer = 0f;
    private float currentAge;

    private void Start()
    {
        animal = GetComponent<Animal>();
    }

    private void Update()
    {
        currentAge = animal.age;
        
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f)
            return;

        if (animal.age < animal.grownUpAge)
            return;

        TryFindMate();
    }

    private void TryFindMate()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, matingRange);

        foreach (Collider col in nearby)
        {

            if (col.gameObject == gameObject)
                continue;

            Animal other = col.GetComponent<Animal>();
            if (other == null)
                continue;

            if (other.species != animal.species)
                continue;

            if (other.age < animal.grownUpAge)
                continue;

            if (other.IsMale == animal.IsMale)
                continue;

            if (gameObject.GetInstanceID() > col.gameObject.GetInstanceID())
            continue;

            MateWith(col.gameObject);
            break;
        }
    }

    private void MateWith(GameObject partner)
    {
        Vector3 spawnPosition = (transform.position + partner.transform.position) / 2f;

        GameObject baby = Instantiate(animalPrefab, spawnPosition, Quaternion.identity);

        Animal babyAge = baby.GetComponent<Animal>();
        if (babyAge != null)
        {
            babyAge.age = 0f;
        }
        Instantiate(heartPrefab, transform.position + Vector3.up * heartHeight, Quaternion.identity);
        Instantiate(heartPrefab, partner.transform.position + Vector3.up * heartHeight, Quaternion.identity);

        cooldownTimer = matingCooldown;

        Mating partnerMating = partner.GetComponent<Mating>();
        if (partnerMating != null)
        {
            partnerMating.cooldownTimer = matingCooldown;
        }
    }
}
