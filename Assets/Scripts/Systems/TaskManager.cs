using TMPro;
using UnityEngine;

public class TaskManager : MonoBehaviour
{

  public GameObject blackBoard;

  private TextMeshProUGUI blackBoardText;
  public void Start()
  {
    if (blackBoard == null)
      Debug.Log("Blackboard not attached to tasks");

    blackBoardText = blackBoard.GetComponent<TextMeshProUGUI>();
    //blackBoardText.color = Color.green;
  }



}
