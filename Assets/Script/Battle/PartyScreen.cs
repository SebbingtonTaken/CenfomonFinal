using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;
    List<Cenfomon> cenfomons;
    CenfomonParty party;

    int selection = 0;

    public Cenfomon SelectedMember => cenfomons[selection];


    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        party = CenfomonParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        cenfomons = party.Cenfomons;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < cenfomons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(cenfomons[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        messageText.text = "Escoge un cenfomon";
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++selection;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --selection;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            selection += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selection -= 2;

        selection = Mathf.Clamp(selection, 0, cenfomons.Count - 1);

        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < cenfomons.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < cenfomons.Count; i++)
        {
            string message = tmItem.CanBeTaught(cenfomons[i]) ? "Puede!" : "No puede!";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < cenfomons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
