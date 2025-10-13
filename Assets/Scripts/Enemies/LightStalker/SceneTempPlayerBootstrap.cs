using System.Collections;
using UnityEngine;

public class SceneTempPlayerBootstrap : MonoBehaviour
{
    [Tooltip("State event to invoke on player's StateMachine (default -> toIdle)")]
    [SerializeField] private string stateEventToInvoke = "toIdle";

    [SerializeField] private float startDelay = 0.05f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(startDelay);

        var player = Object.FindFirstObjectByType<Player>();
        if (player == null)
        {
            Debug.LogWarning("[SceneTempPlayerBootstrap] No Player found in scene.");
            yield break;
        }

        if (player.playerInput != null && player.playerInput.actions != null)
        {
            player.playerInput.actions.Enable();
            Debug.Log("[SceneTempPlayerBootstrap] Enabled playerInput.actions");
        }

        try
        {
            player.InMenuExit();
            Debug.Log("[SceneTempPlayerBootstrap] Called player.InMenuExit()");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[SceneTempPlayerBootstrap] Could not call InMenuExit(): " + e.Message);
        }

        if (player.stateMachine != null)
        {
            player.stateMachine.InvokeStateEvent(stateEventToInvoke);
            Debug.Log("[SceneTempPlayerBootstrap] Invoked state event: " + stateEventToInvoke);
        }
        else
        {
            Debug.LogWarning("[SceneTempPlayerBootstrap] player.stateMachine == null");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
