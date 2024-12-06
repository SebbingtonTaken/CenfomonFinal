using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class pocionItem : Item
{
    public override void Accept(IItemVisitor visitor)
    {
        visitor.VisitPocion(this);
    }
    public PlayerController GetPlayerController()
    {
        return FindObjectOfType<PlayerController>();
    }
}
