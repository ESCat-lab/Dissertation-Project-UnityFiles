using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class RenderFunctions : MonoBehaviour
{
  //------------------------------------Class to Contain Data of Each Triangle Corner--------------------------------------------
  protected class Triangle 
  {
    Vector3[] corners = new Vector3[3];
    Vector3[] normals = new Vector3[3];

    int subMesh;

    public Vector3[] Corners;
    public Vector3[] Normals;

    public int SubMesh;

    public Triangle(Vector3[] corners, Vector3[] normals, int subMesh)
    {
      this.corners = corners;
      this.normals = normals;
      this.subMesh = subMesh;

      Corners = corners;
      Normals = normals;
      SubMesh = subMesh;
    }

  }
  //-----------------------------------------------------------------------------------------------------------------------------
  //--------------------------------------Get the Triangles of the Reference Mesh------------------------------------------------
  protected List<Triangle> ExtrapolateRefPlanes(Mesh referenceMesh)
  {   
    List<Vector3> vertices = referenceMesh.vertices.ToList();
    List<Vector3> normals = referenceMesh.normals.ToList();
    List<int> indices = referenceMesh.triangles.ToList();        

    List<Triangle> triangles = new List<Triangle>();
    List<Vector3> pCorners = new List<Vector3>();
    List<Vector3> pNormals = new List<Vector3>();

    for(int i = 0; i < referenceMesh.subMeshCount; i++)
    {
      int start = referenceMesh.GetSubMesh(i).indexStart;
      int indiceCount = referenceMesh.GetSubMesh(i).indexCount;
          
      for(int t = 0; t < indiceCount; t++)
      {
        pCorners.Add(vertices[indices[start + t]]);
        pNormals.Add(normals[indices[start + t]]);   

        if(t % 3 == 2 && t != 0)
        {
          triangles.Add(new Triangle(pCorners.ToArray(), pNormals.ToArray(), i));
          pCorners.Clear();
          pNormals.Clear();
        }       
      }
    }               
    return triangles;
  }

  //-----------------------------------------------------------------------------------------------------------------------------
  //-------------------------------------Spawn a Few Planes on a Set of Triangles------------------------------------------------
  protected List<Vector3>[] CalculatePlaneSpawnPositions(List<Triangle> triangles, int density, int seed)
  {
    List<Vector3> spawnPoints = new List<Vector3>();
    List<Vector3> spawnNormals = new List<Vector3>();

    density = Mathf.Clamp(density, 1, 100);
    int tempSubMesh = triangles[0].SubMesh;

    foreach(Triangle triangle in triangles)
    {
      Vector3 A = triangle.Corners[0];
      Vector3 B = triangle.Corners[1];
      Vector3 C = triangle.Corners[2];

      Vector3 nA = triangle.Normals[0];
      Vector3 nB = triangle.Normals[1];
      Vector3 nC = triangle.Normals[2];

      if(triangle.SubMesh != tempSubMesh)
      {
        tempSubMesh = triangle.SubMesh;
      }

      for (int t = 0; t < density; t++)
      {
        // Generate two random values between 0 and 1
        Random.InitState(seed + t);            
        float pointDensityA = Random.Range(0f, 1f);
        float pointDensityB = Random.Range(0f, 1f);

        // Ensure the points are inside the triangle (barycentric coordinate system)
        if (pointDensityA + pointDensityB > 1f)
        {
          pointDensityA = 1f - pointDensityA;
          pointDensityB = 1f - pointDensityB;
        }

        // Compute point on the plane by linear combination of triangle vertices
        Vector3 P = A + pointDensityA * (B - A) + pointDensityB * (C - A);
        spawnPoints.Add(P);

        // Average the normals and add to the spawnNormals list
        //Vector3 N = (nA + nB + nC) / 3f; //Cross product
        Vector3 N = Vector3.Cross(B - A, C - A);
        spawnNormals.Add(N.normalized);
      }
    }
    return new List<Vector3>[2] {spawnPoints, spawnNormals};
  }

  //-----------------------------------------------------------------------------------------------------------------------------
  //-----------------------------Spawn the Vertex Positions of a Plane Centered on a Point--------------------------------------
  protected List<Vector3> CreatePlaneVertices(Vector3 spawnPosition, Vector3 spawnNormal, float planeSize, Vector2Int XYRatio)
  { 
    XYRatio = new Vector2Int(Mathf.Clamp(XYRatio.x, 1, 10000), Mathf.Clamp(XYRatio.y, 1, 10000));
    float planeSizeX = planeSize * XYRatio.x / (XYRatio.x + XYRatio.y);
    float planeSizeY = planeSize * XYRatio.y / (XYRatio.x + XYRatio.y);

    List<Vector3> tempVertices = new List<Vector3>{};

    Vector3 a = Vector3.Cross(spawnNormal.normalized, Vector3.up).normalized;
    Vector3 zOffset = spawnNormal.normalized * Random.Range(-0.01f,0.01f);

    tempVertices.Add(spawnPosition - (a * planeSizeX + Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize + zOffset);
    tempVertices.Add(spawnPosition - (a * planeSizeX - Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize + zOffset);
    tempVertices.Add(spawnPosition + (a * planeSizeX + Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize + zOffset);
    tempVertices.Add(spawnPosition + (a * planeSizeX - Vector3.Cross(spawnNormal.normalized, a).normalized * planeSizeY)* planeSize + zOffset);

    return tempVertices;
  }   
  //-----------------------------------------------------------------------------------------------------------------------------
  //-------------Return 4 Normal Vectors for Each Corners of the Plane Based on the Original Referenced Mesh Normal--------------
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
  //-----------------------------------------------------------------------------------------------------------------------------
  //---------------------------------------Return 6 Indices for Each Plane's Two Triangles---------------------------------------
  protected List<int> CreatePlaneIndices(bool flipIndices, int i)
  {
    List<int> tempIndices = new List<int>()
    {
      0 + i * 4,
      3 + i * 4,
      2 + i * 4,
      2 + i * 4,
      1 + i * 4,
      0 + i * 4
    };      

    if(flipIndices)
    {
      tempIndices.Reverse();
    }
      
    return tempIndices;
  }
  //-----------------------------------------------------------------------------------------------------------------------------
  //------Return 4 Vectors for Each Corners of the Plane's UV Coordinates. It maps to each corner corresponding to a square------
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
  //-----------------------------------------------------------------------------------------------------------------------------
  //------------------------Combines the Previous Methods to Create a Mesh Made out of a Bunch of Planes-------------------------
  protected Mesh CreatePlanes(List<Vector3> spawnPoints, List<Vector3> spawnNormals, float planeSize, Vector2Int XYRatio, bool flipIndices)
  {
    Mesh tempMesh = new Mesh();
    tempMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    
    List<Vector3> tempVertices = new List<Vector3>();
    List<Vector3> tempNormals = new List<Vector3>();
    List<Vector2> tempUvs = new List<Vector2>();
    List<int> tempIndices = new List<int>();
    //Debug.Log(spawnPoints.Count);

    for(int i = 0; i < spawnPoints.Count; i++)
    {
      if(i == 0)
      {
        tempVertices = CreatePlaneVertices(spawnPoints[i], spawnNormals[i], planeSize, XYRatio);
        tempNormals = CreatePlaneNormals(spawnNormals[i]);
        tempUvs = CreatePlaneUVs();
        tempIndices = CreatePlaneIndices(flipIndices, i);
      }
      else
      {
        tempVertices.AddRange(CreatePlaneVertices(spawnPoints[i], spawnNormals[i], planeSize, XYRatio));
        tempNormals.AddRange(CreatePlaneNormals(spawnNormals[i]));
        tempUvs.AddRange(CreatePlaneUVs());
        tempIndices.AddRange(CreatePlaneIndices(flipIndices, i));
      }
    }

    tempMesh.SetVertices(tempVertices);
    tempMesh.SetNormals(tempNormals);
    tempMesh.SetUVs(0, tempUvs);
    tempMesh.SetIndices(tempIndices.ToArray(), MeshTopology.Triangles, 0);

    return tempMesh;
  }
  //-----------------------------------------------------------------------------------------------------------------------------
  //--------------------------------Method to Combine a List of Meshes, not used anymore-----------------------------------------
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