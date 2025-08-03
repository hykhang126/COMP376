using UnityEngine;

public class ItemFlood : MonoBehaviour
{

    [SerializeField] GameObject keyPrefab;
    public float spawnRadius = 2f; // radius around spawn location

    public void SpawnKey()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            0f, // keep y the same, or adjust if you want vertical randomness
            Random.Range(-spawnRadius, spawnRadius)
        );

        Vector3 spawnPosition = transform.position + randomOffset;

        Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
    }

}
