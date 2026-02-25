using UnityEngine;

public class Mating : MonoBehaviour
{
    public float matingRange = 5f;
    public float matingCooldown = 30f;
    public float currentAge = 0f;

    public GameObject moosePrefab;

    private Moose moose;
    private float cooldownTimer = 0f;

    private void Start()
    {
        moose = GetComponent<Moose>();
    }

    private void Update()
    {
        currentAge = moose.age;
        
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f)
            return;

        if (moose.age < 10f)
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

            Moose other = col.GetComponent<Moose>();
            if (other == null)
                continue;

            if (other.age < 10f)
                continue;

            if (other.IsMale == moose.IsMale)
                continue;

            MateWith(col.gameObject);
            break;
        }
    }

    private void MateWith(GameObject partner)
    {
        Vector3 spawnPosition = (transform.position + partner.transform.position) / 2f;

        GameObject baby = Instantiate(moosePrefab, spawnPosition, Quaternion.identity);

        Moose babyAge = baby.GetComponent<Moose>();
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
