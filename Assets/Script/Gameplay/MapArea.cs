using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Cenfomon> wildCenfomons;

    public Cenfomon GetRandomWildCenfomon()
    {
        var wildCenfomon = wildCenfomons[Random.Range(0, wildCenfomons.Count)];
        wildCenfomon.Init();
        return wildCenfomon;
    }
}
