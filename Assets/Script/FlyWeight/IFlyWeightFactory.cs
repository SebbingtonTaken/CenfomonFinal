using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IFlyWeightFactory
{
    private Dictionary<string, FlyWeight> flyweights = new Dictionary<string, FlyWeight>();

    public FlyWeight GetItemFlyweight(string name, int price)
    {
        if (!flyweights.ContainsKey(name))
        {
            flyweights[name] = new FlyWeight(name, price);
        }

        return flyweights[name];
    }
}