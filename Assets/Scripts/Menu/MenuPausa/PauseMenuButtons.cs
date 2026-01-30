using UnityEngine;

public class PauseMenuButtons : MonoBehaviour
{
    public void ResumeGame()
    {
        PauseEvents.OnResumeGame?.Invoke();
    }

    public void RestartGame()
    {
        PauseEvents.OnRestartGame?.Invoke();
    }

    public void ReturnToMenu()
    {
        PauseEvents.OnReturnToMenu?.Invoke();
    }
}