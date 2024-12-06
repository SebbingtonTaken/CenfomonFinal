using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [System.Serializable]
    public class ItemConfiguration
    {
        public string itemName;
        public int itemPrice;
    }

    public ItemConfiguration[] itemConfigurations;

    private static Dictionary<string, FlyWeight> itemFlyweights = new Dictionary<string, FlyWeight>();

    private void Awake()
    {
        foreach (var config in itemConfigurations)
        {
            if (!itemFlyweights.ContainsKey(config.itemName))
            {
                FlyWeight flyweight = new FlyWeight(config.itemName, config.itemPrice);
                itemFlyweights.Add(config.itemName, flyweight);
            }
        }
    }

    public FlyWeight GetItemFlyweight(string itemName)
    {
        if (itemFlyweights.ContainsKey(itemName))
        {
            return itemFlyweights[itemName];
        }
        else
        {
            return null;
        }
    }
}



