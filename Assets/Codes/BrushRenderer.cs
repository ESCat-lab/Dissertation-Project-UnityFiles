using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BrushRenderer : MonoBehaviour
{
    [SerializeField]
    Mesh referenceMesh;
    void OnEnable () 
    {
		var mesh = new Mesh { name = "Procedural Mesh" };
        //mesh.vertices = new Vector3[] { Vector3.zero, Vector3.right, Vector3.up };
        Vector3[] tempVertices = new Vector3[referenceMesh.vertexCount * 2];
        Vector3[] tempNormals = new Vector3[tempVertices.Length];
        Vector2[] tempUvs = new Vector2[tempVertices.Length];
        int[] tempIndices = new int[tempVertices.Length/4 * 6];

        for(int i = 0; i < referenceMesh.vertexCount; i++)
        {
          if(i % 2 == 0)
          {

            Vector3 tangent;

            Vector3 c1 = Vector3.Cross(referenceMesh.normals[i], new Vector3(0.0f, 0.0f, 1.0f));
            Vector3 c2 = Vector3.Cross(referenceMesh.normals[i], new Vector3(0.0f, 1.0f, 0.0f));

            if( c1.magnitude > c2.magnitude )
            {
              tangent = c1;
            }
            else
            {
              tangent = c2;
            }

            tempVertices[i] = referenceMesh.vertices[i] +  tangent.normalized/10;
            tempVertices[i + 1] = referenceMesh.vertices[i] - tangent.normalized/10;
            Debug.Log("Vertice " + i + " & " + (i+1) + ":         " + tempVertices[i] + " , " + tempVertices[i + 1]);

            tempNormals[i] = referenceMesh.normals[i];
            tempNormals[i + 1] = referenceMesh.normals[i];
            //Debug.Log("Normal " + i + " & " + (i+1) + ":           " + tempNormals[i] + " , " + tempNormals[i + 1]);

            tempUvs[i] = Vector3.up; 
            tempUvs[i + 1] = Vector3.down; 
            //Debug.Log("Uv " + i + " & " + (i+1) + ":            " + tempUvs[i] + " , " + tempUvs[i + 1]);
            
          }        
        }

        for(int i = 0; i < tempIndices.Length / 3; i++)
        {
          if(i % 2 == 0)
          {
            tempIndices[i] = i;
            tempIndices[i + 1] = i + 1;
            tempIndices[i + 2] = i + 2;
          }
          else
          {
            tempIndices[i + 2] = i;
            tempIndices[i + 1] = i + 1;
            tempIndices[i] = i + 2;
          }
          Debug.Log("Indices " + i + " & " + (i+1) + " & " + (i+2) + ":        " + tempIndices[i] + " , " + tempIndices[i + 1] + " , "+ tempIndices[i + 2]);
        }

        
        mesh.vertices = tempVertices;
        mesh.triangles = tempIndices;
        mesh.normals = tempNormals;
        mesh.uv = tempUvs;

        

        GetComponent<MeshFilter>().mesh = mesh;
	}
}