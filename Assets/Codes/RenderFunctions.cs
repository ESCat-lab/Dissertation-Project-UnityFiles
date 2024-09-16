using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class RenderFunctions : MonoBehaviour
{
    protected class Plane 
    {
      List<Vector3> corners = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();

      public List<Vector3> Corners;
      public List<Vector3> Normals;

      public Plane(List<Vector3> corners, List<Vector3> normals)
      {
        this.corners = corners;
        this.normals = normals;

        Corners = corners;
        Normals = normals;
      }

    }

    protected List<Plane> ExtrapolateRefPlanes(List<Vector3> vertices, List<Vector3> normals, List<int> indices)
    {
        List<Plane> planes = new List<Plane>();
        List<Vector3> pCorners = new List<Vector3>();
        List<Vector3> pNormals = new List<Vector3>();       
        
        for(int i = 0; i < vertices.Count; i++)
        {
            if(i % 3 == 0 && i != 0)
            {
              planes.Add(new Plane(pCorners, pNormals));
              pCorners.Clear();
              pNormals.Clear();
            }

            pCorners.Add(vertices[indices[i]]);
            pNormals.Add(normals[indices[i]]);

        }
        return planes;
    }

    protected List<Vector3>[] CalculatePlaneSpawnPositions(List<Vector3> vertices, List<Vector3> normals, List<int> indices, int density)
    {
        List<Vector3> spawnPoints = new List<Vector3>();
        List<Vector3> spawnNormals = new List<Vector3>();
        density = Mathf.Clamp(density, 1, 100);
        int planeCount = Mathf.CeilToInt(vertices.Count / 3);        
        
        for(int i = 0; i < indices.Count; i++)
        {
            Vector3 A;
            Vector3 B;
            Vector3 C;
            
            Vector3 nA;
            Vector3 nB;
            Vector3 nC;            

            A = vertices[indices[i]];
            B = vertices[indices[(i + 1) % indices.Count]];
            C = vertices[indices[(i + 2) % indices.Count]];

            nA = normals[indices[i]];
            nB = normals[indices[(i + 1) % indices.Count]];
            nC = normals[indices[(i + 2) % indices.Count]];        

            for(int t = 1; t < density; t++)
            {
              float pointDensityA = 1f * t/density;
              float pointDensityB = (1f - pointDensityA)/density;

              float pDensityASquared = Mathf.Sqrt(pointDensityA);

              //P = (1 − √a)v1 + (a√(1 − b))v2 + (b√a)v3
              //where a,b ~ U[0,1]
              Vector3 P = (1 - pDensityASquared) * A + pDensityASquared * (1 - pointDensityB) * B + pointDensityB * pDensityASquared * C;
              spawnPoints.Add(P);
              
              Vector3 N = new Vector3((nA.x + nB.x + nC.x)/3, (nA.y + nB.y + nC.y)/3, (nA.z + nB.z + nC.z)/3).normalized;
              spawnNormals.Add(N);
              //Debug.DrawLine(A, P, Color.blue, 100f);
            }                      
        }
               
        return new List<Vector3>[2] {spawnPoints, spawnNormals};
    }

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