using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderFunctions : MonoBehaviour
{
    protected List<Vector3> CreatePlaneVertices(Vector3 spawnPosition, Vector3 spawnNormal, float planeSize, Vector2Int XYRatio)
    { 
      XYRatio = new Vector2Int(Mathf.Clamp(XYRatio.x, 1, 10000), Mathf.Clamp(XYRatio.y, 1, 10000));
      float planeSizeX = planeSize * XYRatio.x / (XYRatio.x + XYRatio.y);
      float planeSizeY = planeSize * XYRatio.y / (XYRatio.x + XYRatio.y);

      List<Vector3> tempVertices = new List<Vector3>{};

      Vector3 a = Vector3.Cross(spawnNormal.normalized, Vector3.up).normalized;

      tempVertices.Add(spawnPosition - (a * planeSizeX + Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize);
      tempVertices.Add(spawnPosition - (a * planeSizeX - Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize);
      tempVertices.Add(spawnPosition + (a * planeSizeX + Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize);
      tempVertices.Add(spawnPosition + (a * planeSizeX - Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize);

      return tempVertices;
    }   
    
    protected List<Vector3> CreatePlaneNormals(Vector3 spawnNormal)
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
   
    protected List<int> CreatePlaneIndices(bool flipIndices)
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

    protected List<Vector2> CreatePlaneUVs()
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

    protected List<Mesh> CreatePlanes(List<Vector3> spawnPoints, List<Vector3> spawnNormals, float planeSize, Vector2Int XYRatio, bool flipIndices)
    {
      List<Mesh> tempMeshes = new List<Mesh>();
      //Debug.Log(spawnPoints.Count);
      for(int i = 0; i < spawnPoints.Count; i++)
      {
        Mesh tempMesh = new Mesh { name = "Procedural Mesh no " + i };
        //FindDirection(spawnNormals[i]);
        tempMesh.vertices = CreatePlaneVertices(spawnPoints[i], spawnNormals[i], planeSize, XYRatio).ToArray();
        tempMesh.normals = CreatePlaneNormals(spawnNormals[i]).ToArray();
        tempMesh.uv = CreatePlaneUVs().ToArray();
        tempMesh.SetIndices(CreatePlaneIndices(flipIndices).ToArray(), MeshTopology.Triangles,0);
        tempMeshes.Add(tempMesh);

      }
      return tempMeshes;
    }

    protected void CombineMeshesCustom(Mesh overAllMesh, Matrix4x4 transforPosition, List<Mesh> meshes)
    {
      CombineInstance[] combineInstance = new CombineInstance[meshes.Count];

      for(int i = 0; i < meshes.Count; i++)
      {
        combineInstance[i].mesh = meshes[i];
        combineInstance[i].transform = transforPosition;
      }

      overAllMesh.CombineMeshes(combineInstance, true, true);
    }



}