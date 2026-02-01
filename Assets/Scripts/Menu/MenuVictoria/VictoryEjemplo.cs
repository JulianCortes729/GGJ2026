using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryEjemplo : MonoBehaviour
{

    //Cuando un luchador cae fuera del ring (3D)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // El enemigo cayó fuera del ring
            CheckIfPlayerWon();
        }
    }

  

    // Verifica si el jugador es el último en el ring
    private void CheckIfPlayerWon()
    {
        // Busca todos los luchadores enemigos que quedan en el ring
        GameObject[] enemiesInRing = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemiesInRing.Length == 0)
        {
            // victoria
            TriggerVictory();
        }
    }

    //Método público que puedes llamar desde cualquier lugar
    public void TriggerVictory()
    {
        VictoryEvents.OnVictory?.Invoke();
    }

    //Activar victoria manualmente después de eliminar a todos
    public void OnAllFightersEliminated()
    {
        TriggerVictory();
    }
}

