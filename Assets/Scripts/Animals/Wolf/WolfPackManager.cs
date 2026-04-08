using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WolfPackManager : MonoBehaviour
{
    public Wolf leader;
    public List<Wolf> members = new List<Wolf>();
    public int DebugCurrentPackSize;

    private void Update()
    {
        DebugCurrentPackSize = countCurrentPackSize();
    }

    public int GetMaxPackSize()
    {
        if (SeasonManager.Instance.IsWinter)
        {
            return 10; 
        }
        else
        {
            return 6; 
        }
    }

    public Vector3 GetPackDirection()
    {
        return leader.transform.forward;
    }

    public int countCurrentPackSize()
    {
       return members.Count;
    }

    public void OnLeaderDeath()
    {
        members.Remove(leader);
        leader = null;

        if (members.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        foreach(Wolf member in members)
        {
            if(members != null && !member.GetComponent<AnimalBehaviour>().isDead)
            {
                leader = member;
                leader.isLeader = true;
                Debug.Log("New leader: " + leader.name);
                break;
            }
        }

        if (leader == null)
        {
            foreach(Wolf member in members)
            {
                if(member != null)
                {
                    member.pack = null;
                    member.isLeader = false;
                }
            }
            Destroy(gameObject);
        }
    }
}