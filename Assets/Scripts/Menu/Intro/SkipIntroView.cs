using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipIntroView : MonoBehaviour
{
    [SerializeField] private IntroVideoController controller;
    public void Skip()
    {
        controller.SkipIntro();
    }
}
