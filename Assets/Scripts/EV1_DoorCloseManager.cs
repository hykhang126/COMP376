using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EV1_DoorCloseManager : MonoBehaviour
{
    [Header("Door Jumpscare settings")]
    [SerializeField] private Door[] doors;

    [SerializeField] private BoxCollider EV1_Trigger;

    [SerializeField] private float waitTime = 0.5f;

    private void Awake()
    {
        if (doors == null)
        {
            Debug.LogError("Door component not found on the EV1_DoorCloseManager object.");
        }

        if (EV1_Trigger == null)
        {
            // Find trigger in children 
            EV1_Trigger = GetComponentInChildren<BoxCollider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(CloseDoorsContinous(waitTime));
    }

    IEnumerator CloseDoorsContinous(float waitTime = 0.5f)
    {
        for (int i = 0; i < 10; i++)
        {
            foreach (var door in doors)
            {
                if (door.doorAction != null)
                {
                    yield return StartCoroutine(door.doorAction.opening(waitTime));
                }
            }

            foreach (var door in doors)
            {
                if (door.doorAction != null)
                {
                    yield return StartCoroutine(door.doorAction.closing(waitTime));
                }
            }
        }

        // Turn off the trigger after closing doors
        EV1_Trigger.enabled = false;
        yield return new WaitForSeconds(waitTime);
    }
}