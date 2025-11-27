using UnityEngine;
using UnityEngine.Events;

public class DeathManager : MonoBehaviour
{
    public UnityEvent onJumpscareComplete = new();

    public UnityEvent onNeckSnap = new();
}
