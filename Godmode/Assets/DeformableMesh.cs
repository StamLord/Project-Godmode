using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeformableMesh : MonoBehaviour
{
    public float maximumDepression;
    public List<Vector3> originalVertices;
    public List<Vector3> modifiedVertices;

    public Mesh mesh;

    private void Update()
    {
        if (!mesh)
            return;
        mesh.SetVertices(modifiedVertices);
        mesh.MarkDynamic();
    }

    public void MeshRegenerated()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.MarkDynamic();
        originalVertices = mesh.vertices.ToList();
        modifiedVertices = mesh.vertices.ToList();
    }

    public void AddDepression(Vector3 depressionPoint, Vector3 collisionDown, float radius)
    {
        var worldPos = transform.InverseTransformPoint(depressionPoint);
        for (int i = 0; i < modifiedVertices.Count; ++i)
        {
            float distance = (worldPos - modifiedVertices[i]).magnitude;
            if(distance < radius)
            {
                float depr;
                if (distance <= radius / 3f)
                    depr = maximumDepression;
                else if (distance <= radius / 3f * 2f)
                    depr = maximumDepression / 3f * 2f;
                else
                    depr = maximumDepression / 3f;

                var newVert = originalVertices[i] + collisionDown * depr;
                modifiedVertices.RemoveAt(i);
                modifiedVertices.Insert(i, newVert);
            }
        }

        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        Debug.Log("Mesh Depressed");
    }

    void Start()
    {
        MeshRegenerated();
    }
}
