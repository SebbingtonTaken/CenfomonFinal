using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;

        yield return DialogManager.Instance.ShowDialogText("Te ves cansado, quieres descansar?",
            choices: new List<string>() { "Si", "No" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            // Si
            yield return Fader.i.FadeIn(0.5f);

            var playerParty = player.GetComponent<CenfomonParty>();
            playerParty.Cenfomons.ForEach(p => p.Heal());
            playerParty.PartyUpdated();

            yield return Fader.i.FadeOut(0.5f);

            yield return DialogManager.Instance.ShowDialogText($"Tu cenfomon debería estar curado");
        }
        else if (selectedChoice == 1)
        {
            // No
            yield return DialogManager.Instance.ShowDialogText($"Okay! Vuelve si cambias de parecer");
        }


    }
}
