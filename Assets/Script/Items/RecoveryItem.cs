using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Condiciones de estado")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revivir")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Cenfomon cenfomon)
    {
        // Revivir
        if (revive || maxRevive)
        {
            if (cenfomon.HP > 0)
                return false;

            if (revive)
                cenfomon.IncreaseHP(cenfomon.MaxHp / 2);
            else if (maxRevive)
                cenfomon.IncreaseHP(cenfomon.MaxHp);

            cenfomon.CureStatus();

            return true;
        }

        // no se pueden usar en cenfomones derrotados
        if (cenfomon.HP == 0)
            return false;

        // Restaurar HP
        if (restoreMaxHP || hpAmount > 0)
        {
            if (cenfomon.HP == cenfomon.MaxHp)
                return false;

            if (restoreMaxHP)
                cenfomon.IncreaseHP(cenfomon.MaxHp);
            else
                cenfomon.IncreaseHP(hpAmount);
        }

        // restaurar estado
        if (recoverAllStatus || status != ConditionID.none)
        {
            if (cenfomon.Status == null && cenfomon.VolatileStatus == null)
                return false;

            if (recoverAllStatus)
            {
                cenfomon.CureStatus();
                cenfomon.CureVolatileStatus();
            }
            else
            {
                if (cenfomon.Status.Id == status)
                    cenfomon.CureStatus();
                else if (cenfomon.VolatileStatus.Id == status)
                    cenfomon.CureVolatileStatus();
                else
                    return false;
            }
        }

        // Restaurar PP
        if (restoreMaxPP)
        {
            cenfomon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if (ppAmount > 0)
        {
            cenfomon.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }

        return true;
    }
}
