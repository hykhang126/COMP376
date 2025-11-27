using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class FinalPuzzleManager : MonoBehaviour
{
    [SerializeField] private ItemContractSO finalKeyItemSO;

    public UnityEvent onJumpscareComplete = new UnityEvent();

    public UnityEvent onNeckSnap = new UnityEvent();

    public void Start()
    {
        FindAnyObjectByType<DeathManager>().onJumpscareComplete.AddListener(GameOver);
    }
    
    public void CheckExitCondition()
    {
        if(Inventory.InstanceReference != null || Inventory.InstanceReference.playerInventorySO.items.Count > 0)
        {
            if(Inventory.InstanceReference.playerInventorySO.items.Count > 0 && Inventory.InstanceReference.playerInventorySO.items[Inventory.InstanceReference.GetCurrentItemIndex()].Id == finalKeyItemSO.Id)
            {
                Debug.Log("Final key used, exiting game...");
                // Implement game exit or level completion logic here
                SceneManager.LoadScene("TutorialScene");
            }
            else
            {
                SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
    }

    private void GameOver()
    {
        //Reset the players inventory to before their first entered the level
        Inventory.InstanceReference.playerInventorySO.ClearItemsThenReAdd();
        //Destroy the EV2_LevelManager instance to reset the level completely
        Destroy(FindAnyObjectByType<SpriteChecker>().gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
