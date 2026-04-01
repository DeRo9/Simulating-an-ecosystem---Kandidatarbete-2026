using UnityEngine;
using System.Collections;

public class ReplenishBerries : MonoBehaviour
{
    [SerializeField] private GameObject berryBunchPrefab;
    [SerializeField] private float respawnDelay = 10f;

    private bool isRespawning = false;

    void OnTransformChildrenChanged()
    {
        if (transform.childCount == 1)
        {
            StartCoroutine(RespawnBerries());
        }
    }

    IEnumerator RespawnBerries()
    {
        if (SeasonManager.Instance.IsWinter)
        {
            respawnDelay *= 5f;
        }
        else if (SeasonManager.Instance.IsRaining)
        {
            respawnDelay *= 0.7f;
        }
        
        yield return new WaitForSeconds(respawnDelay);
        if (!SeasonManager.Instance.IsSnowing)
        {
            Instantiate(berryBunchPrefab, transform.position, Quaternion.identity, transform);
        }
    }
}