using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FractalGenerator : MonoBehaviour
{
    public Material mat;
    public RenderTexture fractTex;

    void Update()
    {
        mat.SetPass(0);
        Graphics.Blit(null, fractTex, mat);
        
    }
}
