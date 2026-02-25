using UnityEngine;

public class Mating : MonoBehaviour
{
    public float matingRange = 5f;
    public float matingCooldown = 30f;
    public float currentAge = 0f;

    public GameObject animalPrefab;

    private Animal animal;
    private float cooldownTimer = 0f;

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

        if (animal.age < 10f)
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

            if (other.age < 10f)
                continue;

            if (other.IsMale == animal.IsMale)
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

        cooldownTimer = matingCooldown;

        Mating partnerMating = partner.GetComponent<Mating>();
        if (partnerMating != null)
        {
            partnerMating.cooldownTimer = matingCooldown;
        }
    }
}
