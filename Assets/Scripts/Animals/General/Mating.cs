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

    public float matingRange = 1f;
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
    public float gestationDuration = 30f;
    public float pregnancyHungerDrainPerSecond = 0.35f;
    public float pregnancyThirstDrainPerSecond = 0.45f;
    public float pregnancyStaminaDrainPerSecond = 0.25f;

    private float pregnancyTimer = 0f;
    private float pendingBabySize;
    private float pendingBabySpeed;
    private float pendingBabySight;
    private float pendingBabyHearing;

    private bool hasSpawnedCurrentBaby = false;

    [Header("Failed Mating")]
    public float matingRejectionCooldown = 30f;
    private System.Collections.Generic.List<GameObject> rejectedMates = new System.Collections.Generic.List<GameObject>();

    public static event Action<string> OnMating;

    private void Start()
    {
        animal = GetComponent<Animal>();
        needs = GetComponent<AnimalNeeds>();
        behaviour = GetComponent<AnimalBehaviour>();

        pregnancyTimer = 0f;
        behaviour.SetPregnant(false);
        gestationDuration = GetGestationDurationForSpecies(animal.species);

        cooldownTimer = matingCooldown;
    }


    private void Update()
    {
        if (animal == null || needs == null)
            return;

        if (behaviour != null && behaviour.isDead)
            return;

        UpdatePregnancy();

        if (behaviour.isPregnant)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f)
            return;

        if (animal.age < animal.grownUpAge)
            return;

        if (!HasEnoughNeeds(needs))
            return;

        if (SeasonManager.Instance.IsWinter && (animal.species == Species.bear || animal.species == Species.moose))
            return;

    }

    public float GetPregnancyTimer()
    {
        return pregnancyTimer;
    }

    public float GetGestationDuration()
    {
        return gestationDuration;
    }

    private static float GetGestationDurationForSpecies(Species species)
    {
        // Uses species-specific gestation lengths here.
        // The values are in game seconds; adjust the scale if needed.
        return species switch
        {
            Species.bear => 80f,  // ~8 months in game time
            Species.moose => 80f, // ~8 months in game time
            Species.wolf => 20f,   // ~2 months in game time
            _ => 20f,
        };
    }

    private float TryFindMate()
    {
        return gestationDuration;
    }
    
    public float GetCooldownTimer()
    {
        return cooldownTimer;
    }

    public bool IsRejectedMate(GameObject potential)
    {
        return potential != null && rejectedMates.Contains(potential);
    }

    private void RejectMate(GameObject mate)
    {
        if (mate != null && !rejectedMates.Contains(mate))
        {
            rejectedMates.Add(mate);
            StartCoroutine(RemoveRejectedMateAfterDelay(mate));
        }
    }

    private System.Collections.IEnumerator RemoveRejectedMateAfterDelay(GameObject mate)
    {
        yield return new WaitForSeconds(matingRejectionCooldown);
        rejectedMates.Remove(mate);
    }

    public void TryMate(GameObject partner)
    {
        if (partner == null) return;
        MateWith(partner);
    }
    
    public bool HasEnoughNeeds(AnimalNeeds targetNeeds)
    {
        float staminaPercent = targetNeeds.maxStamina <= 0f
            ? 0f
            : targetNeeds.staminaLevel / targetNeeds.maxStamina;

        return targetNeeds.howHungryInPercent >= minHungerPercentToMate
            && targetNeeds.howThirstyInPercent >= minThirstPercentToMate
            && staminaPercent >= minStaminaPercentToMate
            && !targetNeeds.isDead;
    }

    private void MateWith(GameObject partner)
    {
        if (animalPrefab == null || heartPrefab == null)
        {
            RejectMate(partner);
            return;
        }
        
        if (animal.age < animal.grownUpAge)
        {
            RejectMate(partner);
            return;
        }

        Animal partnerAnimal = partner.GetComponent<Animal>();
        Mating partnerMating = partner.GetComponent<Mating>();
        if (partnerAnimal == null || partnerMating == null)
        {
            RejectMate(partner);
            return;
        }

        // Guard against multiple males mating with the same female in the same frame
        if (cooldownTimer > 0f || partnerMating.cooldownTimer > 0f)
            return;
        
        if (partnerAnimal.age < partnerAnimal.grownUpAge)
        {
            RejectMate(partner);
            partnerMating.RejectMate(gameObject);
            return;
        }

        Mating motherMating = animal.IsMale ? partnerMating : this;
        Animal fatherAnimal = animal.IsMale ? animal : partnerAnimal;
        if (motherMating.usePregnancySystem)
        {
            if (!motherMating.TryStartPregnancy(fatherAnimal))
            {
                RejectMate(partner);
                partnerMating.RejectMate(gameObject);
                return;
            }
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

        if (animal.IsMale || behaviour.isPregnant)
            return false;
        if (cooldownTimer > 0f)
            return false;
        if (animal.age < animal.grownUpAge)
        return false;

        behaviour.SetPregnant(true);
        cooldownTimer = matingCooldown;
        pregnancyTimer = gestationDuration;

        hasSpawnedCurrentBaby = false;

        pendingBabySize = (animal.size + fatherAnimal.size) / 2f;
        pendingBabySpeed = (animal.speed + fatherAnimal.speed) / 2f;
        pendingBabySight = (animal.sightRange + fatherAnimal.sightRange) / 2f;
        pendingBabyHearing = (animal.hearingRange + fatherAnimal.hearingRange) / 2f;

        return true;
    }

    private void UpdatePregnancy()
    {
        if (!usePregnancySystem || !behaviour.isPregnant)
            return;

        pregnancyTimer -= Time.deltaTime;

        needs.Eat(-pregnancyHungerDrainPerSecond * Time.deltaTime);
        needs.drinkFromSource(-pregnancyThirstDrainPerSecond * Time.deltaTime);
        needs.staminaLevel = Mathf.Clamp(
            needs.staminaLevel - pregnancyStaminaDrainPerSecond * Time.deltaTime,
            0f,
            needs.maxStamina
        );

        if (pregnancyTimer <= 0f && behaviour.isPregnant && !hasSpawnedCurrentBaby)  // Extra safety check
        {
            hasSpawnedCurrentBaby = true;
            SpawnPregnancyBaby();
        
            behaviour.SetPregnant(false);
            pregnancyTimer = -2f;
            
            cooldownTimer = matingCooldown;
            return;
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
        {
            Destroy(baby);
            return;
        }

        babyAnimal.age = 0f;
        babyAnimal.size = pendingBabySize;
        babyAnimal.speed = pendingBabySpeed;
        babyAnimal.sightRange = pendingBabySight;
        babyAnimal.hearingRange = pendingBabyHearing;

        Mating babyMating = baby.GetComponent<Mating>();
        if(babyMating != null)
        {
            babyMating.pregnancyTimer = 0f;
            babyMating.cooldownTimer = babyMating.matingCooldown;
        }

        AnimalBehaviour babyBehaviour = baby.GetComponent<AnimalBehaviour>();
        if (babyBehaviour != null)
        {
            babyBehaviour.SetPregnant(false);
            babyBehaviour.StartWandering();
        }

        if (StatisticsTableManager.instance != null)
        {
            if (animal.species == Species.bear) StatisticsTableManager.instance.BearBirthCount++;
            else if (animal.species == Species.wolf) StatisticsTableManager.instance.WolfBirthCount++;
            else if (animal.species == Species.moose) StatisticsTableManager.instance.MooseBirthCount++;
        }

        SetupCub(baby);
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

        AnimalBehaviour babyBehaviour = baby.GetComponent<AnimalBehaviour>();
        if (babyBehaviour != null)
            babyBehaviour.StartWandering();

        if (StatisticsTableManager.instance != null)
        {
            if (animal.species == Species.bear) StatisticsTableManager.instance.BearBirthCount++;
            else if (animal.species == Species.wolf) StatisticsTableManager.instance.WolfBirthCount++;
            else if (animal.species == Species.moose) StatisticsTableManager.instance.MooseBirthCount++;
        }
        SetupCub(baby);
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

    private void SetupCub(GameObject baby)
{
    AnimalBehaviour adultBehaviour = baby.GetComponent<AnimalBehaviour>();
    if (adultBehaviour != null)
        adultBehaviour.enabled = false;

    AnimalBehaviour motherBehaviour = animal.IsMale
        ? null
        : GetComponent<AnimalBehaviour>();

    CubBehaviour cub = null;
    switch (animal.species)
    {
        case Species.bear:
            cub = baby.AddComponent<BearCubBehaviour>();
            break;
        case Species.wolf:
            cub = baby.AddComponent<WolfCubBehaviour>();
            break;
        case Species.moose:
            cub = baby.AddComponent<MooseCalfBehaviour>();
            break;
    }

    if (cub != null && motherBehaviour != null)
        cub.mother = motherBehaviour;
}



}
