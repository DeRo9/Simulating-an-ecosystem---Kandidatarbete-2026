using UnityEngine;
using UnityEngine.AI;

public class CubBehaviour : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;
    Animal animalData;


    public AnimalBehaviour mother;
    float followDistance = 5f;
    float age = 0f;
    bool isAdult = false;

    float wanderRadius = 8f;
    float repathTimer = 0f;
    float repathInterval = 0f;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        animalData = GetComponent<Animal>();

        agent.speed = animalData.speed * 0.6f;
        transform.localScale = Vector3.one * animalData.size * 0.4f;
    }


    protected virtual void Update()
    {
        if (isAdult) {
            return;
        }
        age += Time.deltaTime;

        if(age >= animalData.grownUpAge)
        {
            BecomeAdult();
            return;
        }

        if (mother.isDead || mother == null)
        {
            BecomeAdult();
            return;
        }

        float progress = age / animalData.grownUpAge;
        float currentScale = Mathf.Lerp(animalData.size * 0.4f, animalData.size,progress);
        transform.localScale = Vector3.one * currentScale;

        FollowMother();
        UpdateAnimations();

    }



    protected virtual void FollowMother()
    {
        float distancetoMother = Vector3.Distance (transform.position,mother.transform.position);
        if(mother.CurrentState == AnimalBehaviour.State.Hunt || mother.CurrentState == AnimalBehaviour.State.Fleeing)
        {
            agent.speed = animalData.runningSpeed * 0.8f;
            agent.SetDestination(mother.transform.position);
            return;
        }

        if (distancetoMother > followDistance * 2f)
        {
            agent.speed = animalData.speed * 1.2f;
            agent.SetDestination (mother.transform.position);
            return;
        }

        if (distancetoMother < followDistance)
        {
            repathTimer += Time.deltaTime;
            if (repathTimer >= repathInterval)
            {
                Vector3 randomOffset = Random.insideUnitSphere * wanderRadius;
                randomOffset.y = 0;
                Vector3 wanderTarget = mother.transform.position + randomOffset;

                NavMeshHit hit;

                if(NavMesh.SamplePosition(wanderTarget,out hit, wanderRadius, NavMesh.AllAreas))
                {
                    agent.speed = animalData.speed * 0.6f;
                    agent.SetDestination(hit.position);
                }
                repathTimer = 0f;
            }
        }
        else
        {
            agent.speed = animalData.speed;
            agent.SetDestination(mother.transform.position);
        }
    }
    protected virtual void BecomeAdult()
    {
        isAdult = true;
        transform.localScale = Vector3.one * animalData.size;
        Debug.Log(animalData.species + "cub has grown up");
        Destroy(this);
    }
    void UpdateAnimations()
    {
        if (anim == null) return;
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && agent.velocity.magnitude <= 3.2f);
        anim.SetBool("isRunning", agent.velocity.magnitude > 3.5f);
    }
}