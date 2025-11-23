using UnityEngine;

public class LightStalkerTrigger : MonoBehaviour
{
    [Header("NPC / Movement")]
    public GameObject npcPrefab;     // the character prefab
    public Transform spawnPoint;     // Point A
    public Transform destination;    // Point B

    [Header("Jumpscare Audio")]
    [Tooltip("One-shot jumpscare clip played from this trigger (2D).")]
    public AudioClip jumpscareClip;
    [Range(0f, 1f)]
    public float jumpscareVolume = 1f;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Spawn NPC at Point A
        GameObject npc = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
        npc.transform.Rotate(0f, 180f, 0f); // face the player

        // Give it its destination and start moving
        SimpleMover mover = npc.GetComponent<SimpleMover>();
        if (mover != null)
        {
            mover.pointB = destination;
            mover.StartMoving();
        }
        else
        {
            Debug.LogWarning("Spawned NPC missing SimpleMover component.");
        }

        // Play jumpscare from the trigger (2D so player always hears it)
        if (jumpscareClip != null)
        {
            AudioSource src = GetComponent<AudioSource>();
            if (src == null)
            {
                src = gameObject.AddComponent<AudioSource>();
                src.spatialBlend = 0f; // 0 => 2D (non-spatial)
                src.playOnAwake = false;
                src.loop = false;
            }

            src.PlayOneShot(jumpscareClip, jumpscareVolume);
        }
        else
        {
            Debug.LogWarning("No jumpscareClip assigned on LightStalkerTrigger.");
        }
    }
}
