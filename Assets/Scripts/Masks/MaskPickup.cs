using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskPickup : MonoBehaviour
{
    [SerializeField] private Mask maskLogic; //La lógica de la máscara
    private MeshRenderer[] visualRenderers; //Los renderers para ocultar/mostrar

    void Awake()
    {
        //Si no está asignado, buscar en este GameObject o hijos
        if (maskLogic == null)
            maskLogic = GetComponentInChildren<Mask>();

        //Cachear todos los renderers para ocultar/mostrar
        visualRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    void Start()
    {
        //Al inicio, la máscara debe estar visible
        SetVisible(true);
    }

    //Devuelve la lógica de la máscara
    public Mask GetMask()
    {
        return maskLogic;
    }

    //Muestra u oculta el pickup en el mundo (solo los renderers, no el GameObject)
    public void SetVisible(bool visible)
    {
        foreach (MeshRenderer renderer in visualRenderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }
    }

    // Getter para saber si está visible
    public bool IsVisible()
    {
        if (visualRenderers == null || visualRenderers.Length == 0) return false;
        return visualRenderers[0] != null && visualRenderers[0].enabled;
    }
}