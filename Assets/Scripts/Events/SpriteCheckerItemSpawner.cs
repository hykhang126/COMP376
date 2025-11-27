using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpriteCheckerItemSpawner : MonoBehaviour
{
    private SpriteChecker spriteChecker;
    private AudioSource successSource;

    [SerializeField] private AudioClip successClip;
    [SerializeField] private GameObject[] itemsToActivate;

    private bool hasSpawnedKey = false; // prevent multiple spawns

    void Start()
    {
        spriteChecker = transform.parent.GetComponent<SpriteChecker>();
        if (spriteChecker == null)
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
        if (!hasSpawnedKey && spriteChecker != null && spriteChecker.isPuzzleCorrect)
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

