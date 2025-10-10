using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
  public void OnHitByRay()
  {
    Debug.Log($"{name} was hit by a ray!");
    // Add your hit logic here (take damage, flash red, etc.)
  }
}
