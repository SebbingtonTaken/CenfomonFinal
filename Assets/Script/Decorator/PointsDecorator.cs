using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsDecorator : IDecorator
{
    private int _moneyToAdd;

    public PointsDecorator(int moneyToAdd)
    {
    }

    public void ApplyDecorator(PlayerController player)
    {
        player.ApplyMoney(_moneyToAdd);
       
    }
}
