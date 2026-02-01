using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GeneradorCirculo : MonoBehaviour
{
    public GameObject cuboPrefab;
    public int totalCubos = 52;
    public float radio = 10f;
    public float posY = 0;

    private List<GameObject> listaDeCubos = new List<GameObject>();


    void Start()
    {
        CrearCirculoCompleto();
        EliminarCubosEspecificos();
    }

    void CrearCirculoCompleto()
    {
        float gradosPorCubo = 360f / totalCubos;

        for (int i = 0; i < totalCubos; i++)
        {
            float anguloGrados = i * gradosPorCubo;
            float anguloRad = anguloGrados * Mathf.Deg2Rad;

            Vector3 posicion = new Vector3(Mathf.Cos(anguloRad) * radio, posY, Mathf.Sin(anguloRad) * radio);

            Quaternion rotacionHaciaCentro = Quaternion.LookRotation(Vector3.zero - posicion);

            GameObject nuevoCubo = Instantiate(cuboPrefab, posicion, rotacionHaciaCentro);
            nuevoCubo.name = "Cubo_" + i;
            listaDeCubos.Add(nuevoCubo);
        }
    }

    void EliminarCubosEspecificos()
    {
        int[] indicesParaBorrar = { 0, 4, 5, 6, 13, 20, 21, 22, 26, 30, 31, 32, 39, 46, 47, 48 };

        System.Array.Sort(indicesParaBorrar);
        System.Array.Reverse(indicesParaBorrar);

        foreach (int indice in indicesParaBorrar)
        {
            if (indice >= 0 && indice < listaDeCubos.Count)
            {
                if (listaDeCubos[indice] != null)
                {
                    Destroy(listaDeCubos[indice]);
                    listaDeCubos.RemoveAt(indice);
                }
            }
        }
    }
}