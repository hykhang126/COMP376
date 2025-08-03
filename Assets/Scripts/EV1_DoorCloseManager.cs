using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EV1_DoorCloseManager : MonoBehaviour
{
    [Header("Door Jumpscare settings")]
    [SerializeField] private Door[] doors;

    [SerializeField] private BoxCollider EV1_Trigger;

    [SerializeField] private float waitTime = 0.5f;

    [SerializeField] private EV3_CreatureApproaching creatureEvent;

    [SerializeField] private TVInteractable tVInteractable;

    [SerializeField] Player player;

    public ItemFlood[] keyfloodEvents;

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

        if (EV1_Trigger == null)
        {
            Debug.LogError("Trigger component not found on the EV1_DoorCloseManager object.");
        }

        if (tVInteractable == null)
        {
            Debug.LogError("TVInteractable component not found on the EV1_DoorCloseManager object.");
        }

        if (creatureEvent == null)
        {
            Debug.LogError("CreatureApproaching component not found on the EV1_DoorCloseManager object.");
        }
    }

    public void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(CloseDoorsContinous(waitTime));
        foreach (var itemFlood in keyfloodEvents)
            StartCoroutine(SpawnKeys(itemFlood));

        // Start the creature approach event
        if (creatureEvent != null)
        {
            creatureEvent.ActivateCreature();
        }
        // Start the TV interaction event
        if (tVInteractable != null)
        {
            tVInteractable.Interact(player);
        }
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

    private IEnumerator SpawnKeys(ItemFlood itemFlood)
    {
        int counter = 0;
        while (EV1_Trigger.enabled == true)
        {
            if(counter < 50)
                itemFlood.SpawnKey();
            yield return new WaitForSeconds(0.01f);
        }
    }
}