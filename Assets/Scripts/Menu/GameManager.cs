using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private void Awake()
    {
        // Asegura una única instancia del GameManager
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        MenuEvents.OnStartGame += StartGame;
        MenuEvents.OnQuitGame += QuitGame;
    }

    private void OnDisable()
    {
        MenuEvents.OnStartGame -= StartGame;
        MenuEvents.OnQuitGame -= QuitGame;
    }

    private void StartGame()
    {
        SceneLoader.LoadGameScene();
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
