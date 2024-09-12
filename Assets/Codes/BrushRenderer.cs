using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] [ExecuteInEditMode]
public class BrushRenderer : MonoBehaviour
{
    [SerializeField]
    Mesh referenceMesh;

    [SerializeField] [Range(0.001f, 10f)]
    float planeSize = 1f;
    [SerializeField] [Range(0.001f, 10f)]
    float planeSizeX = 1f;
    [SerializeField] [Range(0.001f, 10f)]
    float planeSizeY = 1f;
    [SerializeField] [Range(0, 3)]
    int planeDirection;
    [SerializeField]
    bool flipIndices = false;

    void FindDirection(Vector3 spawnNormal)
    {
      if(Vector3.Cross(spawnNormal, new Vector3(1, 0, 0)).magnitude >= 1 )
      {
        planeDirection = 0;
        Debug.DrawLine(this.gameObject.transform.position, this.gameObject.transform.position + Vector3.Cross(spawnNormal, new Vector3(1, 0, 0)).normalized, Color.red, 10.0f);
      }
      if(Vector3.Cross(spawnNormal, new Vector3(0, 1, 0)).magnitude >= 1 )
      {
        planeDirection = 1;
        Debug.DrawLine(this.gameObject.transform.position, this.gameObject.transform.position + Vector3.Cross(spawnNormal, new Vector3(0, 1, 0)).normalized, Color.green, 10.0f);
      }
      if(Vector3.Cross(spawnNormal, new Vector3(0, 0, 1)).magnitude >= 1 )
      {
        planeDirection = 2; 
        Debug.DrawLine(this.gameObject.transform.position, this.gameObject.transform.position + Vector3.Cross(spawnNormal, new Vector3(0, 0, 1)).normalized, Color.blue, 10.0f);
      }
    }

    List<Vector3> CreatePlaneVertices(int plane, Vector3 spawnPosition, Vector3 spawnNormal, float planeSizeX, float planeSizeY)
    { 
      planeSizeX *= 0.01f;
      planeSizeY *= 0.01f;

      List<Vector3> tempVertices = new List<Vector3>{};

      Vector3 a = Vector3.Cross(spawnNormal.normalized, Vector3.up).normalized;

      tempVertices.Add(spawnPosition - (a * planeSizeX + Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize);
      tempVertices.Add(spawnPosition - (a * planeSizeX - Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize);
      tempVertices.Add(spawnPosition + (a * planeSizeX + Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize);
      tempVertices.Add(spawnPosition + (a * planeSizeX - Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize);

      return tempVertices;
    }   
    List<Vector3> CreatePlaneNormals(Vector3 spawnNormal)
    {
      List<Vector3> tempNormals = new List<Vector3>()
      {
        spawnNormal,
        spawnNormal,
        spawnNormal,
        spawnNormal
      };

      return tempNormals;
    }   
    List<int> CreatePlaneIndices(int plane, Vector3 spawnNormal)
    {
      List<int> tempIndices = new List<int>()
      {
        0,
        3,
        2,
        2,
        1,
        0
      };      

      if(flipIndices)
      {
        tempIndices.Reverse();
      }
      
      return tempIndices;
    }

    List<Vector2> CreatePlaneUVs()
    {
      List<Vector2> tempUVs = new List<Vector2>()
      {
        new Vector2(0,1),
        new Vector2(0,0),
        new Vector2(1,0),
        new Vector2(1,1)
      };
      return tempUVs;
    }

    List<Mesh> CreatePlanes(List<Vector3> spawnPoints, List<Vector3> spawnNormals, float planeSizeX, float planeSizeY)
    {
      List<Mesh> tempMeshes = new List<Mesh>();
      //Debug.Log(spawnPoints.Count);
      for(int i = 0; i < spawnPoints.Count; i++)
      {
        Mesh tempMesh = new Mesh { name = "Procedural Mesh no " + i };
        //FindDirection(spawnNormals[i]);
        tempMesh.vertices = CreatePlaneVertices(planeDirection, spawnPoints[i], spawnNormals[i], planeSizeX, planeSizeY).ToArray();
        tempMesh.normals = CreatePlaneNormals(spawnNormals[i]).ToArray();
        tempMesh.uv = CreatePlaneUVs().ToArray();
        tempMesh.SetIndices(CreatePlaneIndices(planeDirection, spawnNormals[i]).ToArray(), MeshTopology.Triangles,0);
        tempMeshes.Add(tempMesh);

      }
      return tempMeshes;
    }


    void CombineMeshesCustom(Mesh overAllMesh, Matrix4x4 transforPosition, List<Mesh> meshes)
    {
      CombineInstance[] combineInstance = new CombineInstance[meshes.Count];

      for(int i = 0; i < meshes.Count; i++)
      {
        combineInstance[i].mesh = meshes[i];
        combineInstance[i].transform = transforPosition;
      }

      overAllMesh.CombineMeshes(combineInstance, true, true);
    }



    void OnEnable () 
    {
		  Mesh overAllMesh = new Mesh { name = "Combined Mesh" };
      MeshFilter filter = GetComponent<MeshFilter>();
      
      List<Mesh> meshes = CreatePlanes(referenceMesh.vertices.ToList(), referenceMesh.normals.ToList(), planeSizeX, planeSizeY);
      CombineMeshesCustom(overAllMesh, Matrix4x4.identity, meshes);
      filter.mesh = overAllMesh;
	  }

}