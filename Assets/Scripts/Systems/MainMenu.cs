using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // Serialized
    [SerializeField] private GameObject door;
    [SerializeField] private GameObject playerEndLocation;
    [SerializeField] private GameObject blackVoid;
    [SerializeField] private float cameraSpeed = 10f;

    // UI references
    private Button start;
    private Button quit;
    // Systems
    private Pause pauseSystem;
    private Inventory inventorySystem;


    public void Awake()
    {
        start = transform.Find("Start").gameObject.GetComponent<Button>();
        if (start == null)
        {
            Debug.LogError("Start button not found");
        }

        start.onClick.AddListener(StartGame);

        quit = transform.Find("Quit").gameObject?.GetComponent<Button>();
        if (quit == null)
        {
            Debug.LogError("Quit button not found");
        }

        #if UNITY_WEBGL
                quit.SetActive(false);
        #endif

        quit.onClick.AddListener(QuitGame);

    // No local player cache — use Player.InstanceReference when needed

        pauseSystem = FindAnyObjectByType<Pause>();

        inventorySystem = FindAnyObjectByType<Inventory>();


    }

    public void Start()
    {
        Player.InstanceReference.stateMachine.InvokeStateEvent(PlayerStateType.InMenu.ToString());
        StartCoroutine(MainMenuInit());




    }

    private IEnumerator MainMenuInit()
    {
        yield return null;
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make the cursor visible
        EventSystem.current.SetSelectedGameObject(start.gameObject);
    }

    public void StartGame()
    {
        Debug.Log("Start Game");
        //Door opens as the camera slowly enters through the door. Entering the next room.
        //Load what is suppose to be behind the door, this case the Main Game Scene
        //Door Opens
        door.GetComponent<DoorAction>().OpenDoor();
        //camera moves until a certain point after the door
        Vector3 playerStartPos = Player.InstanceReference != null ? Player.InstanceReference.transform.position : Vector3.zero;
        StartCoroutine(StartSceneTransition(playerStartPos, playerEndLocation.transform.position, cameraSpeed));
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        blackVoid.SetActive(true);
        door.GetComponent<DoorAction>().OpenDoor();
        Vector3 playerStartPos = Player.InstanceReference != null ? Player.InstanceReference.transform.position : Vector3.zero;
        StartCoroutine(QuitSceneTransition(playerStartPos, playerEndLocation.transform.position, cameraSpeed));
    }

    public void Update()
    {
        if (EventSystem.current != null)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (EventSystem.current.GetComponent<InputSystemUIInputModule>().move.action.triggered)
                {
                    EventSystem.current.SetSelectedGameObject(start.gameObject);
                }
            }
        }
    }

    private IEnumerator StartSceneTransition(Vector3 playerStartPos, Vector3 playerEndPos, float timeToReach)
    {
        start.interactable = false;
        quit.interactable = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        float elapsed = 0f;
        while (elapsed < timeToReach)
        {
            if (Player.InstanceReference != null)
                Player.InstanceReference.transform.position = Vector3.Lerp(playerStartPos, playerEndPos, elapsed / timeToReach);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (Player.InstanceReference != null)
            Player.InstanceReference.transform.position = playerEndPos;

        door.GetComponent<DoorAction>().CloseDoor();
        if (Player.InstanceReference != null)
            Player.InstanceReference.playerInputHandler.EnableInput(); // Re-enable player input actions
        // Transition player to Idle state after game start
        Player.InstanceReference.stateMachine.InvokeStateEvent("toIdle");
        if (blackVoid != null)
        {
            blackVoid.SetActive(false);
        }
        this.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Unlock the cursor
        Cursor.visible = false; // Make the cursor visible
        EventSystem.current.SetSelectedGameObject(null);
        //Unlock other systems
        pauseSystem.action.Enable();
        inventorySystem.actions.Enable();

    }

    private IEnumerator QuitSceneTransition(Vector3 playerStartPos, Vector3 playerEndPos, float timeToReach)
    {
        start.interactable = false;
        quit.interactable = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        float elapsed = 0f;
        while (elapsed < timeToReach)
        {
            if (Player.InstanceReference != null)
                Player.InstanceReference.transform.position = Vector3.Lerp(playerStartPos, playerEndPos, elapsed / timeToReach);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (Player.InstanceReference != null)
            Player.InstanceReference.transform.position = playerEndPos;

        Application.Quit();

    }
}
