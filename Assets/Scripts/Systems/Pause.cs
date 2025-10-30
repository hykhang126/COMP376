using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class Pause : MonoBehaviour
{

    public PauseAction action;
    public GameObject pauseMenu;

    [Header("Pause Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private GameSettingsSO gameSettingsSO;

    private bool paused = false;
    private Inventory inventorySystem;
    private string previousPlayerState;

    private void Awake()
    {
        action = new PauseAction();
    }

    private void Start()
    {
        action.Pause.PauseGame.performed += _ => DeteminePause();
        pauseMenu.SetActive(false); // Ensure the pause menu is hidden at start
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);
        
        inventorySystem = FindAnyObjectByType<Inventory>();

        if (!gameSettingsSO)
            gameSettingsSO = Resources.Load<GameSettingsSO>("Scriptable Objects/GameSettingsSO");
    }

    public void Update()
    {
        if (EventSystem.current != null)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (EventSystem.current.GetComponent<InputSystemUIInputModule>().move.action.triggered)
                {
                    EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
                }
            }
        }
    }

    private void DeteminePause()
    {
        if (paused)
            ResumeGame();
        else
            PauseGame();
    }
    private void OnEnable()
    {
        action.Enable();
    }
    private void OnDisable()
    {
        action.Disable();
    }
    public void PauseGame()
    {
        // State handler
        if (Player.InstanceReference.stateMachine.GetCurrentStateName() == PlayerStateType.InMenu.ToString()) return;
        previousPlayerState = Player.InstanceReference.stateMachine.GetCurrentStateName();
        Player.InstanceReference.stateMachine.InvokeStateEvent(PlayerStateType.InMenu.ToString());

        Time.timeScale = 0f; // Pause the game
        paused = true;
        pauseMenu.SetActive(true); // Show the pause menu
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make the cursor visible

        Player.InstanceReference.playerInputHandler.DisableInput(); // Disable player controls

        if (resumeButton != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        inventorySystem.actions.Disable();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game
        paused = false;
        pauseMenu.SetActive(false); // Hide the pause menu
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false; // Hide the cursor

        Player.InstanceReference.playerInputHandler.EnableInput(); // Enable player controls

        EventSystem.current.SetSelectedGameObject(null);
        inventorySystem.actions.Enable();

        //Might cause a bug where if you pause when you were carrying before

        // Reset to previous state
        Player.InstanceReference.stateMachine.InvokeStateEvent(previousPlayerState);
    }

    public void QuitGame()
    {
        Debug.Log("QuttingGame");
        Application.Quit();
    }
}   