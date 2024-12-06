using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyWeight
{
    public string itemName;
   
    public int itemPrice;

  
    public FlyWeight(string name, int price)
    {
        itemName = name;
      
        itemPrice = price;
    }
}
