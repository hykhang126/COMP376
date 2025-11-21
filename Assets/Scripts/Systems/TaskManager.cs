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

  public void LeaveApartment()
  {
    if (allTasksDone)
      SceneManager.LoadScene("HorrorActScene1");
    else
    {
      Debug.Log("not all tasks done");
    }
  }

}
