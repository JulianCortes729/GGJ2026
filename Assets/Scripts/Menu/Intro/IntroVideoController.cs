using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class IntroVideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float delayAfterVideo = 0.5f;

    private bool hasFinished;

    private void Awake()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void Start()
    {
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (hasFinished) return;
        hasFinished = true;

        Invoke(nameof(LoadMenu), delayAfterVideo);
    }

    public void SkipIntro()
    {
        if (hasFinished) return;

        hasFinished = true;
        videoPlayer.Stop();
        LoadMenu();
    }

    private void LoadMenu()
    {
        SceneLoader.LoadMenuScene();
    }
}
