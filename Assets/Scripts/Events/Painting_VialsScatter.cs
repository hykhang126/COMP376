using UnityEngine;

public class Painting_VialsScatter : MonoBehaviour
{
    [SerializeField] private GameObject[] vialsToScatter;
    [SerializeField] private Transform[] scatterLocations;

    public static T[] FisherYatesShuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (array[j], array[i]) = (array[i], array[j]);
        }
        return array;
    }

    void Awake()
    {
        // null check
        if (vialsToScatter.Length == 0 || scatterLocations.Length == 0)
        {
            Debug.LogWarning("No vials or scatter locations assigned to Painting_VialsScatter.");
            return;
        }

        // Scatter vials to random locations
        FisherYatesShuffle(scatterLocations);
        for (int i = 0; i < vialsToScatter.Length && i < scatterLocations.Length; i++)
        {
            vialsToScatter[i].transform.position = scatterLocations[i].position;
        }
    }
}
