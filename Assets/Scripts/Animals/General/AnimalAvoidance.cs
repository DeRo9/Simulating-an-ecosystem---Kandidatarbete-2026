using UnityEngine;
using UnityEngine.AI;

public class AnimalAvoidance : MonoBehaviour
{
    NavMeshAgent agent;
    AnimalBehaviour behaviour;
    Animal animal;

    [Header("Settings")]
    public float avoidanceRange = 25f;
    public float checkInterval = 2f;
    public string[] tagsToAvoid;

    float cooldown = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        behaviour = GetComponent<AnimalBehaviour>();
        animal = GetComponent<Animal>();
    }

    void Update()
    {
        /*if (behaviour == null || behaviour.isDead) return;
        if (behaviour.CurrentState != AnimalBehaviour.State.Idle &&
            behaviour.CurrentState != AnimalBehaviour.State.Wander) return;*/

        switch (behaviour.CurrentState)
        {
         case AnimalBehaviour.State.Idle:
         case AnimalBehaviour.State.Wander:
         case AnimalBehaviour.State.SearchMate:
         case AnimalBehaviour.State.SearchFood:
         case AnimalBehaviour.State.SearchWater:
            break; // Continue with avoidance
         default:
            return;  
        }
        AnimalNeeds needs = GetComponent<AnimalNeeds>();
         if (needs != null && needs.howHungryInPercent < 0.2f) return;
        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;
        cooldown = checkInterval;

        Vector3 avoidDirection = Vector3.zero;
        int threatCount = 0;

        Collider[] nearby = Physics.OverlapSphere(transform.position, avoidanceRange);

        foreach (Collider col in nearby)
        {
            bool isThreat = false;
            foreach (string tag in tagsToAvoid)
            {
                if (col.CompareTag(tag))
                {
                    isThreat = true;
                    break;
                }
            }

            if (!isThreat) continue;

            AnimalBehaviour other = col.GetComponentInParent<AnimalBehaviour>();
            if (other == null || other.isDead) continue;

            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < avoidanceRange)
            {
                Vector3 away = (transform.position - col.transform.position).normalized;
                float urgency = 1f - (distance / avoidanceRange);
                avoidDirection += away * urgency;
                threatCount++;
            }
        }

        if (threatCount > 0 && agent.isOnNavMesh)
        {
            Vector3 avoidTarget = transform.position + avoidDirection.normalized * 15f;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(avoidTarget, out navHit, 15f, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
            }
        }
    }
}