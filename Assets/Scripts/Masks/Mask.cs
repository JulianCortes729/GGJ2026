using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mask : MonoBehaviour
{

    [SerializeField] protected float cooldown = 2f; //tiempo entre usos
    protected float cooldownTimer; //cuánto falta para volver a usar


    
    public bool CanUse() //para saber si se puede usar la habilidad de la mascara nuevamente
    {
        return cooldownTimer <= 0f;
    }



    
    public void Tick(float deltaTime) //reduce el cooldown
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= deltaTime;
    }

    
    public void TryUse(GameObject player) //el player ejecuta la habilidad de la mascara
    {
        if (!CanUse()) return;

        Use(player);
        cooldownTimer = cooldown;
    }


    
    protected abstract void Use(GameObject player); //cada máscara define su propio comportamiento.

}
