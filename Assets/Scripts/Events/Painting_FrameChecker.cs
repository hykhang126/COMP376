using System.Collections.Generic;
using UnityEngine;

public class FrameChecker : MonoBehaviour
{
    [SerializeField] private Renderer[] materialsToCheck;
    [SerializeField] private Renderer[] frameRenderers;

    public bool isCorrectMaterial = false;
    public bool hasSpawnedKey = false;

    [SerializeField] private Material[] requiredMaterials;

    private static FrameChecker instance;

    void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        List<Material> colorMaterials = new List<Material>(requiredMaterials);
        if (frameRenderers.Length == 0)
        {
            Debug.LogWarning("No frame renderers assigned to EV2_FrameChecker.");
            return;
        }

        for(int i=0;i<materialsToCheck.Length;i++)
        {
            if (frameRenderers[i] == null || materialsToCheck[i] == null)
            {
                Debug.LogWarning("One of the frame renderers or materials to check is not assigned.");
                return;
            }
            int randomIndexToCheck = Random.Range(0, colorMaterials.Count);

            materialsToCheck[i].material.color = colorMaterials[randomIndexToCheck].color;
            Debug.Log("Assigned color " + materialsToCheck[i].material.color + " to material to check " + i);

            colorMaterials.RemoveAt(randomIndexToCheck); // Ensure unique colors
        }

        isCorrectMaterial = false;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < frameRenderers.Length; i++)
        {
            if (frameRenderers[i].material.name != materialsToCheck[i].material.name || frameRenderers[i].material.color != materialsToCheck[i].material.color)
            {
                isCorrectMaterial = false; // If any material does not match
                return;
            }
        }

        isCorrectMaterial = true; // If all materials match
    }
}
