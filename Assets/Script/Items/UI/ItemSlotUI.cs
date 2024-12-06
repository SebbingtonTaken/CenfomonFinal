using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI nameText;
    [SerializeField] TMPro.TextMeshProUGUI countText;

    RectTransform rectTransform;
    private void Awake()
    {

    }

    public TMPro.TextMeshProUGUI NameText => nameText;
    public TMPro.TextMeshProUGUI CountText => countText;

    public float Height => rectTransform.rect.height;

    public void SetData(ItemSlot itemSlot)
    {
        rectTransform = GetComponent<RectTransform>();
        nameText.text = itemSlot.Item.Name;
        countText.text = $"X {itemSlot.Count}";
    }

    public void SetNameAndPrice(ItemBase item)
    {
        rectTransform = GetComponent<RectTransform>();
        nameText.text = item.Name;
        countText.text = $"$ {item.Price}";
    }
}
