using UnityEngine;

[CreateAssetMenu(fileName = "GameSettingsSO", menuName = "Scriptable Objects/GameSettingsSO")]
public class GameSettingsSO : MyScriptables
{
    public GraphicsQuality graphicsQuality;

    public bool isFullscreen = true;

    public Resolution resolution;

    [NaughtyAttributes.Button("Clear All Data")]
    public override void ClearAllData()
    {
        base.ClearAllData();
        graphicsQuality = GraphicsQuality.Medium;
        isFullscreen = true;
        resolution = Resolution.R_1920x1080;
    }

    public enum GraphicsQuality
    {
        Low,
        Medium,
        High
    }

    public enum Resolution
    {
        R_640x480,
        R_800x600,
        R_1024x768,
        R_1280x720,
        R_1920x1080,
        R_2560x1440,
        R_3840x2160
    }
}