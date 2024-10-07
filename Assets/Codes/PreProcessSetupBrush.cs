using System;
using UnityEngine;

public class BrushPreProcess : MonoBehaviour
{
    [SerializeField]
    ComputeShader preProcessBrushCompute;

    [SerializeField]
    Mesh referenceMesh;

    [SerializeField]
    Material geometryMaterial;

    [SerializeField]
    Vector2Int XYRatio = new Vector2Int(3, 1);

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

    private ComputeBuffer verticesBuffer;
    private ComputeBuffer normalsBuffer;
    private ComputeBuffer indicesBuffer;
    private ComputeBuffer constantBuffer;
    private ComputeBuffer newVerticesBuffer; // Add a buffer for the generated vertices

    byte vector3Size = sizeof(float) * 3;
    byte floatSize = sizeof(float);
    byte intSize = sizeof(int);

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

    private void Start()
    {
        int kernelID = preProcessBrushCompute.FindKernel("GenerateVertices");

        // Initialize constants
        Constants[] constants = { new Constants(XYRatio, brushDensity, seed, planeSize, rotation, flipIndices) };
        int constantStride = intSize * 5 + floatSize * 2;

        // Create and set data for buffers
        verticesBuffer = new ComputeBuffer(referenceMesh.vertices.Length, vector3Size);
        normalsBuffer = new ComputeBuffer(referenceMesh.normals.Length, vector3Size);
        indicesBuffer = new ComputeBuffer(referenceMesh.triangles.Length, intSize);
        constantBuffer = new ComputeBuffer(1, constantStride);

        verticesBuffer.SetData(referenceMesh.vertices);
        normalsBuffer.SetData(referenceMesh.normals);
        indicesBuffer.SetData(referenceMesh.triangles);
        constantBuffer.SetData(constants);

        // Create and initialize the `newVertices` buffer (AppendStructuredBuffer)
        int maxVertexCount = 10000; // Adjust based on expected number of vertices
        newVerticesBuffer = new ComputeBuffer(maxVertexCount, vector3Size + vector3Size + intSize + 2 * floatSize, ComputeBufferType.Append);
        newVerticesBuffer.SetCounterValue(0); // Reset counter for the append buffer

        // Set buffers in the compute shader
        preProcessBrushCompute.SetBuffer(kernelID, "refVertices", verticesBuffer);
        preProcessBrushCompute.SetBuffer(kernelID, "refNormals", normalsBuffer);
        preProcessBrushCompute.SetBuffer(kernelID, "refIndices", indicesBuffer);
        preProcessBrushCompute.SetBuffer(kernelID, "constants", constantBuffer);
        preProcessBrushCompute.SetBuffer(kernelID, "newVertices", newVerticesBuffer); // Set the new buffer

        // Dispatch the compute shader for each submesh
        int subMeshCount = referenceMesh.subMeshCount;
        for (int i = 0; i < subMeshCount; i++)
        {
            int triangleCount = referenceMesh.GetSubMesh(i).indexCount / 3;
            preProcessBrushCompute.Dispatch(kernelID, Mathf.CeilToInt(triangleCount / 256.0f), 1, 1);
        }

        // Link the `newVertices` buffer to the material (optional)
        geometryMaterial.SetBuffer("newVertices", newVerticesBuffer);
    }

    private void OnDestroy()
    {
        // Release the compute buffers
        verticesBuffer?.Release();
        normalsBuffer?.Release();
        indicesBuffer?.Release();
        constantBuffer?.Release();
        newVerticesBuffer?.Release(); // Release the new buffer
    }
}