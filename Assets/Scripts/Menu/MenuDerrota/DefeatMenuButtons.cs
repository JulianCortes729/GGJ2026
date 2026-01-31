using UnityEngine;

public class DefeatMenuButtons : MonoBehaviour
{
    public void RetryGame()
    {
        DefeatEvents.OnRetryFromDefeat?.Invoke();
    }

    public void ReturnToMenu()
    {
        DefeatEvents.OnReturnToMenuFromDefeat?.Invoke();
    }
}