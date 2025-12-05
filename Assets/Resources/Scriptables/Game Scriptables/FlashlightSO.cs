using UnityEngine;

[CreateAssetMenu(fileName = "FlashlightSO", menuName = "Scriptable Objects/FlashlightSO")]
public class FlashlightSO : ScriptableObject
{
    float _batteryLife = 30f;

    float _batteryLifeCheckPoint = 30f;

    bool resetOnDeath = false;

    string previousScene = "";

    public string PreviousScene { get { return previousScene; } set { previousScene = value; } }
    public float BatteryLife { get { return _batteryLife; } set { _batteryLife = value; } }

    public float BatteryLifeCheckPoint{ get { return _batteryLifeCheckPoint; } set { _batteryLifeCheckPoint = value; } }

    public bool ResetOnDeath { get { return resetOnDeath; } set { resetOnDeath = value; } }
}
