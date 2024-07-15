using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

// struct of two bytes
public struct TwoBytes
{
    public byte a;
    public byte b;
}

[ExecuteInEditMode]
public class TestCode : MonoBehaviour
{

    [SerializeField]
    Color color;

    [SerializeField]
    Material mat;

    //static int numObjects = 1;
    private void Update()
    {
        // c#
        // create compute buffer

        //int stride = sizeOf(typeof(Color));

       // ComputeBuffer cb = new ComputeBuffer(numObjects, stride, ComputeBufferType.Default);
        // 2 bytes                                                   
        // important: numObjects needs to be an even number

        // create array of bytes
        //half4[] data = new half4[numObjects];

        mat.SetColor("MyColor", color);

        //for (int x = 0; x < numObjects; x++)
        //{
            //Mathf.FloatToHalf(color.g);
            //Mathf.FloatToHalf(color.b);
            //Mathf.FloatToHalf(color.a);
            //data[x] = new half4(rcolor, color.g, color.b, color.a);
        //}

        // copy data in array into the compute buffer
        //cb.SetData(data);

        // pass it to the shader calling SetBuffer() where appropriate

        //mat.SetBuffer("MyColor", cb);

    }
}

