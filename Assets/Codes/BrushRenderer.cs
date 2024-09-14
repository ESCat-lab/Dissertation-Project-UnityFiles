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
    Vector2Int brushDensity = new Vector2Int(5,5);
    [SerializeField] [Range(0.1f, 1f)]
    float planeSize = 0.5f;
    [SerializeField]
    bool flipIndices = false;

    void OnEnable () 
    {
	    Mesh overAllMesh = new Mesh { name = "Combined Mesh" };
        MeshFilter filter = GetComponent<MeshFilter>();

        List<Vector3> spawnPos = CalculatePlaneSpawnPositions(referenceMesh.vertices.ToList(), referenceMesh.GetIndices(0).ToList(), brushDensity);

        List<Mesh> meshes = CreatePlanes(spawnPos, referenceMesh.normals.ToList(), planeSize, XYRatio, flipIndices);
        CombineMeshesCustom(overAllMesh, Matrix4x4.identity, meshes);
        filter.mesh = overAllMesh;
	}

}