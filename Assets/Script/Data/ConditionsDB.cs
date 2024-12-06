using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Veneno",
                StartMessage = "Ha sido envenenado",
                OnAfterTurn = (Cenfomon cenfomon) =>
                {
                    cenfomon.DecreaseHP(cenfomon.MaxHp / 8);
                    cenfomon.StatusChanges.Enqueue($"{cenfomon.Base.Name} se hizo daño por el veneno");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Quemadura",
                StartMessage = "Ha sido quemado",
                OnAfterTurn = (Cenfomon cenfomon) =>
                {
                    cenfomon.DecreaseHP(cenfomon.MaxHp / 16);
                    cenfomon.StatusChanges.Enqueue($"{cenfomon.Base.Name} se hizo daño por la quemadura");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralisis",
                StartMessage = "ha sido paralizado",
                OnBeforeMove = (Cenfomon cenfomon) =>
                {
                    if  (Random.Range(1, 5) == 1)
                    {
                        cenfomon.StatusChanges.Enqueue($"{cenfomon.Base.Name} está paralizado y no puede moverse");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Congelado",
                StartMessage = "ha sido congelado",
                OnBeforeMove = (Cenfomon cenfomon) =>
                {
                    if  (Random.Range(1, 5) == 1)
                    {
                        cenfomon.CureStatus();
                        cenfomon.StatusChanges.Enqueue($"{cenfomon.Base.Name} ya no está congelado");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Dormir",
                StartMessage = "se ha dormido",
                OnStart = (Cenfomon cenfomon) =>
                {
                    // dormir 1 a 3 turnos
                    cenfomon.StatusTime = Random.Range(1, 4);
                    Debug.Log($"estará dormido por {cenfomon.StatusTime} turnos");
                },
                OnBeforeMove = (Cenfomon cenfomon) =>
                {
                    if (cenfomon.StatusTime <= 0)
                    {
                        cenfomon.CureStatus();
                        cenfomon.StatusChanges.Enqueue($"{cenfomon.Base.Name} despertó!");
                        return true;
                    }

                    cenfomon.StatusTime--;
                    cenfomon.StatusChanges.Enqueue($"{cenfomon.Base.Name} está durmiendo");
                    return false;
                }
            }
        },

        // condiciones volatiles
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "ha sido confundido",
                OnStart = (Cenfomon cenfomon) =>
                {
                    // Confusion 1 a 4 turnos
                    cenfomon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"estará confundido por {cenfomon.VolatileStatusTime} turnos ");
                },
                OnBeforeMove = (Cenfomon cenfomon) =>
                {
                    if (cenfomon.VolatileStatusTime <= 0)
                    {
                        cenfomon.CureVolatileStatus();
                        cenfomon.StatusChanges.Enqueue($"{cenfomon.Base.Name} salió de la confusión!");
                        return true;
                    }
                    cenfomon.VolatileStatusTime--;

                    // 50% chance to do a move
                    if (Random.Range(1, 3) == 1)
                        return true;

                    // Hurt by confusion
                    cenfomon.StatusChanges.Enqueue($"{cenfomon.Base.Name} está confundido");
                    cenfomon.DecreaseHP(cenfomon.MaxHp / 8);
                    cenfomon.StatusChanges.Enqueue($"se hizo daño por la confusión");
                    return false;
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}
