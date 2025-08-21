using UnityEngine;
using UnityEngine.UI;

public class FullscreenToggle : MonoBehaviour
{
    // FullscreenToggle
    [SerializeField] private Toggle fullscreenToggle;

    [SerializeField] private GameSettingsSO gameSettingsSO;

    void Start()
    {
        fullscreenToggle = GetComponent<Toggle>();
        fullscreenToggle.isOn = gameSettingsSO.isFullscreen;
        fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);

        // Null check
        if (!gameSettingsSO)
        {
            gameSettingsSO = Resources.Load<GameSettingsSO>("Scriptable Objects/GameSettingsSO");
        }
    }

    // Toggle fullscreen
    [NaughtyAttributes.Button("Toggle Fullscreen")]
    private void ToggleFullscreen(bool isOn)
    {
        gameSettingsSO.isFullscreen = isOn;
        Screen.fullScreen = isOn;
    }
}
