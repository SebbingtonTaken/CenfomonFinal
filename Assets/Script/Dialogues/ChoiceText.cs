using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceText : MonoBehaviour
{
    TMPro.TextMeshProUGUI text;
    private void Awake()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void SetSelected(bool selected)
    {
        text.color = (selected) ? GlobalSettings.i.HighlightedColor : Color.black;
    }

    public TMPro.TextMeshProUGUI TextField => text;
}
