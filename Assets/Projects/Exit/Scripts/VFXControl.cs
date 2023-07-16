using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXControl : MonoBehaviour
{
    [SerializeField] private VisualEffect graph;

    void Start()
    {
        graph = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            graph.Reinit();
        }
    }
    IEnumerator Anim()
    {
        yield return null;
        while (true)
        {


        }
    }
}
