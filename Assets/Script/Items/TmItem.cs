using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Cenfomon cenfomon)
    {
        // aprender movimineto los maneja el Inventory UI, si lo aprende es true
        return cenfomon.HasMove(move);
    }

    public bool CanBeTaught(Cenfomon cenfomon)
    {
        return cenfomon.Base.LearnableByItems.Contains(move);
    }

    public override bool IsReusable => isHM;

    public override bool CanUseInBattle => false;

    public MoveBase Move => move;
    public bool IsHM => isHM;
}
