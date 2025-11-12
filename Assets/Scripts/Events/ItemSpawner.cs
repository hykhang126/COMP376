using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    private EV2_FrameChecker frameChecker;
    private AudioSource successSource;

    [SerializeField] private AudioClip successClip;
    [SerializeField] private GameObject[] itemsToActivate;

    private bool hasSpawnedKey = false; // prevent multiple spawns

    void Start()
    {
        frameChecker = transform.parent.GetComponent<EV2_FrameChecker>();
        if (frameChecker == null)
        {
            Debug.LogError("Event checker is null");
        }

        successSource = GetComponent<AudioSource>();

        foreach (var item in itemsToActivate)
        {
            item.SetActive(false);
        }
    }

    void Update()
    {
        if (!hasSpawnedKey && frameChecker != null && frameChecker.isCorrectMaterial)
        {
            PuzzleSuccessful();
            hasSpawnedKey = true;
        }
    }

    private void PuzzleSuccessful()
    {
        // Optional: random spawn near position
        Vector2 offset = Random.insideUnitCircle * 1.5f;
        Vector3 spawnPos = transform.position + new Vector3(offset.x, 0f, offset.y);

        foreach (var item in itemsToActivate)
        {
            item.SetActive(true);
        }
        successSource.PlayOneShot(successClip);
    }
}

