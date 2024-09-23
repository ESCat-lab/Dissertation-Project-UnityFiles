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
        List<int> indices = referenceMesh.triangles.ToList();

        //Debug.Log("vertices count: " + vertices.Count);
        //Debug.Log("indices count: " + indices.Count);

	    Mesh overAllMesh = new Mesh { name = "Combined Mesh" };
        overAllMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        MeshFilter filter = GetComponent<MeshFilter>();

        List<Vector3>[] spawnPos = CalculatePlaneSpawnPositions(vertices, referenceMesh.normals.ToList(), indices, brushDensity, seed);
        overAllMesh = CreatePlanes(spawnPos[0], spawnPos[1], planeSize, XYRatio, flipIndices);
        filter.mesh = overAllMesh;
	}

}