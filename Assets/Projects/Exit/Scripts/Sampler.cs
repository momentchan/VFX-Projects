using System.Collections;
using System.Collections.Generic;
using mj.gist.sampling;
using UnityEngine;

public class Sampler : MonoBehaviour
{
    [SerializeField] private float stddev = 0.01f;
    [SerializeField] private int nInitials = 100; // burn-in
    [SerializeField] private int nSamples = 100;

    [SerializeField] private GameObject prefab;
    [SerializeField] private Texture2D texture;

    [SerializeField] private float sleepDuration = 0.1f;

    private MCMC2DSampler mcmc;

    void Start()
    {
        mcmc = new MCMC2DSampler(texture, stddev);
        StartCoroutine(StartChaining());
    }
    private IEnumerator StartChaining()
    {
        while (true)
        {
            if (sleepDuration <= 0f)
                yield return null;
            else
                yield return new WaitForSeconds(sleepDuration);

            foreach (var uv in mcmc.Chain(nInitials, nSamples))
            {
                var worldPos = Camera.main.ViewportToWorldPoint(uv);
                worldPos.z = 0f;

                var go = Instantiate(prefab, worldPos, Quaternion.identity);
                go.transform.parent = transform;
            }
        }
    }
}
