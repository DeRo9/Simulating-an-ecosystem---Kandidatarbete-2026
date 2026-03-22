using System;
using UnityEngine;

public class Mating : MonoBehaviour
{
    public Animal animal;
    private AnimalNeeds needs;
    public GameObject animalPrefab;
    public GameObject heartPrefab;
    public float heartHeight = 2f;

    public float matingRange = 5f;
    public float matingCooldown = 30f;
    private float cooldownTimer = 0f;

    [Header("Mating Requirements (0-1)")]
    [Range(0f, 1f)] public float minHungerPercentToMate = 0.20f;
    [Range(0f, 1f)] public float minThirstPercentToMate = 0.20f;
    [Range(0f, 1f)] public float minStaminaPercentToMate = 0.20f;

    [Header("Reproduction Costs")]
    public float hungerCostOnMating = 10f;
    public float thirstCostOnMating = 8f;
    public float staminaCostOnMating = 15f;

    public static event Action OnMating;

    private void Start()
    {
        animal = GetComponent<Animal>();
        needs = GetComponent<AnimalNeeds>();
    }


    private void Update()
    {
        if (animal == null || needs == null)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f)
            return;

        if (animal.age < animal.grownUpAge)
            return;

        if (!HasEnoughNeeds(needs))
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

            AnimalNeeds otherNeeds = col.GetComponent<AnimalNeeds>();
            if (otherNeeds == null)
                continue;

            if (other.species != animal.species)
                continue;

            if (other.age < other.grownUpAge)
                continue;

            if (other.IsMale == animal.IsMale)
                continue;

            if (!HasEnoughNeeds(otherNeeds))
                continue;

            if (gameObject.GetInstanceID() > col.gameObject.GetInstanceID())
            continue;

            MateWith(col.gameObject);
            break;
        }
    }

    private void MateWith(GameObject partner)
    {
        if (animalPrefab == null || heartPrefab == null)
            return;

        Vector3 spawnPosition = (transform.position + partner.transform.position) / 2f;

        GameObject baby = Instantiate(animalPrefab, spawnPosition, Quaternion.identity, transform.parent);

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
            partnerMating.cooldownTimer = partnerMating.matingCooldown;

            AnimalNeeds partnerNeeds = partner.GetComponent<AnimalNeeds>();
            if (partnerNeeds != null)
            {
                ApplyReproductionCosts(partnerNeeds);
            }
        }

        ApplyReproductionCosts(needs);

        OnMating?.Invoke();

    }

    private bool HasEnoughNeeds(AnimalNeeds targetNeeds)
    {
        float staminaPercent = targetNeeds.maxStamina <= 0f
            ? 0f
            : targetNeeds.staminaLevel / targetNeeds.maxStamina;

        return targetNeeds.howHungryInPercent >= minHungerPercentToMate
            && targetNeeds.howThirstyInPercent >= minThirstPercentToMate
            && staminaPercent >= minStaminaPercentToMate
            && !targetNeeds.isDead;
    }

    private void ApplyReproductionCosts(AnimalNeeds targetNeeds)
    {
        targetNeeds.Eat(-hungerCostOnMating);
        targetNeeds.drinkFromSource(-thirstCostOnMating);
        targetNeeds.staminaLevel = Mathf.Clamp(
            targetNeeds.staminaLevel - staminaCostOnMating,
            0f,
            targetNeeds.maxStamina
        );
    }


}
