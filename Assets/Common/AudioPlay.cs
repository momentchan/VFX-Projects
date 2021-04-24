using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] private float time = 0;
    [SerializeField] private float delay = 0;
    void Start()
    {
        source = GetComponent<AudioSource>();
        
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) {
            StopAllCoroutines();
            StartCoroutine(Play());
        }
    }
    IEnumerator Play() {
        source.Stop();
        yield return new WaitForSeconds(delay);
        source.time = time;
        source.Play();
    }
}
