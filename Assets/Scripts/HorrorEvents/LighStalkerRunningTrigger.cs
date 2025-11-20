using UnityEngine;

public class LightStalkerTrigger : MonoBehaviour
{
  public GameObject npcPrefab;     // the character prefab
  public Transform spawnPoint;     // Point A
  public Transform destination;    // Point B

  private bool triggered = false;

  private void OnTriggerEnter(Collider other)
  {
    if (triggered) return;
    if (!other.CompareTag("Player")) return;

    triggered = true;

    // 1. Spawn NPC at Point A
    GameObject npc = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
    npc.transform.Rotate(0f, 180f, 0f); // turn it 180° on Y

    // 2. Give it its destination
    SimpleMover mover = npc.GetComponent<SimpleMover>();
    mover.pointB = destination;

    // 3. Start moving
    mover.StartMoving();
  }
}