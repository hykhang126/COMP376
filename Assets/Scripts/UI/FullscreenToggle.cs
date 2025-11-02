using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class FullscreenToggle : MonoBehaviour
{
    // FullscreenToggle
    [SerializeField] private Toggle fullscreenToggle;

    [SerializeField] private GameSettingsSO gameSettingsSO;

    void Start()
    {
        // Null check
        if (!gameSettingsSO)
        {
            gameSettingsSO = Resources.Load<GameSettingsSO>("Scriptable Objects/GameSettingsSO");
        }

        if (!fullscreenToggle)
        {
            fullscreenToggle = GetComponent<Toggle>();
        }

        // Initialize UI
        fullscreenToggle.isOn = gameSettingsSO.isFullscreen;

        fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
    }

    // Toggle fullscreen
    private void ToggleFullscreen(bool isOn)
    {
        gameSettingsSO.isFullscreen = isOn;
        Screen.fullScreen = isOn;
    }

    // Debug button to toggle fullscreen in editor
    [NaughtyAttributes.Button("Toggle Fullscreen")] 
    private void ToggleFullscreenDebug()
    {
        fullscreenToggle.isOn = !fullscreenToggle.isOn;
        ToggleFullscreen(fullscreenToggle.isOn);
    }
}
