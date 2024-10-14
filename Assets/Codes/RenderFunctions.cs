using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RenderFunctions : MonoBehaviour
{
  public int sensitivity = 1000;
  protected int scalingSensitivity = 10;
  //--------------------------------------------Class to Contain Vertex Data-----------------------------------------------------
  protected struct Vertex
  {
    public Vector3 Position;
    public Vector3 Normal;
    public int SubMesh;
    public Vector2 Uv;
    public float Size;

    public Vertex(Vector3 position, Vector3 normal, int subMesh = 0, float size = 0)
    {
      Position = position;
      Normal = normal;
      SubMesh = subMesh;
      Uv = new Vector2(0,0);
      Size = size;
    }
    public Vertex(Vector3 position, Vector3 normal, Vector2 uv, int subMesh = 0, float size = 0)
    {
      Position = position;
      Normal = normal;
      SubMesh = subMesh;
      Uv = uv;
      Size = size;
    }
  }
  //-----------------------------------------------------------------------------------------------------------------------------
  //------------------------------------Class to Contain Data of Each Triangle Corner--------------------------------------------
  protected struct Triangle 
  {
    public Vertex cornerA;
    public Vertex cornerB;
    public Vertex cornerC;

    public int SubMesh;
    public float Area;

    public Triangle(Vertex[] corners, int subMesh)
    {
      //Area = Square root of√s(s - a)(s - b)(s - c) where s is half the perimeter, or (a + b + c)/2.
      float a = (corners[0].Position - corners[1].Position).magnitude;
      float b = (corners[1].Position - corners[2].Position).magnitude;
      float c = (corners[2].Position - corners[0].Position).magnitude;
      float s = (a + b + c)/2;

      Area = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
      cornerA = corners[0];
      cornerB = corners[1];
      cornerC = corners[2];
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
      Vector3 A = triangle.cornerA.Position;
      Vector3 B = triangle.cornerB.Position;
      Vector3 C = triangle.cornerC.Position;

      int tempDensity = (density * (Mathf.FloorToInt(triangle.Area * sensitivity) + 1))/10;

      //Debug.Log(Mathf.FloorToInt(triangle.Area * sensitivity));

      for (int t = 0; t < tempDensity; t++)
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
        spawnPoints.Add(new Vertex(P, N, Vector2.zero, triangle.SubMesh, triangle.Area));
      }
    }
    return spawnPoints;
  }

  //-----------------------------------------------------------------------------------------------------------------------------
  //-----------------------------Spawn the Vertex Positions of a Plane Centered on a Point--------------------------------------
  protected List<Vertex> CreatePlaneVertices(Vertex spawnPoint, float size, Vector2Int XYRatio, float rotation = 0.1f)
  { 
    XYRatio = new Vector2Int(Mathf.Clamp(XYRatio.x, 1, 10000), Mathf.Clamp(XYRatio.y, 1, 10000));
    float planeSize = size + (spawnPoint.Size / scalingSensitivity);
    //float planeSize = (size * (spawnPoint.Size / sensitivity + 1));
    //Debug.Log(planeSize);
    planeSize = planeSize * Random.Range(0.5f, 1f);
    float planeSizeX = planeSize * XYRatio.x / (XYRatio.x + XYRatio.y);
    float planeSizeY = planeSize * XYRatio.y / (XYRatio.x + XYRatio.y);

    List<Vertex> tempVertices = new List<Vertex>{};

    Vector3 up = Vector3.up;
    if(Mathf.Abs(Vector3.Dot(up, spawnPoint.Normal)) > 0.95f)
    {
      up = Vector3.right;
    }

    Vector3 rotationOffset = Random.insideUnitSphere.normalized * rotation;
    Vector3 a = Vector3.Cross(spawnPoint.Normal.normalized, up).normalized;
    a += rotationOffset;
    Vector3 b = Vector3.Cross(spawnPoint.Normal.normalized, a).normalized;
    Vector3 zOffset = spawnPoint.Normal.normalized * Random.Range(-0.01f,0.01f);

    Vector3 pos = spawnPoint.Position - (a * planeSizeX + b * planeSizeY)* planeSize + zOffset;
    tempVertices.Add(new Vertex(pos, spawnPoint.Normal.normalized, new Vector2(0,1), spawnPoint.SubMesh, spawnPoint.Size));

    pos = spawnPoint.Position - (a * planeSizeX - b * planeSizeY)* planeSize + zOffset;
    tempVertices.Add(new Vertex(pos, spawnPoint.Normal.normalized, new Vector2(0,0), spawnPoint.SubMesh, spawnPoint.Size));

    pos = spawnPoint.Position + (a * planeSizeX + b * planeSizeY)* planeSize + zOffset;
    tempVertices.Add(new Vertex(pos, spawnPoint.Normal.normalized, new Vector2(1,0), spawnPoint.SubMesh, spawnPoint.Size));

    pos = spawnPoint.Position + (a * planeSizeX - b * planeSizeY)* planeSize + zOffset;
    tempVertices.Add(new Vertex(pos, spawnPoint.Normal.normalized, new Vector2(1,1), spawnPoint.SubMesh, spawnPoint.Size));

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
  //-----------------------------------Helper Function for Finding Smallest Divisible of a Number--------------------------------
  protected int GetSmallestDivisibleFactor(int number)
  {
    if (number <= 1) return number; // Return the number itself if <= 1
    
    for (int i = 2; i <= Mathf.Sqrt(number); i++)
    {
      if (number % i == 0)
      {
          return i; // Return the first factor found
      }
    }
    
    return number; // If no factors are found, return the number itself (it's prime)
  }

}