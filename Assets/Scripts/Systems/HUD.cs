using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{   
    public GameObject EndGamePanel{ get; private set; }
    public GameObject EndGameVolume { get; private set; }
    private Button quitGame;

    private GameObject GameOverPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // DontDestroyOnLoad(gameObject);
        EndGamePanel = transform.Find("EndGame")?.gameObject;

        if (quitGame != null)
        {
#if UNITY_WEBGL
            quitGame.gameObject.SetActive(false); // Hide on WebGL
#endif
        }

    if (EndGamePanel != null)
            EndGamePanel.SetActive(false);
        
        EndGameVolume = GameObject.Find("End Game Volume");

        GameOverPanel = transform.parent.Find("GameOver")?.gameObject;
        if (GameOverPanel != null)
            GameOverPanel.SetActive(false);
        else
        {
            Debug.LogError("GameOverPanel not found in HUD.");
        }

        FindAnyObjectByType<LightStalkerController>().onNeckSnap.AddListener(DeathScreen);
    }
    
    

    public void QuitGame()
    {
        Debug.Log("QuitGame");
        Application.Quit();
    }

    public void ShowEndGamePrompt()
    {
        Player.InstanceReference.playerInputHandler.DisableInput();

        Player.InstanceReference.stateMachine.InvokeStateEvent(PlayerStateType.InMenu.ToString());
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EndGamePanel.SetActive(true);
        if (quitGame != null)
        {
        #if UNITY_WEBGL
            quitGame.gameObject.SetActive(false); // Hide on WebGL
            EventSystem.current.SetSelectedGameObject(quitGame.gameObject);
        #endif
        }
    }

    private void DeathScreen()
    {
        GameOverPanel.SetActive(true);
    }
}
