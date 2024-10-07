using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] [ExecuteInEditMode]
public class BrushRendererWithShaders : RenderFunctions
{
    [SerializeField]
    ComputeShader computeShader;
    
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

    [SerializeField] [Range(0.001f, 1f)]
    float rotation = 0.1f;

    [SerializeField]
    bool flipIndices = false;

    //SunnySunshine and Unity Technologies (2015). How Can I Read in the Actual Elements from an Append Compute Buffer? [online] Unity Discussions. 
    //Available at: https://discussions.unity.com/t/how-can-i-read-in-the-actual-elements-from-an-append-compute-buffer/147044 [Accessed 7 Oct. 2024].
    int RetrieveDataBufferSize(ComputeBuffer appendBuffer)
    {
        // Buffer to store count in.
        // You shouldn't be creating this each frame, but rather create it once
        // and then reuse it. Here solely to show its creation.
        var countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        // Copy the count.
        ComputeBuffer.CopyCount(appendBuffer, countBuffer, 0);

        // Retrieve it into array.
        int[] counter = new int[1] { 0 };
        countBuffer.GetData(counter);

        // Actual count in append buffer.
        int count = counter[0]; // <-- This is the answer

        // Don't do this every frame.
        countBuffer.Release();

        return count;
    }
    //--------------------------------------------------------------------------------------------------------------------------------------------------

    void OnEnable () 
    {
        List<Triangle> triangles = ExtrapolateRefPlanes(referenceMesh);

        int vertexStride = sizeof(float) * 3 + sizeof(float) * 3 + sizeof(float) * 2 + sizeof(int) + sizeof(float);
        int trianglesStride = vertexStride * 3 + sizeof(int) + sizeof(float);

        ComputeBuffer TrianglesBuffer = new ComputeBuffer(triangles.Count, trianglesStride);
        TrianglesBuffer.SetData(triangles.ToArray());

        ComputeBuffer spawnPointsBuffer = new ComputeBuffer( triangles.Count * brushDensity, vertexStride, ComputeBufferType.Append);
        spawnPointsBuffer.SetCounterValue(0);

        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetInt("density", brushDensity);
        computeShader.SetInt("sensitivity", sensitivity);
        computeShader.SetInt("seed", seed);
        computeShader.SetInt("triangleCount", triangles.Count);
        computeShader.SetBuffer(kernelHandle, "refTriangles", TrianglesBuffer);
        computeShader.SetBuffer(kernelHandle, "newVertices", spawnPointsBuffer);

        int threadCount = Mathf.CeilToInt(triangles.Count/8f);
        computeShader.Dispatch(kernelHandle, threadCount, 1, 1);

        //Debug.Log(RetrieveDataBufferSize(spawnPointsBuffer));
        Vertex[] spawnPos = new Vertex[RetrieveDataBufferSize(spawnPointsBuffer)];
        spawnPointsBuffer.GetData(spawnPos);

        Mesh overAllMesh = CreatePlanes(spawnPos.ToList(), planeSize, XYRatio, flipIndices, rotation);
        overAllMesh.name = "Combined Brush Strokes";

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = overAllMesh;
        
        Renderer renderer = GetComponent<Renderer>();
        renderer.SetSharedMaterials(materialList.ToList());
	}

}