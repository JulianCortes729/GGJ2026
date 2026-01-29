using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MaskType 
{
    None,
    RedMask,
    GreenMask,
    BlueMask
}

//Sistema de ventajas para combate 
public static class MaskAdvantageSystem
{
    // RedMask gana a GreenMask
    // GreenMask gana a BlueMask
    // BlueMask gana a RedMask

    
    //Determina si maskA tiene ventaja sobre maskB
    public static bool HasAdvantage(MaskType maskA, MaskType maskB)
    {
        if (maskA == MaskType.None || maskB == MaskType.None)
            return false;

        return (maskA == MaskType.RedMask && maskB == MaskType.GreenMask) ||
               (maskA == MaskType.GreenMask && maskB == MaskType.BlueMask) ||
               (maskA == MaskType.BlueMask && maskB == MaskType.RedMask);
    }

    
    //Determina si maskA tiene desventaja contra maskB
    public static bool HasDisadvantage(MaskType maskA, MaskType maskB)
    {
        return HasAdvantage(maskB, maskA);
    }

    //Devuelve la máscara que gana contra targetMask
    public static MaskType GetCounterMask(MaskType targetMask)
    {
        switch (targetMask)
        {
            case MaskType.RedMask:
                return MaskType.BlueMask;
            case MaskType.GreenMask:
                return MaskType.RedMask;
            case MaskType.BlueMask:
                return MaskType.GreenMask;
            default:
                return MaskType.None;
        }
    }


    //Devuelve el multiplicador de daño basado en la relación de máscaras
    public static float GetDamageMultiplier(MaskType attackerMask, MaskType defenderMask)
    {
        if (HasAdvantage(attackerMask, defenderMask))
            return 2.0f; // El doble de daño con ventaja
        else if (HasDisadvantage(attackerMask, defenderMask))
            return 0.5f; // Mitad de daño con desventaja
        else
            return 1.0f; // Daño normal
    }

    //Compara dos máscaras y devuelve un string descriptivo
    public static string GetRelationship(MaskType maskA, MaskType maskB)
    {
        if (maskA == MaskType.None || maskB == MaskType.None)
            return "Unknown";

        if (HasAdvantage(maskA, maskB))
            return "Advantage";
        else if (HasDisadvantage(maskA, maskB))
            return "Disadvantage";
        else
            return "Neutral";
    }
}