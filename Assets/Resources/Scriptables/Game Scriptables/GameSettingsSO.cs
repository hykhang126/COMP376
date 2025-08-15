using UnityEngine;

public enum GraphicsQuality
{
    Low,
    Medium,
    High
}

[CreateAssetMenu(fileName = "GameSettingsSO", menuName = "Scriptable Objects/GameSettingsSO")]
public class GameSettingsSO : MyScriptables
{
    public GraphicsQuality graphicsQuality;

    [NaughtyAttributes.Button("Clear All Data")]
    public override void ClearAllData()
    {
        base.ClearAllData();
        graphicsQuality = GraphicsQuality.Medium;
    }
}