using UnityEngine;

public class Door : MonoBehaviour
{
    public DoorAction doorAction;

    private void Awake()
    {
        doorAction = GetComponent<DoorAction>();
        if (doorAction == null)
        {
            Debug.LogError("DoorAction component not found on the Door object.");
        }
    }
    
	// Make a function to open the door in the inspector
    [NaughtyAttributes.Button("Open Door")]
	public void OpenDoorInspector()
	{
		doorAction.OpenDoor();
	}

	// Make a function to close the door in the inspector
	[NaughtyAttributes.Button("Close Door")]
	public void CloseDoorInspector()
	{
		doorAction.CloseDoor();
	}
}
