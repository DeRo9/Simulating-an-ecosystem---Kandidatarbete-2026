using UnityEngine;
using UnityEngine.AI;

public class CubBehaviour : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;
    Animal animalData;
    AnimalNeeds cubNeeds;
    Mating cubMating;

    public AnimalBehaviour mother;
    float followDistance = 5f;
    float age = 0f;
    bool isAdult = false;

    float wanderRadius = 8f;
    float repathTimer = 0f;
    float repathInterval = 1f;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        animalData = GetComponent<Animal>();
        cubNeeds = GetComponent<AnimalNeeds>();
        cubMating = GetComponent<Mating>();

        // Disable needs and mating so cub doesn't starve or reproduce, maybe it could be replaced with them eating as well, will consider that option
        if (cubNeeds != null) cubNeeds.enabled = false;
        if (cubMating != null) cubMating.enabled = false;

        if (agent != null && animalData != null)
        {
            agent.speed = animalData.speed * 0.6f;
        }

        /// transform.localScale = Vector3.one * animalData.size * 0.4f; aging handles this already
    }

    protected virtual void Update()
    {
        if (isAdult) return;
        if (animalData == null || agent == null) return;

        age += Time.deltaTime;

        if (age >= animalData.grownUpAge)
        {
            BecomeAdult();
            return;
        }

        if (mother == null || mother.isDead)
        {
            BecomeAdult();
            return;
        }

        // Gradual growth
        float progress = age / animalData.grownUpAge;
        /*float currentScale = Mathf.Lerp(animalData.size * 0.4f, animalData.size, progress);
        transform.localScale = Vector3.one * currentScale;*/

        // Scale speed with growth
        agent.speed = Mathf.Lerp(animalData.speed * 0.6f, animalData.speed, progress);

        FollowMother();
        UpdateAnimations();
    }

    void FollowMother()
    {
        if (!agent.isOnNavMesh) return;

        float distanceToMother = Vector3.Distance(transform.position, mother.transform.position);

        // Mother is hibernating,she should not be, but in case it happens
        if (mother.CurrentState == AnimalBehaviour.State.Hibernate)
        {
            if (distanceToMother > followDistance)
            {
                agent.isStopped = false;
                agent.speed = animalData.speed * 0.6f;
                agent.SetDestination(mother.transform.position);
            }
            else
            {
                agent.isStopped = true;
            }
            return;
        }

        // Mother is mating, the cub could wait or it could grow up, two options we have
        if (mother.CurrentState == AnimalBehaviour.State.Mating ||
            mother.CurrentState == AnimalBehaviour.State.SearchMate)
        {
            if (distanceToMother > followDistance)
            {
                agent.isStopped = false;
                agent.SetDestination(mother.transform.position);
            }
            else
            {
                agent.isStopped = true;
            }
            return;
        }

        agent.isStopped = false;

        // Mother is in danger, follow her actions
        if (mother.CurrentState == AnimalBehaviour.State.Hunt ||
            mother.CurrentState == AnimalBehaviour.State.Fleeing ||
            mother.CurrentState == AnimalBehaviour.State.Defend)
        {
            agent.speed = animalData.runningSpeed * 0.8f;
            agent.SetDestination(mother.transform.position);
            return;
        }

        // Too far,run back to mother
        if (distanceToMother > followDistance * 2)
        {
            agent.speed = animalData.speed * 1.2f;
            agent.SetDestination(mother.transform.position);
            return;
        }

        // Close enough — wander nearby
        if (distanceToMother < followDistance)
        {
            repathTimer += Time.deltaTime;
            if (repathTimer >= repathInterval)
            {
                Vector3 randomOffset = Random.insideUnitSphere * wanderRadius;
                randomOffset.y = 0;
                Vector3 wanderTarget = mother.transform.position + randomOffset;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(wanderTarget, out hit, wanderRadius, NavMesh.AllAreas))
                {
                    agent.speed = animalData.speed * 0.6f;
                    agent.SetDestination(hit.position);
                }
                repathTimer = 0f;
            }
        }
        else
        {
            // Walk toward mother
            agent.speed = animalData.speed;
            agent.SetDestination(mother.transform.position);
        }
    }

    protected virtual void BecomeAdult()
    {
        isAdult = true;
        //transform.localScale = Vector3.one * animalData.size;

        // Re-enable needs
        if (cubNeeds != null) cubNeeds.enabled = true;
        if (cubMating != null)
        {
            cubMating.enabled = true;
        }

        Debug.Log(animalData.species + " cub has grown up!");
        Destroy(this);
    }

    void UpdateAnimations()
    {
        if (anim == null || animalData == null) return;
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < animalData.runningSpeed * 0.95f);
        anim.SetBool("isRunning", agent.velocity.magnitude > animalData.runningSpeed * 0.95f);
    }
}