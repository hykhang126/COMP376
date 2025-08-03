using System.Collections;
using UnityEngine;

public class EV3_CreatureApproaching : MonoBehaviour
{
    [SerializeField] private float approachDistance = 20f; // Distance at which the game ends

    [SerializeField] private float timeToEventEnd = 50f;

    [SerializeField] private bool isActivated = false;

    [SerializeField] private Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isActivated = false;
        gameObject.SetActive(false);

        if (player == null)
        {
            Debug.LogError("Player reference not set in EV3_CreatureApproaching.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActivated) return;

        // Lerp the creature's position towards the player
        transform.position = Vector3.Lerp(transform.position, player.transform.position, timeToEventEnd);
    }

    [NaughtyAttributes.Button("Activate Creature")]
    public void ActivateCreature()
    {
        isActivated = true;
        gameObject.SetActive(true);
        // StartCoroutine(ApproachCreature());
    }

    [NaughtyAttributes.Button("Stop Creature")]
    public void StopCreature()
    {
        isActivated = false;
        gameObject.SetActive(false);
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
