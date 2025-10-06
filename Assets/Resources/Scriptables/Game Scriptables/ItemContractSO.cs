using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemContractSO", menuName = "Scriptable Objects/ItemContractSO")]
public class ItemContractSO : ScriptableObject
{
    [SerializeField] private String _name;
    [SerializeField] private String _description;
    [SerializeField] private Mesh _meshRef;

    [SerializeField] private Material _material;

    public String Name { get { return _name; } }
    public String Description { get { return _description; } }
    public Mesh MeshRef { get { return _meshRef; } }
    public Material Material { get { return _material; } }

    [SerializeField] private string _id = Guid.NewGuid().ToString();
    public string Id => _id;

}
