using System.Collections;
using UnityEngine;

public class EV3_CreatureApproaching : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [NaughtyAttributes.Button("Activate Creature")]
    public void ActivateCreature()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(ApproachCreature());
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
