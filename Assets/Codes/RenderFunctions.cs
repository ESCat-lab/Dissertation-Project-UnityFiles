using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class RenderFunctions : MonoBehaviour
{
  //--------------------------------------------Class to Contain Vertex Data-----------------------------------------------------
  protected class Vertex
  {
    Vector3 position;
    Vector3 normal;
    int subMesh;
    Vector2 uv;

    public Vector3 Position;
    public Vector3 Normal;
    public int SubMesh;
    public Vector2 Uv;

    public Vertex(Vector3 position, Vector3 normal, int subMesh)
    {
      this.position = position;
      this.normal = normal;
      this.subMesh = subMesh;
      //this.uv = uv;

      Position = position;
      Normal = normal;
      SubMesh = subMesh;
      //Uv = uv;
    }
    public Vertex(Vector3 position, Vector3 normal, Vector2 uv, int subMesh)
    {
      this.position = position;
      this.normal = normal;
      this.subMesh = subMesh;
      this.uv = uv;

      Position = position;
      Normal = normal;
      SubMesh = subMesh;
      Uv = uv;
    }
    public Vertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
      this.position = position;
      this.normal = normal;
      this.uv = uv;

      Position = position;
      Normal = normal;
      Uv = uv;
    }
    public Vertex(Vector3 position, Vector3 normal)
    {
      this.position = position;
      this.normal = normal;

      Position = position;
      Normal = normal;
    }
  }
  //-----------------------------------------------------------------------------------------------------------------------------
  //------------------------------------Class to Contain Data of Each Triangle Corner--------------------------------------------
  protected class Triangle 
  {
    Vertex[] corners = new Vertex[3];

    int subMesh;
    public Vertex[] Corners;

    public int SubMesh;

    public Triangle(Vertex[] corners, int subMesh)
    {
      this.subMesh = subMesh;
      this.corners = corners;
      Corners = corners;
      SubMesh = subMesh;
    }
  }
  //-----------------------------------------------------------------------------------------------------------------------------
  //--------------------------------------Get the Triangles of the Reference Mesh------------------------------------------------
  protected List<Triangle> ExtrapolateRefPlanes(Mesh referenceMesh)
  {   
    List<Vertex> verticesList = new List<Vertex>();
    List<Vector3> vertices = referenceMesh.vertices.ToList();
    List<Vector3> normals = referenceMesh.normals.ToList();
    List<int> indices = referenceMesh.triangles.ToList();        

    List<Triangle> triangles = new List<Triangle>();

    for(int i = 0; i < referenceMesh.subMeshCount; i++)
    {
      int start = referenceMesh.GetSubMesh(i).indexStart;
      int indiceCount = referenceMesh.GetSubMesh(i).indexCount;
          
      for(int t = 0; t < indiceCount; t++)
      {
        verticesList.Add(new Vertex(vertices[indices[start + t]], normals[indices[start + t]], i));  

        if(t % 3 == 2 && t != 0)
        {
          triangles.Add(new Triangle(verticesList.ToArray(), i));
          verticesList.Clear();
        }       
      }
    }               
    return triangles;
  }

  //-----------------------------------------------------------------------------------------------------------------------------
  //-------------------------------------Spawn a Few Planes on a Set of Triangles------------------------------------------------
  protected List<Vertex> CalculatePlaneSpawnPositions(List<Triangle> triangles, int density, int seed)
  {
    List<Vertex> spawnPoints = new List<Vertex>();

    density = Mathf.Clamp(density, 1, 100);

    foreach(Triangle triangle in triangles)
    {
      Vector3 A = triangle.Corners[0].Position;
      Vector3 B = triangle.Corners[1].Position;
      Vector3 C = triangle.Corners[2].Position;

      for (int t = 0; t < density; t++)
      {
        //Debug.Log(triangle.SubMesh);
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

        // Average the normals and add to the spawnNormals list
        //Vector3 N = (nA + nB + nC) / 3f; //Cross product
        Vector3 N = Vector3.Cross(B - A, C - A).normalized;
        spawnPoints.Add(new Vertex(P, N, Vector2.zero, triangle.SubMesh));
      }
    }
    return spawnPoints;
  }

  //-----------------------------------------------------------------------------------------------------------------------------
  //-----------------------------Spawn the Vertex Positions of a Plane Centered on a Point--------------------------------------
  protected List<Vertex> CreatePlaneVertices(Vertex spawnPoint, float planeSize, Vector2Int XYRatio, float rotation = 0.1f)
  { 
    XYRatio = new Vector2Int(Mathf.Clamp(XYRatio.x, 1, 10000), Mathf.Clamp(XYRatio.y, 1, 10000));
    planeSize = planeSize * Random.Range(0.5f, 1f);
    float planeSizeX = planeSize * XYRatio.x / (XYRatio.x + XYRatio.y);
    float planeSizeY = planeSize * XYRatio.y / (XYRatio.x + XYRatio.y);

    List<Vertex> tempVertices = new List<Vertex>{};

    Vector3 rotationOffset = Random.insideUnitSphere.normalized * rotation;
    Vector3 a = Vector3.Cross(spawnPoint.Normal.normalized, Vector3.up).normalized;
    a += rotationOffset;
    Vector3 b = Vector3.Cross(spawnPoint.Normal.normalized, a).normalized;
    Vector3 zOffset = spawnPoint.Normal.normalized * Random.Range(-0.01f,0.01f);

    Vector3 pos = spawnPoint.Position - (a * planeSizeX + b * planeSizeY)* planeSize + zOffset;
    tempVertices.Add(new Vertex(pos, spawnPoint.Normal.normalized, new Vector2(0,1), spawnPoint.SubMesh));

    pos = spawnPoint.Position - (a * planeSizeX - b * planeSizeY)* planeSize + zOffset;
    tempVertices.Add(new Vertex(pos, spawnPoint.Normal.normalized, new Vector2(0,0), spawnPoint.SubMesh));

    pos = spawnPoint.Position + (a * planeSizeX + b * planeSizeY)* planeSize + zOffset;
    tempVertices.Add(new Vertex(pos, spawnPoint.Normal.normalized, new Vector2(1,0), spawnPoint.SubMesh));

    pos = spawnPoint.Position + (a * planeSizeX - b * planeSizeY)* planeSize + zOffset;
    tempVertices.Add(new Vertex(pos, spawnPoint.Normal.normalized, new Vector2(1,1), spawnPoint.SubMesh));

    return tempVertices;
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
  //------------------------Combines the Previous Methods to Create a Mesh Made out of a Bunch of Planes-------------------------
  protected Mesh CreatePlanes(List<Vertex> spawnPoints, float planeSize, Vector2Int XYRatio, bool flipIndices, float rotation = 0.1f)
  {
    Mesh tempMesh = new Mesh();
    tempMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

    List<Vertex> tempVertices = new List<Vertex>();
    List<int> tempIndices = new List<int>();
    int submeshCount = 0;

    for(int i = 0; i < spawnPoints.Count; i++)
    {
      if(i == 0)
      {        
        tempVertices = CreatePlaneVertices(spawnPoints[i], planeSize, XYRatio, rotation);
        tempIndices = CreatePlaneIndices(flipIndices, i);
        submeshCount = spawnPoints[i].SubMesh;
      }
      else
      {
        tempVertices.AddRange(CreatePlaneVertices(spawnPoints[i], planeSize, XYRatio, rotation));
        tempIndices.AddRange(CreatePlaneIndices(flipIndices, i));
        if(submeshCount < spawnPoints[i].SubMesh)
        {
          submeshCount = spawnPoints[i].SubMesh;
        }
      }
    }

    List<Vector3> pos = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    foreach(Vertex vert in tempVertices)
    {
      pos.Add(vert.Position);
      normals.Add(vert.Normal);
      uvs.Add(vert.Uv);

    }
    tempMesh.SetVertices(pos);
    tempMesh.SetNormals(normals);
    tempMesh.SetUVs(0, uvs);
    tempMesh.subMeshCount = submeshCount + 1;

    List<int> subMeshIndices = new List<int>();
    int counter = 0;
    for(int i = 0; i < tempIndices.Count; i++)
    {
      //Debug.Log("Vertice submesh: " + tempVertices[tempIndices[i]].SubMesh);
      if(i > 0 && tempVertices[tempIndices[i - 1]].SubMesh != tempVertices[tempIndices[i]].SubMesh)
      {        
        tempMesh.SetIndices(subMeshIndices.ToArray(), MeshTopology.Triangles, counter);        
        subMeshIndices.Clear();
        counter++;
      }else if(i == tempIndices.Count - 1)
      {
        subMeshIndices.Add(tempIndices[i]);  
        tempMesh.SetIndices(subMeshIndices.ToArray(), MeshTopology.Triangles, counter);
      }

      subMeshIndices.Add(tempIndices[i]);      
    }

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