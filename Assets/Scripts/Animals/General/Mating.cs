using System;
using UnityEngine;

public class Mating : MonoBehaviour
{
    public Animal animal;
    private AnimalNeeds needs;
    private AnimalBehaviour behaviour;
    public GameObject animalPrefab;
    public GameObject heartPrefab;
    public float heartHeight = 2f;

    public float matingRange = 9f;
    public float matingCooldown = 18f;
    private float cooldownTimer = 0f;

    [Header("Mating Requirements (0-1)")]
    [Range(0f, 1f)] public float minHungerPercentToMate = 0.10f;
    [Range(0f, 1f)] public float minThirstPercentToMate = 0.10f;
    [Range(0f, 1f)] public float minStaminaPercentToMate = 0.10f;

    [Header("Reproduction Costs")]
    public float hungerCostOnMating = 6f;
    public float thirstCostOnMating = 5f;
    public float staminaCostOnMating = 10f;

    [Header("Pregnancy")]
    public bool usePregnancySystem = true;
    public float gestationDuration = 50f;
    public float pregnancyHungerDrainPerSecond = 0.35f;
    public float pregnancyThirstDrainPerSecond = 0.45f;
    public float pregnancyStaminaDrainPerSecond = 0.25f;

    private bool isPregnant = false;
    private float pregnancyTimer = 0f;
    private float pendingBabySize;
    private float pendingBabySpeed;
    private float pendingBabySight;
    private float pendingBabyHearing;

    public static event Action<string> OnMating;

    private void Start()
    {
        animal = GetComponent<Animal>();
        needs = GetComponent<AnimalNeeds>();
        behaviour = GetComponent<AnimalBehaviour>();
    }


    private void Update()
    {
        if (animal == null || needs == null)
            return;

        UpdatePregnancy();

        if (isPregnant)
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

            Mating otherMating = col.GetComponent<Mating>();
            if (otherMating == null)
                continue;

            if (otherMating.cooldownTimer > 0f)
                continue;

            if (other.species != animal.species)
                continue;

            if (other.age < other.grownUpAge)
                continue;

            if (other.IsMale == animal.IsMale)
                continue;

            if (!animal.IsMale && isPregnant)
                continue;

            if (!other.IsMale && otherMating.isPregnant)
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

        Animal partnerAnimal = partner.GetComponent<Animal>();
        Mating partnerMating = partner.GetComponent<Mating>();
        if (partnerAnimal == null || partnerMating == null)
            return;

        // Female carries the pregnancy.
        Mating motherMating = animal.IsMale ? partnerMating : this;
        Animal fatherAnimal = animal.IsMale ? animal : partnerAnimal;

        if (usePregnancySystem)
        {
            if (!motherMating.TryStartPregnancy(fatherAnimal))
                return;
        }
        else
        {
            SpawnBabyNow(partnerAnimal);
        }

        Instantiate(heartPrefab, transform.position + Vector3.up * heartHeight, Quaternion.identity);
        Instantiate(heartPrefab, partner.transform.position + Vector3.up * heartHeight, Quaternion.identity);

        cooldownTimer = matingCooldown;

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

        OnMating?.Invoke(animal.species.ToString());

    }

    private bool TryStartPregnancy(Animal fatherAnimal)
    {
        if (animal == null || fatherAnimal == null)
            return false;

        if (animal.IsMale || isPregnant)
            return false;

        isPregnant = true;
        pregnancyTimer = gestationDuration;

        pendingBabySize = (animal.size + fatherAnimal.size) / 2f;
        pendingBabySpeed = (animal.speed + fatherAnimal.speed) / 2f;
        pendingBabySight = (animal.sightRange + fatherAnimal.sightRange) / 2f;
        pendingBabyHearing = (animal.hearingRange + fatherAnimal.hearingRange) / 2f;

        if (behaviour != null)
            behaviour.EnterPregnantState();

        return true;
    }

    private void UpdatePregnancy()
    {
        if (!usePregnancySystem || !isPregnant)
            return;

        pregnancyTimer -= Time.deltaTime;

        needs.Eat(-pregnancyHungerDrainPerSecond * Time.deltaTime);
        needs.drinkFromSource(-pregnancyThirstDrainPerSecond * Time.deltaTime);
        needs.staminaLevel = Mathf.Clamp(
            needs.staminaLevel - pregnancyStaminaDrainPerSecond * Time.deltaTime,
            0f,
            needs.maxStamina
        );

        if (pregnancyTimer <= 0f)
        {
            SpawnPregnancyBaby();
            isPregnant = false;
            if (behaviour != null)
                behaviour.ExitPregnantState();
        }
    }

    private void SpawnPregnancyBaby()
    {
        if (animalPrefab == null)
            return;

        Vector3 spawnPosition = transform.position + transform.forward;
        GameObject baby = Instantiate(animalPrefab, spawnPosition, Quaternion.identity, transform.parent);
        Animal babyAnimal = baby.GetComponent<Animal>();
        if (babyAnimal == null)
            return;

        babyAnimal.age = 0f;
        babyAnimal.size = pendingBabySize;
        babyAnimal.speed = pendingBabySpeed;
        babyAnimal.sightRange = pendingBabySight;
        babyAnimal.hearingRange = pendingBabyHearing; 
    }

    private void SpawnBabyNow(Animal partnerAnimal)
    {
        Vector3 spawnPosition = (transform.position + partnerAnimal.transform.position) / 2f;
        GameObject baby = Instantiate(animalPrefab, spawnPosition, Quaternion.identity, transform.parent);
        Animal babyAnimal = baby.GetComponent<Animal>();
        if (babyAnimal == null)
            return;

        babyAnimal.age = 0f;
        babyAnimal.size = (animal.size + partnerAnimal.size) / 2f;
        babyAnimal.speed = (animal.speed + partnerAnimal.speed) / 2f;
        babyAnimal.sightRange = (animal.sightRange + partnerAnimal.sightRange) / 2f;
        babyAnimal.hearingRange = (animal.hearingRange + partnerAnimal.hearingRange) / 2f;
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
