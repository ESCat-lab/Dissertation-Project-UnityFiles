using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] [ExecuteInEditMode]
public class BrushRenderer : RenderFunctions
{
    [SerializeField]
    Mesh referenceMesh;

    [SerializeField]
    Material[] materialList;

    [SerializeField]
    Vector2Int XYRatio = new Vector2Int(3,1);

    [SerializeField]
    int brushDensity = 10;

    [SerializeField]
    int seed = 42;

    [SerializeField] [Range(0.1f, 1f)]
    float planeSize = 0.5f;

    [SerializeField]
    bool flipIndices = false;

    void OnEnable () 
    {
        List<Vector3> vertices = referenceMesh.vertices.ToList();
        List<Vector3> normals = referenceMesh.normals.ToList();
        List<int> indices = referenceMesh.triangles.ToList();

        //Debug.Log("vertices count: " + vertices.Count);
        //Debug.Log("indices count: " + indices.Count);

	    Mesh overAllMesh = new Mesh { name = "Combined Mesh" };
        overAllMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        List<Vector3>[] spawnPos = CalculatePlaneSpawnPositions(vertices, normals, indices, brushDensity, seed);
        overAllMesh = CreatePlanes(spawnPos[0], spawnPos[1], planeSize, XYRatio, flipIndices);

        if(materialList.Length != 0)
        {
            overAllMesh.subMeshCount = materialList.Length;
        }else
        {
            overAllMesh.subMeshCount = 1;
        }

        int section = overAllMesh.triangles.Count() / overAllMesh.subMeshCount;

        for(int i = 0; i < overAllMesh.subMeshCount; i++)
        {
            int startPoint = Mathf.Clamp(i * section, 0, overAllMesh.triangles.Count() -1);
            int endPoint = Mathf.Clamp(startPoint + section, 0 , overAllMesh.triangles.Count() -1);
            overAllMesh.SetTriangles( overAllMesh.triangles[startPoint..], i);
        }

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = overAllMesh;
        
        Renderer renderer = GetComponent<Renderer>();
        renderer.SetSharedMaterials(materialList.ToList());
	}

}