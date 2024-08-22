using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BrushRenderer : MonoBehaviour
{
    [SerializeField]
    Mesh referenceMesh;

    Vector3[] tempVertexCount;
    Vector3[] tempVertices;
    Vector3[] tempNormals;
    Vector2[] tempUvs;
    List<int> tempIndices;

    void OnEnable () 
    {
		    var mesh = new Mesh { name = "Procedural Mesh" };
        tempVertexCount = new Vector3[]{referenceMesh.vertices[0], referenceMesh.vertices[1], referenceMesh.vertices[2], referenceMesh.vertices[3]};
        tempVertices = new Vector3[tempVertexCount.Length * 2];
        tempNormals = new Vector3[tempVertices.Length];
        tempUvs = new Vector2[tempVertices.Length];
        tempIndices = new List<int>();

        for(int i = 0; i < tempVertexCount.Length; i++)
        {
          if(i % 2 == 0)
          {
            Vector3 tangent = Vector3.Cross(tempVertexCount[(i+1)%tempVertexCount.Length], tempNormals[i]).normalized;

            tempVertices[i] = tempVertexCount[i] +  tangent/10;
            tempVertices[i + 1] = tempVertexCount[i] - tangent/10;
            //Debug.Log("Vertice " + i + " & " + (i+1) + ":         " + tempVertices[i] + " , " + tempVertices[i + 1]);

            tempNormals[i] = Vector3.back;
            tempNormals[i + 1] = Vector3.back;
            //Debug.Log("Normal " + i + " & " + (i+1) + ":           " + tempNormals[i] + " , " + tempNormals[i + 1]);

            //tempUvs[i] = Vector3.up; 
            //tempUvs[i + 1] = Vector3.down; 
            //Debug.Log("Uv " + i + " & " + (i+1) + ":            " + tempUvs[i] + " , " + tempUvs[i + 1]);

            tempIndices.Add(i);
            tempIndices.Add(i+1);
            tempIndices.Add(i+2);

            tempIndices.Add(i+2);
            tempIndices.Add(i+1);
            tempIndices.Add(i);
          }        
        }
        
        mesh.vertices = tempVertices;
        mesh.triangles = tempIndices.ToArray();
        mesh.normals = tempNormals;
        mesh.uv = tempUvs;

        

        GetComponent<MeshFilter>().mesh = mesh;
	}
    void FixedUpdate() 
    {
      Color color = new Color(0.0f, 0.0f, 1.0f);
      Color color2 = new Color(0.0f, 1.0f, 1.0f);
      Vector3 c0 = this.transform.position + referenceMesh.vertices[0];
      Vector3 c1 = this.transform.position + referenceMesh.vertices[1];
      Vector3 c2 = this.transform.position + referenceMesh.vertices[2];
      Vector3 c3 = this.transform.position + referenceMesh.vertices[3];

      Vector3 tangent = Vector3.Cross(c0 - c1, tempNormals[0]).normalized;

      Debug.DrawLine(c0, c1, color);
      Debug.DrawLine(c0, c2, color);
      Debug.DrawLine(c1, c3, color);
      Debug.DrawLine(c2, c3, color);

      Debug.DrawLine(c0, c0 + tangent, color2);
      //Debug.Log("Tangent " + tangent);
      //Debug.Log("c0 " + c0);
    }
}