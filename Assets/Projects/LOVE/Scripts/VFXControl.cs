using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXControl : MonoBehaviour
{
    [SerializeField] VisualEffect effect;
    void Start()
    {
        effect = GetComponent<VisualEffect>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            effect.Reinit();
        }
    }
}
