using UnityEngine;
using UnityEngine.AI;

public class AnimalAvoidance : MonoBehaviour
{
    NavMeshAgent agent;
    AnimalBehaviour behaviour;
    Animal animal;

    [Header("Settings")]
    public float avoidanceRange;
    public float checkInterval = 0.1f;
    float cooldown = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        behaviour = GetComponent<AnimalBehaviour>();
        animal = GetComponent<Animal>();
        avoidanceRange = animal.baseSight * 0.8f;
    }

    void Update()
    {

        if (behaviour == null || animal == null || agent == null || behaviour.isDead) return;

        switch (behaviour.CurrentState)
        {
         case AnimalBehaviour.State.Idle:
         case AnimalBehaviour.State.Wander:
         case AnimalBehaviour.State.SearchMate:
         case AnimalBehaviour.State.SearchFood:
         case AnimalBehaviour.State.SearchWater:
            break;
         default:
            return;  
        }

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;
        cooldown = checkInterval;

        Vector3 avoidDirection = Vector3.zero;
        float totalThreatLevel = 0f;

        Collider[] nearby = Physics.OverlapSphere(transform.position, avoidanceRange);

        foreach (Collider col in nearby)
        {

            AnimalBehaviour otherBehaviour = col.GetComponentInParent<AnimalBehaviour>();
            if (otherBehaviour == null || otherBehaviour.isDead) continue;

            Animal otherAnimal = col.GetComponentInParent<Animal>();
            if (otherAnimal == null) continue;

            float distance = Vector3.Distance(transform.position, col.transform.position);

            float threatLevel = AssesThreat(otherAnimal, otherBehaviour, distance);
            if (threatLevel <= 0f) continue;

            if (threatLevel > 0f)
            {
                Vector3 away = (transform.position - col.transform.position).normalized;
                float urgency = 1f - (distance / avoidanceRange);
                avoidDirection += away * urgency * threatLevel;
                totalThreatLevel += threatLevel;
            }
        }

        if (totalThreatLevel > 0 && agent.isOnNavMesh)
        {
            Vector3 avoidTarget = transform.position + avoidDirection.normalized * 15f;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(avoidTarget, out navHit, 15f, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
            }
        }
    }
    public float AssesThreat(Animal otherAnimal, AnimalBehaviour otherBehaviour, float distance)
    {
        if (otherAnimal.species == animal.species)
        {
            return 0f;
        }
        if (animal.species == Species.moose)
        {
            return AssesMooseThreat(otherAnimal, otherBehaviour, distance);
        }
        if (animal.species == Species.bear)
        {
            return AssesBearThreat(otherAnimal, otherBehaviour, distance);
        }
        if (animal.species == Species.wolf)
        {
            return AssesWolfThreat(otherAnimal, otherBehaviour, distance);
        }
        return 0f;
    }

    public float AssesMooseThreat(Animal otherAnimal, AnimalBehaviour otherBehaviour, float distance)
    {
        bool isAdult = animal.age > animal.grownUpAge;
        
        if (otherAnimal.species == Species.bear)
        {
            return 1f; 
        }
        if (otherAnimal.species == Species.wolf)
        {
            if (!isAdult)
            {
                return 1f; 
            }
            else
            {
                Wolf wolf = otherAnimal as Wolf;
                return 0.15f*wolf.pack.countCurrentPackSize(); 
            }
        }
        return 0f;
    }

    public float AssesBearThreat(Animal otherAnimal, AnimalBehaviour otherBehaviour, float distance)
    {
        if (otherAnimal.species == Species.wolf)
        {
            Wolf wolf = otherAnimal as Wolf;
            if (wolf.pack != null && wolf.pack.countCurrentPackSize() >= 6)
            {
                return 0.1f*wolf.pack.countCurrentPackSize();
            }
        }
        return 0f;
    }

    public float AssesWolfThreat(Animal otherAnimal, AnimalBehaviour otherBehaviour, float distance)
    {
        if (otherAnimal.species == Species.bear)
        {
            Wolf wolf = animal as Wolf;
            if (wolf.pack != null && wolf.pack.countCurrentPackSize() < 6)
            {
                return 1f / wolf.pack.countCurrentPackSize();
            }
        }
        return 0f;
    }
    
}