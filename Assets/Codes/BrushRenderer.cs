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
    [SerializeField]
    float planeSize = 0.05f;
    [SerializeField] [Range(0, 3)]
    int planeDirection = 0;
    
    Vector3[] planeDirections = new Vector3[]
    {
      new Vector3(1, 1, 0),
      new Vector3(1, 0, 1),
      new Vector3(0, 1, 1),
      new Vector3(1, 1, 1)
    };

    int FindDirection(Vector3 spawnPosition)
    {
      float mag = 1;
      int a = 0;
      Vector3 absPosition = new Vector3(Mathf.Abs(spawnPosition.normalized.x), Mathf.Abs(spawnPosition.normalized.y), Mathf.Abs(spawnPosition.normalized.z));

      for(int i = 0; i < planeDirections.Length; i++)
      {
        float tempMag = Vector3.Distance(planeDirections[i], absPosition);
        if( tempMag < mag)
        {
          mag = tempMag;
          a = i;
        }
      }
      return a;
    }

    List<Vector3> CreatePlaneVertices(int plane, Vector3 spawnPosition, Vector3 spawnNormal, float planeSize)
    {      
      List<Vector3> tempVertices = new List<Vector3>{};

      switch(plane) 
      {
        case 0:
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(-1, 1, 0)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(-1, -1, 0)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(1, -1, 0)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(1, 1, 0)).normalized * planeSize);
          break;
        case 1:
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(1, 0, -1)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(-1, 0, -1)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(-1, 0, 1)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(1, 0, 1)).normalized * planeSize);
          break;
        case 2:
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(0, -1, 1)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(0, -1, -1)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(0, 1, -1)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(0, 1, 1)).normalized * planeSize);
          break;
        case 3:
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(1, 1, 1)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(1, 0, 1)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(0, 0, 0)).normalized * planeSize);
          tempVertices.Add(spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(0, 1, 0)).normalized * planeSize);
          break;
      }

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
        1,
        2,
        2,
        3,
        0
      };
      
      float b = 0;

      switch(plane) 
      {
        case 0:
          b = spawnNormal.normalized.z;
          break;
        case 1:
          b = spawnNormal.normalized.y;
          break;
        case 2:
          b = spawnNormal.normalized.x;
          break;
        case 3:
          b = spawnNormal.normalized.x;
          break;
      }

      if(b <= 0)
      {
        tempIndices.Reverse();
      }
      
      return tempIndices;
    }
    List<Mesh> CreatePlanes(List<Vector3> spawnPoints, List<Vector3> spawnNormals, float planeSize)
    {
      List<Mesh> tempMeshes = new List<Mesh>();

      for(int i = 0; i < spawnPoints.Count; i++)
      {
        Mesh tempMesh = new Mesh { name = "Procedural Mesh no " + i };
        //planeDirection = FindDirection(spawnPoints[i]);
        tempMesh.vertices = CreatePlaneVertices(planeDirection, spawnPoints[i], spawnNormals[i], planeSize).ToArray();
        tempMesh.normals = CreatePlaneNormals(spawnNormals[i]).ToArray();
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
      
      List<Mesh> meshes = CreatePlanes(referenceMesh.vertices.ToList(), referenceMesh.normals.ToList(), planeSize);
      CombineMeshesCustom(overAllMesh, Matrix4x4.identity, meshes);
      filter.mesh = overAllMesh;
	  }
}