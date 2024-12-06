using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI nameText;
    [SerializeField] TMPro.TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TMPro.TextMeshProUGUI messageText;

    Cenfomon _cenfomon;

    public void Init(Cenfomon cenfomon)
    {
        _cenfomon = cenfomon;
        UpdateData();
        SetMessage("");

        _cenfomon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _cenfomon.Base.Name;
        levelText.text = "Lvl " + _cenfomon.Level;
        hpBar.SetHP((float)_cenfomon.HP / _cenfomon.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = GlobalSettings.i.HighlightedColor;
        else
            nameText.color = Color.black;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
