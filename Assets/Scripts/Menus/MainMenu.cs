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
    }

    public void StartGame()
    {
        Debug.Log("Start Game");
        door.GetComponent<opencloseDoor>().OpenDoor();
        //Door opens as the camera slowly enters through the door. Entering a dark void
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
}
