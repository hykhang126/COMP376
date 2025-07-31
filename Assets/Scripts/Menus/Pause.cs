using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{

    public PauseAction action;

    private bool paused = false;

    public GameObject pauseMenu;

    [SerializeField] private Button resumeButton;

    [SerializeField] private Button quitButton;

    [SerializeField] private Player playerController;



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
        Time.timeScale = 0f; // Pause the game
        paused = true;
        pauseMenu.SetActive(true); // Show the pause menu
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make the cursor visible

        if (playerController != null)
            playerController.playerInput.enabled = false;

        if (resumeButton != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game
        paused = false;
        pauseMenu.SetActive(false); // Hide the pause menu
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false; // Hide the cursor

        if (playerController != null)
            playerController.playerInput.enabled = true;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void QuitGame()
    {
        Debug.Log("QuttingGame");
        Application.Quit();
    }
}
