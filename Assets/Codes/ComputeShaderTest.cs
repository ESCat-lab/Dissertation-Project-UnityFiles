using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    [SerializeField]
    ComputeShader computeShader;

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct vertex
    {
        Vector3 position;
        Vector3 normal;
        Vector2 uv;

    }

    private const int vertexStride = sizeof(float) * (3 + 3 + 2);
    private const int indexStride = sizeof(int);

    Vector3[] refVerts;
    Vector3[] refIndices;

    // Start is called before the first frame update
    void Start()
    {
        GraphicsBuffer vertsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, refVerts.Length, vertexStride);
        GraphicsBuffer indicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, refIndices.Length, indexStride);

        int kernel = computeShader.FindKernel("CSMain");

        computeShader.SetBuffer(kernel, "_sourceVertices", vertsBuffer);
        computeShader.SetBuffer(kernel, "_sourceIndices", indicesBuffer);

        vertsBuffer.SetData(refVerts);
        indicesBuffer.SetData(refIndices);

        computeShader.GetKernelThreadGroupSizes(kernel, out uint htreadGroupSize, out _, out _);
        //int dispatchGroupSize = Mathf.CeilToInt()

        computeShader.Dispatch(kernel, 8, 8, 1);
    }
}
