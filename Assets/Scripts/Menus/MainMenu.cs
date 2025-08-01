using System.Collections;
using SojaExiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private Button start;

    private Button quit;

    [SerializeField] private GameObject door;

    private Camera _camera;

    private float distance;

    [SerializeField] private float cameraSpeed = 10f;

    [SerializeField] private GameObject cameraEndLocation;

    public void Awake()
    {
        start = transform.Find("Start").gameObject?.GetComponent<Button>();
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

        quit.onClick.AddListener(QuitGame);
    }

    public void Start()
    {
        EventSystem.current.firstSelectedGameObject = start.gameObject;
        _camera = FindAnyObjectByType<Camera>();
    }

    public void StartGame()
    {
        Debug.Log("Start Game");
        //Door opens as the camera slowly enters through the door. Entering the next room.
        //Load what is suppose to be behind the door, this case the Main Game Scene
        //Door Opens
        door.GetComponent<opencloseDoor>().OpenDoor();
        //camera moves until a certain point after the door
        StartCoroutine(SceneTransition(_camera.transform.position, cameraEndLocation.transform.position, cameraSpeed));
        //close the door
        //Load the Game Scene
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        door.GetComponent<opencloseDoor>().OpenDoor();
        Application.Quit();
    }

    public void Update()
    {
        // If no UI element is selected or the selected is not interactable
        /*if (EventSystem.current.currentSelectedGameObject == null ||
            EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() == null)
        {
            // Reset to default button
            EventSystem.current.SetSelectedGameObject(start.gameObject);
        } */
    }

    private IEnumerator SceneTransition(Vector3 cameraStartPos, Vector3 cameraEndPos, float timeToReach)
    {
        float elapsed = 0f;
        while (elapsed < timeToReach)
        {
            _camera.transform.position = Vector3.Lerp(cameraStartPos, cameraEndPos, elapsed / timeToReach);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _camera.transform.position = cameraEndPos;

        door.GetComponent<opencloseDoor>().CloseDoor();
    }
}
