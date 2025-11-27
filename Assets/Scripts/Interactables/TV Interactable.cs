using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class TVInteractable : Interactable
{

    private VideoPlayer videoPlayer;

    private GameObject _screen;

    private Renderer _screenRenderer;

    [SerializeField] private Material TVOffMaterial;

    [SerializeField] private Material TVOnMaterial;

    [SerializeField] private Material TVCameraFeedMaterial;

    [SerializeField] private bool isCameraFeed = false;

    private bool isPlaying = false;

    Light TVLight;

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Transform screenTransform = transform.Find("Screen");
        if (screenTransform != null)
        {
            _screen = screenTransform.gameObject;
        }
        _screenRenderer = _screen.GetComponent<Renderer>();
        videoPlayer = _screen.GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer component not found on the screen object.");
            return;
        }

        _screenRenderer.material = TVOffMaterial;
        Debug.Log("Changing material to TVOFF");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on the TVInteractable object.");
        }
        if (SceneManager.GetActiveScene().name == "HorrorActScene1")
            StartCoroutine(DelayTV());
        else if(SceneManager.GetActiveScene().name == "TutorialScene")
            Inventory.sandwichEvent.AddListener(Interact);
    
  }

  IEnumerator DelayTV()
  {
    yield return new WaitForSeconds(1f);
    Interact();

  }

  public override void Interact()
    {
        isPlaying = !isPlaying;
        Debug.Log("TV Interacted with. Is Playing: " + isPlaying);
        //If the tv was off, turn it on and player the video
        if (isPlaying)
        {
            StartCoroutine(WaitForVideoToStart());
        }
        else
        {
            _screenRenderer.material = TVOffMaterial;
            if (!isCameraFeed)
            {
                videoPlayer.Stop();
            }
        }
    }

    IEnumerator WaitForVideoToStart()
    {
        //Wait for video to start playing before changing the material
        if (!isCameraFeed)
        {
            videoPlayer.Play();
            yield return new WaitUntil(() => videoPlayer.isPlaying);
            _screenRenderer.material = TVOnMaterial ? TVOnMaterial : null;
        }
        else
        {
            _screenRenderer.material = TVCameraFeedMaterial;
        }
    }
}
