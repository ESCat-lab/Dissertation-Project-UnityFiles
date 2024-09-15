using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BrushRenderer : RenderFunctions
{
    [SerializeField]
    Mesh referenceMesh;
    [SerializeField]
    Vector2Int XYRatio = new Vector2Int(3,1);
    [SerializeField]
    int brushDensity = 10;
    [SerializeField] [Range(0.1f, 1f)]
    float planeSize = 0.5f;
    [SerializeField]
    bool flipIndices = false;

    void OnEnable () 
    {
	    Mesh overAllMesh = new Mesh { name = "Combined Mesh" };
        overAllMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        MeshFilter filter = GetComponent<MeshFilter>();

        List<Vector3>[] spawnPos = CalculatePlaneSpawnPositions(referenceMesh.vertices.ToList(), referenceMesh.normals.ToList(), brushDensity);
        List<Mesh> meshes = CreatePlanes(spawnPos[0], spawnPos[1], planeSize, XYRatio, flipIndices);
        CombineMeshesCustom(overAllMesh, Matrix4x4.identity, meshes);
        filter.mesh = overAllMesh;
	}

}