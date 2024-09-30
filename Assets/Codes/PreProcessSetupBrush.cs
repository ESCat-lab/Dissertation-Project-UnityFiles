using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class BrushPreProcess : RenderFunctions
{
    [SerializeField]
    ComputeShader preProcessBrushCompute;

    [SerializeField]
    Mesh referenceMesh;

    [SerializeField]
    Material geometryMaterial;

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

    protected struct Constants
    {
        int X;
        int Y;
        int brushDensity;
        int seed;
        float planeSize;
        float rotation;
        int flipIndices;

        public Constants(Vector2Int XYRatio, int brushDensity, int seed, float planeSize, float rotation, bool flipIndices)
        {
            X = XYRatio.x;
            Y = XYRatio.y;
            this.brushDensity = brushDensity;
            this.seed = seed;
            this.planeSize = planeSize;
            this.rotation = rotation;
            this.flipIndices = Convert.ToInt32(flipIndices);
        }
    }

    byte vector3Size = sizeof(float) * 3;
    byte floatSize = sizeof(float);
    byte intSize = sizeof(int);

    private ComputeBuffer verticesBuffer;
    private ComputeBuffer normalsBuffer;
    private ComputeBuffer indicesBuffer;
    private ComputeBuffer constantBuffer;

    private void Start()
    {
        int kernelID = preProcessBrushCompute.FindKernel("GenerateVertices");  

        Constants[] constants = {new Constants(XYRatio, brushDensity, seed, planeSize, rotation, flipIndices)};
        int subMeshCount = referenceMesh.subMeshCount;

        //int vertexStride = vector3Size + vector3Size + intSize + vector2Size + floatSize;
        //int triangleStride = vertexStride * 3 + floatSize + intSize;
        int constantStride = intSize * 5 + floatSize * 2;

        verticesBuffer = new ComputeBuffer(referenceMesh.vertices.Length, vector3Size); // float3 -> 3 * 4 bytes
        normalsBuffer = new ComputeBuffer(referenceMesh.normals.Length, vector3Size); // float3 -> 3 * 4 bytes
        indicesBuffer = new ComputeBuffer(referenceMesh.triangles.Length, intSize); // float3 -> 3 * 4 bytes
        constantBuffer = new ComputeBuffer(1, constantStride); // float3 -> 3 * 4 bytes

        verticesBuffer.SetData(referenceMesh.vertices);
        normalsBuffer.SetData(referenceMesh.normals);
        indicesBuffer.SetData(referenceMesh.triangles);
        constantBuffer.SetData(constants);

        // Set the buffers in the compute shader
        preProcessBrushCompute.SetBuffer(kernelID, "refVertices", verticesBuffer);
        preProcessBrushCompute.SetBuffer(kernelID, "refNormals", normalsBuffer);
        preProcessBrushCompute.SetBuffer(kernelID, "refIndices", indicesBuffer);
        preProcessBrushCompute.SetBuffer(kernelID, "constants", constantBuffer);

        int subMeshes = subMeshCount;
        if(subMeshCount == 1)
        {
            subMeshes = GetSmallestDivisibleFactor(subMeshCount / 3);
        }
        
        for(int i = 0; i < subMeshes; i++)
        {

            int triangleCount = referenceMesh.GetSubMesh(i).indexCount / 3;
            // Dispatch the compute shader
            preProcessBrushCompute.Dispatch(kernelID, triangleCount / 256, 1, 1);
        }

        // Link the buffers to the geometry material
        //geometryMaterial.SetBuffer("triangles", verticesBuffer);
    }

    private void OnDestroy()
    {
        // Release buffers
        verticesBuffer.Release();
        normalsBuffer.Release();
        indicesBuffer.Release();
        constantBuffer.Release();
    }
}
