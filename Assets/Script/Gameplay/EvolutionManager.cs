using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image cenfomonImage;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    public IEnumerator Evolve(Cenfomon cenfomon, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);

        cenfomonImage.sprite = cenfomon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{cenfomon.Base.Name} está evolucionando");

        var oldCenfomon = cenfomon.Base;
        cenfomon.Evolve(evolution);

        cenfomonImage.sprite = cenfomon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldCenfomon.Name} evolucionó en {cenfomon.Base.Name}");

        evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
}
