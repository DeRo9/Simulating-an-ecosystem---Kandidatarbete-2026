using UnityEngine;
using System.Collections;

public class ReplenishBerries : MonoBehaviour
{
    [SerializeField] private GameObject berryBunchPrefab;
    [SerializeField] private float respawnDelay = 40f;

    private bool isRespawning = false;

    void OnTransformChildrenChanged()
    {
        if (transform.childCount == 1 && !isRespawning)
        {
            StartCoroutine(RespawnBerries());
        }
    }

    IEnumerator RespawnBerries()
    {
        isRespawning = true;

        float currentDelay = respawnDelay;

        if (SeasonManager.Instance.IsWinter)
        {
            currentDelay *= 3f;
        }
        else if (SeasonManager.Instance.IsRaining)
        {
            currentDelay *= 0.7f;
        }
        
        yield return new WaitForSeconds(currentDelay);
        Instantiate(berryBunchPrefab, transform.position, Quaternion.identity, transform);

        isRespawning = false;
        
    }
}