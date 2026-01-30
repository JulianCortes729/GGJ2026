using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    private bool isPaused = false;

    private void Start()
    {
        // Asegura que el menú de pausa esté oculto al inicio
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Detecta la tecla P para pausar/despausar
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void OnEnable()
    {
        PauseEvents.OnPauseGame += PauseGame;
        PauseEvents.OnResumeGame += ResumeGame;
        PauseEvents.OnRestartGame += RestartGame;
        PauseEvents.OnReturnToMenu += ReturnToMenu;
    }

    private void OnDisable()
    {
        PauseEvents.OnPauseGame -= PauseGame;
        PauseEvents.OnResumeGame -= ResumeGame;
        PauseEvents.OnRestartGame -= RestartGame;
        PauseEvents.OnReturnToMenu -= ReturnToMenu;
    }

    private void PauseGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneLoader.LoadGameScene();
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneLoader.LoadMenuScene();
    }
}