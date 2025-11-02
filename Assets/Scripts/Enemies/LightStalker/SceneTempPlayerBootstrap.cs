using System.Collections;
using UnityEngine;

public class SceneTempPlayerBootstrap : MonoBehaviour
{
    [SerializeField] private float startDelay = 0.05f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(startDelay);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
