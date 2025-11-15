using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlickerCycle : MonoBehaviour
{
  [Header("Base Light Settings")]
  [Tooltip("The steady intensity when the light is ON.")]
  public float baseIntensity = 1f;

  [Tooltip("Base light range when ON.")]
  public float baseRange = 10f;

  [Header("Flicker Settings")]
  [Tooltip("Maximum intensity variation during flicker phase.")]
  public float flickerAmplitude = 0.5f;

  [Tooltip("How fast the light flickers (Perlin noise speed).")]
  public float flickerSpeed = 10f;

  [Tooltip("Optional offset so multiple lights flicker differently.")]
  public float noiseOffset = 0f;

  [Header("Cycle Settings")]
  [Tooltip("Minimum time (in seconds) the light stays steady ON before flickering.")]
  public float minOnDuration = 2f;

  [Tooltip("Maximum time (in seconds) the light stays steady ON before flickering.")]
  public float maxOnDuration = 5f;

  [Tooltip("How long (in seconds) the light flickers before returning to ON state.")]
  public float flickerDuration = 1.5f;

  [Header("Extra Effects")]
  [Tooltip("Whether to also vary the range during flicker.")]
  public bool affectRange = false;

  [Range(0f, 1f)]
  [Tooltip("How much the range varies during flicker.")]
  public float rangeVariation = 0.1f;

  private Light _light;
  private float _initialRange;
  private float _timeSeed;
  private float _stateTimer;
  private bool _isFlickering;

  void Start()
  {
    _light = GetComponent<Light>();
    _initialRange = _light.range;
    _timeSeed = Random.Range(0f, 1000f) + noiseOffset;

    // Start with a random ON duration
    _stateTimer = GetRandomOnDuration();
    _isFlickering = false;
  }

  void Update()
  {
    _stateTimer -= Time.deltaTime;

    if (_isFlickering)
    {
      HandleFlickerPhase();

      if (_stateTimer <= 0f)
      {
        EndFlickerPhase();
      }
    }
    else
    {
      HandleOnPhase();

      if (_stateTimer <= 0f)
      {
        StartFlickerPhase();
      }
    }
  }

  // ---- STATE HANDLERS ----
  private void HandleOnPhase()
  {
    _light.intensity = baseIntensity;
    _light.range = baseRange;
  }

  private void HandleFlickerPhase()
  {
    float noise = Mathf.PerlinNoise(_timeSeed, Time.time * flickerSpeed);
    float intensity = baseIntensity + (noise - 0.5f) * 2f * flickerAmplitude;
    _light.intensity = Mathf.Max(0f, intensity);

    if (affectRange)
    {
      float rangeNoise = Mathf.PerlinNoise(_timeSeed + 50f, Time.time * flickerSpeed);
      _light.range = _initialRange * (1f + (rangeNoise - 0.5f) * 2f * rangeVariation);
    }
  }

  private void StartFlickerPhase()
  {
    _isFlickering = true;
    _stateTimer = flickerDuration;
  }

  private void EndFlickerPhase()
  {
    _isFlickering = false;
    _stateTimer = GetRandomOnDuration();
    _light.intensity = baseIntensity;
    _light.range = baseRange;
  }

  // ---- UTILITY ----
  private float GetRandomOnDuration()
  {
    return Random.Range(minOnDuration, maxOnDuration);
  }
}
