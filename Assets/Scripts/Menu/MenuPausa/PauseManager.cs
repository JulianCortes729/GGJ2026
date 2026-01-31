using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    private void Update()
    {
        // La tecla P solo DISPARA el evento.
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
                PauseEvents.OnResumeGame?.Invoke();
            else
                PauseEvents.OnPauseGame?.Invoke();
        }
    }

    private void OnEnable()
    {
        // Suscribimos las acciones
        PauseEvents.OnPauseGame += HandlePause;
        PauseEvents.OnResumeGame += HandleResume;
        PauseEvents.OnRestartGame += ExecuteRestart;     
        PauseEvents.OnReturnToMenu += ExecuteReturnToMenu;
    }

    private void OnDisable()
    {
        PauseEvents.OnPauseGame -= HandlePause;
        PauseEvents.OnResumeGame -= HandleResume;
        PauseEvents.OnRestartGame -= ExecuteRestart;
        PauseEvents.OnReturnToMenu -= ExecuteReturnToMenu;
    }

    //MANEJADORES

    private void HandlePause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    private void HandleResume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    // Este método se ejecuta cuando alguien pulsa el botón "Reiniciar"
    private void ExecuteRestart()
    {
        Time.timeScale = 1f; // Siempre devolver el tiempo a 1 antes de cambiar escena
        isPaused = false;

        // Cargamos la escena (asegúrate de tener tu script SceneLoader funcionando)
        SceneLoader.LoadGameScene();
    }

    // Este método se ejecuta cuando alguien pulsa el botón "Salir al Menú"
    private void ExecuteReturnToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // Cargamos el menú
        SceneLoader.LoadMenuScene();
    }
}