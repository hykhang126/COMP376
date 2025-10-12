using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/EnemyConfig")]
public class LightStalkerConfig : ScriptableObject
{
    public float moveSpeed = 3.5f;
    public float acceleration = 8f;
    public float stoppingDistance = 1.2f;

    public float terrorRadius = 12f;//Full terror effects kick in
    public float terrorSoundStartDistance = 20f;//Sound cue warning player enemy is coming

    public float flashlightStunSeconds = 3f; //Seconds under beam to despawn
    public float respawnDelay = 8f;

    public float fleeDistanceWhenIlluminated = 6f;
}