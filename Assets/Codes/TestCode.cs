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

    private void Update()
    {
        mat.SetColor("MyColor", color);      

    }
}

