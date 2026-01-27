using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MaskPickup : MonoBehaviour
{
    [SerializeField] private Mask maskPrefab;

    public Mask GetMaskInstance()
    {
        return Instantiate(maskPrefab);
    }
}