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
        List<Triangle> triangles = ExtrapolateRefPlanes(referenceMesh);
        List<Vertex> spawnPos = CalculatePlaneSpawnPositions(triangles, brushDensity, seed);
        Mesh overAllMesh = CreatePlanes(spawnPos, planeSize, XYRatio, flipIndices);
        overAllMesh.name = "Combined Brush Strokes";

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = overAllMesh;
        
        Renderer renderer = GetComponent<Renderer>();
        renderer.SetSharedMaterials(materialList.ToList());
	}

}