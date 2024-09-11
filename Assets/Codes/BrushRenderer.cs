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
    float planeSize = 0.1f;

    List<Vector3> CreatePlaneVertices(Vector3 spawnPosition, float planeSize)
    {
      List<Vector3> tempVertices = new List<Vector3>
      {
        spawnPosition + new Vector3(-1, 1, 0) * planeSize,
        spawnPosition + new Vector3(-1, -1, 0) * planeSize,
        spawnPosition + new Vector3(1, -1, 0) * planeSize,
        spawnPosition + new Vector3(1, 1, 0) * planeSize
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
    List<Mesh> CreatePlanes(List<Vector3> spawnPoints, List<Vector3> spawnNormals, float planeSize)
    {
      List<Mesh> tempMeshes = new List<Mesh>();

      for(int i = 0; i < spawnPoints.Count; i++)
      {
        Mesh tempMesh = new Mesh { name = "Procedural Mesh no " + i };
        tempMesh.vertices = CreatePlaneVertices(spawnPoints[i], planeSize).ToArray();
        tempMesh.normals = CreatePlaneNormals(spawnNormals[i]).ToArray();
        tempMeshes.Add(tempMesh);

      }
      return tempMeshes;
    }

    Mesh CombineMeshesCustom(Mesh overAllMesh, List<Mesh> meshes)
    {
      CombineInstance[] combineInstance = new CombineInstance[meshes.Count];

      for(int i = 0; i < meshes.Count; i++)
      {
        combineInstance[i].mesh = meshes[i];
      }

      overAllMesh.CombineMeshes(combineInstance, true, true);
      return overAllMesh;
    }



    void OnEnable () 
    {
		  Mesh overAllMesh = new Mesh { name = "Combined Mesh" };
      MeshFilter filter = GetComponent<MeshFilter>();
      
      List<Mesh> meshes = CreatePlanes(referenceMesh.vertices.ToList(), referenceMesh.normals.ToList(), planeSize);
      filter.mesh = CombineMeshesCustom(overAllMesh, meshes);

	  }
}