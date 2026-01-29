using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    public void StartGame()
    {
        MenuEvents.OnStartGame?.Invoke();
    }

    public void OpenOptions()
    {
        MenuEvents.OnOpenOptions?.Invoke();
    }

    public void QuitGame()
    {
        MenuEvents.OnQuitGame?.Invoke();
    }
}

