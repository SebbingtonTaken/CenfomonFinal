using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenfomonGiver : MonoBehaviour, ISavable
{
    [SerializeField] Cenfomon cenfomonToGive;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GiveCenfomon(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        cenfomonToGive.Init();
        player.GetComponent<CenfomonParty>().AddCenfomon(cenfomonToGive);

        used = true;

        string dialogText = $"{player.Name} recibió {cenfomonToGive.Base.Name}";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return cenfomonToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
