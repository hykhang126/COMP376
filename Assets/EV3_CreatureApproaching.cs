using System.Collections;
using UnityEngine;

public class EV3_CreatureApproaching : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Speed at which the creature approaches
    [SerializeField] private float approachDistance = 20f; // Distance at which the game ends

    [SerializeField] private float timeToEventEnd = 50f;

    [SerializeField] private bool isActivated = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isActivated = false;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Lerp the creature's position towards the player
        if (isActivated)
        {
            // Lerp?
        }
    }

    [NaughtyAttributes.Button("Activate Creature")]
    public void ActivateCreature()
    {
        this.gameObject.SetActive(true);
        isActivated = true;
        // StartCoroutine(ApproachCreature());
    }

    [NaughtyAttributes.Button("Stop Creature")]
    public void StopCreature()
    {
        StopCoroutine(ApproachCreature());
    }
    
    IEnumerator ApproachCreature(float waitTime = 5f)
    {
        while (true)
        {
            transform.position += new Vector3(0, 0, 5f); // Move the creature forward
            yield return new WaitForSeconds(waitTime); // Wait for a short duration
        }
    }
}
