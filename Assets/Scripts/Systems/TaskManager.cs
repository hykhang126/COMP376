using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskManager : MonoBehaviour
{

  public GameObject blackBoard;

  private TextMeshProUGUI blackBoardText;

  bool pantsPickedUp = false;
  bool shirtPickedUp = false;
  bool clothesTaskFinished = false;
  bool washHandsTaskFinished = false;
  bool makeSandwichTaskFinished = false;
  bool allTasksDone = false;

  private String sandwichText = "\r\nMAKE A SANDWICH";
  private String washHandsText = "\r\nWASH HANDS";
  private String clothesText = "\r\nPUT BEDROOM CLOTHES IN WASHER";



  public void Start()
  {
    if (blackBoard == null)
      Debug.Log("Blackboard not attached to tasks");

    blackBoardText = blackBoard.GetComponent<TextMeshProUGUI>();

    UpdateBlackBoardText();
    Inventory.sandwichEvent.AddListener(CompleteSandwichTask);
  }

  public void OnShirtPickup()
  {
    shirtPickedUp = true;
    UpdateClothesTask();
  }


  public void OnPantsPickup()
  {
    pantsPickedUp = true;
    UpdateClothesTask();
  }

  public void OnWasherInteract()
  {
    if (!pantsPickedUp || !shirtPickedUp)
    {
      Debug.Log("Washer interacted with but clothes not picked up");
    }
    else
    {
      CompleteClothesTask();
    }
  }

  public void OnSinkInteract()
  {
    Debug.Log("Sink interacted");
    washHandsTaskFinished = true;
    washHandsText = "\r\n<s>WASH HANDS</s>";
    UpdateBlackBoardText();
    washHandsTaskFinished = true;
    CheckAllTasksDone();
  }

  private void CompleteClothesTask()
  {
    clothesTaskFinished = true;
    clothesText = "\r\n<s>PUT BEDROOM CLOTHES IN WASHER</s>";
    UpdateBlackBoardText();
    clothesTaskFinished = true;
    CheckAllTasksDone();


  }

  private void CheckAllTasksDone()
  {
    if (clothesTaskFinished && washHandsTaskFinished && makeSandwichTaskFinished)
    {
      allTasksDone = true;
    }

  }

  private void UpdateClothesTask()
  {
    if (pantsPickedUp && shirtPickedUp)
    {
      Debug.Log("Clothes picked up");
    }

  }

  public void CompleteSandwichTask()
  {
    Inventory.sandwichEvent.RemoveListener(CompleteSandwichTask);
    sandwichText = "\r\n<s>MAKE A SANDWICH</s>";
    UpdateBlackBoardText();
    makeSandwichTaskFinished = true;
    CheckAllTasksDone();

  }

  private void UpdateBlackBoardText()
  {
    blackBoardText.SetText(
      "-----TODO LIST----" + sandwichText + washHandsText + clothesText + "\r\nGET GROCERIES"
      );
  }

  public void LeaveApartmentFromDoor(GameObject door)
  {
      // If all tasks done, let the existing logic handle scene switching.
      if (allTasksDone)
      {
          LeaveApartment();
          return;
      }

      AudioSource doorSource = null;
      if (door != null)
      {
          doorSource = door.GetComponent<AudioSource>() ?? door.GetComponentInChildren<AudioSource>();
      }

      if (doorSource != null && doorSource.clip != null)
      {
          doorSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
          doorSource.PlayOneShot(doorSource.clip);
      }
      else
      {
          Debug.LogWarning("LeaveApartmentFromDoor: Door AudioSource has no clip assigned.");
      }   
      
      Debug.Log("Must finish all tasks before leaving for groceries");
  }

  public void LeaveApartment()
  {
    if (!allTasksDone) return; // extra safety in case someone trie sto call this method directly

    switch (SceneManager.GetActiveScene().name)
    {
      case "TutorialScene":
        SceneManager.LoadScene("HorrorActScene1");
        break;
      case "HorrorActScene1":
        SceneManager.LoadScene("HorrorActScene2");
        break;
      case "HorrorActScene2":
        SceneManager.LoadScene("HorrorActScene3");
        break;
      case "HorrorActScene3":
        SceneManager.LoadScene("PuzzleScene");
        break;

      default:
        break;
    }
  }

}
