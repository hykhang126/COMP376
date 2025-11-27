using UnityEngine;
using UnityEngine.Events;

public class DeathManager : MonoBehaviour
{
    public UnityEvent onJumpscareComplete = new UnityEvent();

    public UnityEvent onNeckSnap = new UnityEvent();

    public UnityEvent onDeathSequenceStart = new UnityEvent();
}
