using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatEjemplo : MonoBehaviour
{
    [Header("Player Health (Ejemplo)")]
    [SerializeField] private float playerHealth = 100f;


    //Cuando el jugador cae fuera del ring (3D)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // El jugador cayó fuera del ring
            TriggerDefeat();
        }
    }

    //Cuando la vida del jugador llega a 0
    public void TakeDamage(float damage)
    {
        playerHealth -= damage;

        if (playerHealth <= 0)
        {
            TriggerDefeat();
        }
    }

    //Cuando el jugador es noqueado
    public void OnPlayerKnockedOut()
    {
        TriggerDefeat();
    }

    //Método público que puedes llamar desde cualquier lugar
    public void TriggerDefeat()
    {
        DefeatEvents.OnDefeat?.Invoke();
    }

    //Cuando todos los enemigos siguen en el ring y el jugador es eliminado
    public void CheckDefeatCondition()
    {
        GameObject[] enemiesInRing = GameObject.FindGameObjectsWithTag("Enemy");

        // Si hay enemigos y el jugador fue eliminado, es derrota
        if (enemiesInRing.Length > 0)
        {
            TriggerDefeat();
        }
    }
}
