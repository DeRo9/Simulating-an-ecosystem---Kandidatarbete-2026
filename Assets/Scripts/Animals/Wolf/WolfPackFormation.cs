using UnityEngine;
using System.Collections.Generic;

public class WolfPackFormation : MonoBehaviour
{
    private Wolf wolf;
    private AnimalBehaviour behaviour;
    private WolfPackManager pack;
    private AnimalNeeds needs;

    float formationCooldown = 1f;
    float formationTimer = 0f;

    [SerializeField] private float detectionRadius = 10f;

    private void Start()
    {
        wolf = GetComponent<Wolf>();
        behaviour = GetComponent<AnimalBehaviour>();
        needs = GetComponent<AnimalNeeds>();
    }

    private void Update()
    {
        if (needs.hungerLevel < 0.2f && !SeasonManager.Instance.IsWinter)
        {
            LeavePack();
        }
        formationTimer -= Time.deltaTime;
        if (formationTimer > 0f) return;
        if ((behaviour.CurrentState == AnimalBehaviour.State.Idle || behaviour.CurrentState == AnimalBehaviour.State.Wander) &&
            (wolf.pack == null || wolf.pack.leader != wolf))
        {
            CheckForNearbyWolves();
        }
    }

    private void CheckForNearbyWolves()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        List<Wolf> nearbyWolves = new List<Wolf>();

        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Wolf")) // Not self and is wolf
            {
                Wolf otherWolf = hit.GetComponent<Wolf>();
                if (otherWolf != null)
                {
                    AnimalBehaviour otherBehaviour = otherWolf.GetComponent<AnimalBehaviour>();
                    if (otherBehaviour != null &&
                        (otherBehaviour.CurrentState == AnimalBehaviour.State.Idle || otherBehaviour.CurrentState == AnimalBehaviour.State.Wander))
                    {
                        nearbyWolves.Add(otherWolf);
                    }
                }
            }
        }

        foreach (Wolf nearbyWolf in nearbyWolves)
        {
            FormOrJoinPack(nearbyWolf);
        }
    }

    private void FormOrJoinPack(Wolf otherWolf)
    {

        if (wolf.pack != null) return;
        
        if (otherWolf.pack != null)
        {
            JoinPack(otherWolf.pack);
            formationTimer = formationCooldown;
            return;
        }
        
        if (wolf.GetInstanceID() < otherWolf.GetInstanceID())
        {
            CreatePackAndAddMember(otherWolf);
            formationTimer = formationCooldown;
        }
        
    }

    private void CreatePackAndAddMember(Wolf memberWolf)
    {
        if (wolf.pack != null || memberWolf.pack != null)
        return;

        GameObject packObj = new GameObject("WolfPack");
        WolfPackManager newPack = packObj.AddComponent<WolfPackManager>();

        newPack.leader = wolf;
        newPack.members.Add(wolf);
        newPack.members.Add(memberWolf);

        wolf.pack = newPack;
        memberWolf.pack = newPack;

        wolf.isLeader = true;
        memberWolf.isLeader = false;

        Debug.Log("New pack formed with leader: " + wolf.name + " and member: " + memberWolf.name);
    }

    private void JoinPack(WolfPackManager existingPack)
    {
        if (existingPack == null) return;
        if (existingPack.members.Count >= existingPack.GetMaxPackSize()) return;
        

        existingPack.members.Add(wolf);
        wolf.pack = existingPack;
        wolf.isLeader = false;

        string leaderName = existingPack.leader != null ? existingPack.leader.name : "Unknown";
        Debug.Log(wolf.name + " joined pack led by " + leaderName);
    }

    private void LeavePack() 
    {
        if (wolf.pack == null) return;

        if (wolf.isLeader)
        {
            return;
        }

        wolf.pack.members.Remove(wolf);
        wolf.pack = null;
        wolf.isLeader = false;

        Debug.Log(wolf.name + " left the pack.");
    }
}