using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
  [ContextMenu("Combine Meshes")]
  void CombineMeshes()
  {
    MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

    CombineInstance[] combine = new CombineInstance[meshFilters.Length];

    int i = 0;
    while (i < meshFilters.Length)
    {
      combine[i].mesh = meshFilters[i].sharedMesh;
      combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
      i++;
    }

    // Create new mesh
    Mesh combinedMesh = new Mesh();
    combinedMesh.CombineMeshes(combine, false); // false = keep materials

    // Assign it to a new meshfilter
    MeshFilter mf = gameObject.AddComponent<MeshFilter>();
    mf.sharedMesh = combinedMesh;

    // Mesh renderer for materials
    MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
    mr.sharedMaterials = GetComponentsInChildren<MeshRenderer>()[0].sharedMaterials;

    // Optional: disable children
    foreach (Transform child in transform)
      child.gameObject.SetActive(false);

    Debug.Log("Mesh Combined!");
  }
}
