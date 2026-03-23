using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WolfPackManager : MonoBehaviour
{
    public const int MaxMembers = 5;
    public Wolf leader;
    public List<Wolf> members = new List<Wolf>();

    public Vector3 GetPackDirection()
    {
        return leader.transform.forward;
    }

}
