using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BrushRenderer : MonoBehaviour
{
    [SerializeField]
    Mesh referenceMesh;
    List<Vector3> tempVertices;
    List<Vector3> tempNormals;
    List<Vector2> tempUvs;
    List<int> tempIndices;

    void OnEnable () 
    {
		    var mesh = new Mesh { name = "Procedural Mesh" };
        tempVertices = new List<Vector3>();
        tempNormals = new List<Vector3>();
        tempUvs = new List<Vector2>();
        tempIndices = new List<int>();

        Debug.Log("Normal Amount Ref: " + referenceMesh.normals.Length);
        Debug.Log("Vertex Amount Ref: " + referenceMesh.vertices.Length);

        for(int i = 0; i < referenceMesh.vertices.Length; i++)
        {
          Vector3 tempVertex = referenceMesh.vertices[i];

          Vector3 tangent = Vector3.Cross(tempVertex - referenceMesh.vertices[(i+1)%referenceMesh.vertices.Length], referenceMesh.normals[i]).normalized;

          tempVertices.Add(tempVertex +  tangent/10);
          tempVertices.Add(tempVertex -  tangent/10);
          tempNormals.Add(referenceMesh.normals[i]); 
          tempNormals.Add(referenceMesh.normals[i]); 
        }  

      for(int i = 0; i < tempVertices.Count; i += 4)
      {
        // First triangle (i, i+1, i+2)
        tempIndices.Add(i);
        tempIndices.Add(i + 1);
        tempIndices.Add(i + 2);

        // Second triangle (i+2, i+3, i)
        tempIndices.Add(i + 2);
        tempIndices.Add(i + 3);
        tempIndices.Add(i);
      }

        Debug.Log("Normal Amount: " + tempNormals.Count);
        Debug.Log("Vertex Amount: " + tempVertices.Count);

        mesh.vertices = tempVertices.ToArray();
        mesh.triangles = tempIndices.ToArray();
        mesh.normals = tempNormals.ToArray();
        mesh.uv = tempUvs.ToArray();

        GetComponent<MeshFilter>().mesh = mesh;
	  }
    void FixedUpdate() 
    {
      Color color = new Color(0.0f, 0.0f, 1.0f);
      Color color2 = new Color(1.0f, 1.0f, 0.0f);
      
      for(int i = 0; i < referenceMesh.vertices.Length; i++)
      {
        Vector3 c0 = this.transform.position + referenceMesh.vertices[i];
        Vector3 c1 = this.transform.position + referenceMesh.vertices[(i+1)%referenceMesh.vertices.Length];

        Vector3 tangent = Vector3.Cross(c0 - c1, referenceMesh.normals[i]).normalized;
        Debug.DrawLine(c0, c0 + tangent, color);
        Debug.DrawLine(c0, c0 - tangent, color2);
      }
      
    }
}