using System.Collections.Generic;
using UnityEngine;

public class SpriteChecker : MonoBehaviour
{
    public static SpriteChecker Instance;

    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private SpriteRenderer[] spriteToCheck;

    [SerializeField] private List<Material> requiredMaterials;

    public bool isPuzzleCorrect = false;
    public bool hasSpawnedKey = false;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spriteRenderers.Length == 0)
        {
            Debug.LogWarning("No sprite renderers assigned to EV2_SpriteChecker.");
            return;
        }

        RandomizeSpriteColors();
        isPuzzleCorrect = false;
    }

    private void RandomizeSpriteColors()
    {
        // Randomize colors for sprites to check
        Color[] requiredColors = requiredMaterials.ConvertAll(mat => mat.color).ToArray();
        requiredColors = Painting_VialsScatter.FisherYatesShuffle(requiredColors);

        for (int i = 0; i < spriteToCheck.Length && i < requiredColors.Length; i++)
        {
            if (spriteToCheck[i] == null)
            {
                Debug.LogWarning("One of the sprite renderers to check is not assigned.");
                return;
            }
            spriteToCheck[i].color = requiredColors[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i].sprite != spriteToCheck[i].sprite || spriteRenderers[i].color != spriteToCheck[i].color)
            {
                isPuzzleCorrect = false; // If any sprite does not match
                return;
            }
        }

        isPuzzleCorrect = true; // If all sprites match
    }
}
