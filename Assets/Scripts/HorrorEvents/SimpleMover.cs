using UnityEngine;

public class SimpleMover : MonoBehaviour
{
  public Transform pointB;
  public float speed = 6f;
  private bool shouldMove = false;

  void Update()
  {
    if (shouldMove)
    {
      transform.position = Vector3.MoveTowards(
          transform.position,
          pointB.position,
          speed * Time.deltaTime
      );

      if (Vector3.Distance(transform.position, pointB.position) < 0.05f)
      {
        Despawn();
      }
    }
  }

  public void StartMoving()
  {
    shouldMove = true;
  }

  private void Despawn()
  {
    Destroy(gameObject);
  }
}
