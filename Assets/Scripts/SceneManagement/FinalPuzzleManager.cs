using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalPuzzleManager : MonoBehaviour
{
    [SerializeField] private ItemContractSO finalKeyItemSO;
    
    public void CheckExitCondition()
    {
        if(Inventory.InstanceReference != null)
        {
            if(Inventory.InstanceReference.items.Count > 0 && Inventory.InstanceReference.items[Inventory.InstanceReference.GetCurrentItemIndex()].Id == finalKeyItemSO.Id)
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
}
