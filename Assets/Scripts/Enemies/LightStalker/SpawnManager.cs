using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance { get; private set; }

    public List<Transform> spawnPoints = new List<Transform>();
    public Camera playerCamera;
    public LayerMask viewOcclusionMask = ~0;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        if (playerCamera == null) playerCamera = Camera.main;
    }

    public void NotifyDespawned(LightStalkerController enemy, float respawnDelay)
    {
        enemy.gameObject.SetActive(false);
        StartCoroutine(RespawnCoroutine(enemy, respawnDelay));
    }

    IEnumerator RespawnCoroutine(LightStalkerController enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        Transform chosen = FindFurthestSpawnerNotInView(enemy.transform.position);
        if (chosen == null)
        {
            //Fallback: pick the furthest spawner ignoring view
            chosen = FindFurthestSpawnerFrom(enemy.transform.position);
        }

        //Ensure the chosen spawner isn't inside player view (best-effort)
        Vector3 spawnPos = chosen.position;
        enemy.RespawnAt(spawnPos);
    }

    Transform FindFurthestSpawnerNotInView(Vector3 fromPosition)
    {
        Transform best = null;
        float bestDist = -1f;
        foreach (var s in spawnPoints)
        {
            float d = Vector3.Distance(fromPosition, s.position);
            if (d <= bestDist) continue;
            if (IsInPlayerView(s.position)) continue; //Skip spawners currently visible
            best = s;
            bestDist = d;
        }
        return best;
    }

    Transform FindFurthestSpawnerFrom(Vector3 fromPosition)
    {
        Transform best = null; float bestDist = -1f;
        foreach (var s in spawnPoints)
        {
            float d = Vector3.Distance(fromPosition, s.position);
            if (d > bestDist) { bestDist = d; best = s; }
        }
        return best;
    }

    bool IsInPlayerView(Vector3 worldPos)
    {
        if (playerCamera == null) return false;
        Vector3 vp = playerCamera.WorldToViewportPoint(worldPos);
        bool inViewport = vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;
        if (!inViewport) return false;

        //Raycast to check occlusion (if occluded, treat as not in view)
        Vector3 camPos = playerCamera.transform.position;
        Vector3 dir = (worldPos - camPos);
        if (Physics.Raycast(camPos, dir.normalized, out RaycastHit hit, dir.magnitude, viewOcclusionMask))
        {
            //If hit something before the spawner, it's occluded and not in view
            return false;
        }
        return true;
    }
}
