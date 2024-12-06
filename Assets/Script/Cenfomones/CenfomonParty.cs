using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CenfomonParty : MonoBehaviour
{
    [SerializeField] List<Cenfomon> cenfomons;

    public event Action OnUpdated;

    // public static List<Cenfomon> GetCenfomons
    // {
    //     get
    //     {
    //         return cenfomons;
    //     }

    // }
    public List<Cenfomon> Cenfomons
    {
        get
        {
            return cenfomons;
        }
        set
        {
            cenfomons = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var cenfomon in cenfomons)
        {
            cenfomon.Init();
        }
    }

    private void Start()
    {

    }

    public Cenfomon GetHealthyCenfomon()
    {
        return cenfomons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddCenfomon(Cenfomon newCenfomon)
    {
        if (cenfomons.Count < 6)
        {
            cenfomons.Add(newCenfomon);
            OnUpdated?.Invoke();
        }
        else
        {
        }
    }

    public IEnumerator CheckForEvolutions()
    {
        foreach (var cenfomon in cenfomons)
        {
            var evoution = cenfomon.CheckForEvolution();
            if (evoution != null)
            {
                yield return EvolutionManager.i.Evolve(cenfomon, evoution);
            }
        }
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }

    public static CenfomonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<CenfomonParty>();
    }
}
