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
    Vector3[] tempVertices;
    Vector3[] tempNormals;
    Vector2[] tempUvs;
    List<int> tempIndices;

    void OnEnable () 
    {
		    var mesh = new Mesh { name = "Procedural Mesh" };
        tempVertices = new Vector3[referenceMesh.vertices.Length * 2];
        tempNormals = new Vector3[tempVertices.Length];
        tempUvs = new Vector2[tempVertices.Length];
        tempIndices = new List<int>();

        for(int i = 0; i < referenceMesh.vertices.Length; i++)
        {
          if(i % 2 == 0)
          {
            Vector3 tempVertex = referenceMesh.vertices[i];
            Vector3 tempVertexNext = referenceMesh.vertices[(i+1)%referenceMesh.vertices.Length];

            Vector3 tangent = Vector3.Cross(tempVertex - tempVertexNext, referenceMesh.normals[i]).normalized;

            tempVertices[i] = tempVertex +  tangent/10;
            tempVertices[i + 1] = tempVertex - tangent/10;
            //Debug.Log("Vertice " + i + " & " + (i+1) + ":         " + tempVertices[i] + " , " + tempVertices[i + 1]);

            tempNormals[i] = referenceMesh.normals[i];
            tempNormals[i + 1] = referenceMesh.normals[i];
              //Debug.Log("Normal " + i + " & " + (i+1) + ":           " + tempNormals[i] + " , " + tempNormals[i + 1]);

            //tempUvs[i] = Vector3.up; 
            //tempUvs[i + 1] = Vector3.down; 
            //Debug.Log("Uv " + i + " & " + (i+1) + ":            " + tempUvs[i] + " , " + tempUvs[i + 1]);     
          }
        }  

        for(int i = 0; i < tempVertices.Length; i++)
        {
          if(i%4 == 0)
          {
            tempIndices.Add(i);
            tempIndices.Add(i + 1);
            tempIndices.Add(i + 2);

            tempIndices.Add(i + 2);
            tempIndices.Add(i + 1);
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

      Vector3 t0 = this.transform.position + tempVertices[0];
      Vector3 t1 = this.transform.position + tempVertices[1];
      Vector3 t2 = this.transform.position + tempVertices[2];
      Vector3 t3 = this.transform.position + tempVertices[3];
      //These two are positioned wrong
      Vector3 t4 = this.transform.position + tempVertices[4];
      Vector3 t5 = this.transform.position + tempVertices[5];
      //------------------------------
      Vector3 t6 = this.transform.position + tempVertices[6];
      Vector3 t7 = this.transform.position + tempVertices[7];

      Vector3 tangent = Vector3.Cross(c0 - c1, referenceMesh.normals[0]).normalized;

      Debug.DrawLine(c0, c1, color);
      Debug.DrawLine(c0, c2, color);
      Debug.DrawLine(c1, c3, color);
      Debug.DrawLine(c2, c3, color);

      //Debug.DrawLine(c0, c0 + tangent, color2);
      Debug.DrawLine(t0, t1, color2);
      Debug.DrawLine(t2, t3, color2);
      Debug.DrawLine(t4, t5, color2);
      Debug.DrawLine(t6, t7, color2);

      //Debug.Log("Tangent " + tangent);
      //Debug.Log("c0 " + c0);
    }
}