using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem explosionParticlePrefab;

    public void BreakBlock()
    {
        Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity, null);
        Destroy(gameObject);
    }
}
