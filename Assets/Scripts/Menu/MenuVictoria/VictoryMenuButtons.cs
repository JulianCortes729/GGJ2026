using UnityEngine;

public class VictoryMenuButtons : MonoBehaviour
{
    public void ReturnToMenu()
    {
        VictoryEvents.OnReturnToMenuFromVictory?.Invoke();
    }

    public void RestartGame()
    {
        //Restaura el timeScale antes de reiniciar
        Time.timeScale = 1f;
        SceneLoader.LoadGameScene();
    }
}