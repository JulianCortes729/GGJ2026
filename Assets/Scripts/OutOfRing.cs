using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfRing : MonoBehaviour
{
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DefeatEvents.OnDefeat();
        }
        if (other.CompareTag("Enemy"))
        {
            VictoryEvents.OnEnemyDefeated();
            other.gameObject.SetActive(false);
        }
    }
}
