using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] [ExecuteInEditMode]
public class BrushRenderer : MonoBehaviour
{
    [SerializeField]
    Mesh referenceMesh;
    [SerializeField]
    float planeSize = 0.05f;

    List<Vector3> CreatePlaneVertices(Vector3 spawnPosition, Vector3 spawnNormal, float planeSize)
    {
      List<Vector3> tempVertices = new List<Vector3>
      {
        spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(-1, 1, 0)) * planeSize,
        spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(-1, -1, 0)) * planeSize,
        spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(1, -1, 0)) * planeSize,
        spawnPosition + Vector3.Cross(spawnNormal.normalized, new Vector3(1, 1, 0)) * planeSize
      };

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
    List<int> CreatePlaneIndices(Vector3 spawnNormal)
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

      if(spawnNormal.normalized.z < 0)
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
        tempMesh.vertices = CreatePlaneVertices(spawnPoints[i], spawnNormals[i], planeSize).ToArray();
        tempMesh.normals = CreatePlaneNormals(spawnNormals[i]).ToArray();
        tempMesh.SetIndices(CreatePlaneIndices(spawnNormals[i]).ToArray(), MeshTopology.Triangles,0);
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