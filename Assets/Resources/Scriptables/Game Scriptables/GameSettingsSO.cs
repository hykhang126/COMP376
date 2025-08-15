using UnityEngine;

public enum GraphicsQuality
{
    Low,
    Medium,
    High
}

[CreateAssetMenu(fileName = "GameSettingsSO", menuName = "Scriptable Objects/GameSettingsSO")]
public class GameSettingsSO : ScriptableObject
{
    [SerializeField] private GraphicsQuality graphicsQuality;

    public GraphicsQuality GraphicsQuality => graphicsQuality;
}