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
        yield return new WaitForSeconds(respawnDelay);
        Instantiate(berryBunchPrefab, transform.position, Quaternion.identity, transform);
    }
}