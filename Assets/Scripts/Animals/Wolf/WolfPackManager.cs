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
        if (leader != null)
        {
            return members.Count + 1; // Include leader
        }
        else
        {
            return members.Count; // No leader, just count members
        }
    }

}