using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DefeatManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private TextMeshProUGUI defeatMessageText;
    [SerializeField] private GameObject defeatEffects; // Opcional: para efectos visuales

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnDefeat = true;

    private string[] mensajesDerrota = new string[]
    {
        "TE SACARON DEL RING, VIEJO...",
        "TE NOQUEARON, MAESTRO",
        "NO FUE TU DÍA, CAPO",
        "TE HICIERON PELOTA, BOLUDO",
        "TE CAGARON A TROMPADAS, CHE",
        "NO AGUANTASTE EL ROUND, WACHO",
        "TE ROMPIERON TODO, HERMANO",
    };

    private void Start()
    {
        // Oculta el panel de derrota al inicio
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        if (defeatEffects != null)
        {
            defeatEffects.SetActive(false);
        }
    }

    private void OnEnable()
    {
        DefeatEvents.OnDefeat += ShowDefeatScreen;
        DefeatEvents.OnRetryFromDefeat += RetryGame;
        DefeatEvents.OnReturnToMenuFromDefeat += ReturnToMenu;
    }

    private void OnDisable()
    {
        DefeatEvents.OnDefeat -= ShowDefeatScreen;
        DefeatEvents.OnRetryFromDefeat -= RetryGame;
        DefeatEvents.OnReturnToMenuFromDefeat -= ReturnToMenu;
    }

    private void ShowDefeatScreen()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        if (defeatEffects != null)
        {
            defeatEffects.SetActive(true);
        }

        // Pausa el juego si está configurado
        if (pauseGameOnDefeat)
        {
            Time.timeScale = 0f;
        }

        // Muestra un mensaje aleatorio
        if (defeatMessageText != null)
        {
            string mensajeAleatorio = mensajesDerrota[Random.Range(0, mensajesDerrota.Length)];
            defeatMessageText.text = mensajeAleatorio;

            // Opcional: Animación del texto
            StartCoroutine(AnimateDefeatText());
        }
    }

    private IEnumerator AnimateDefeatText()
    {
        if (defeatMessageText == null) yield break;

        // Animación de fade in
        float duration = 0.8f;
        float elapsed = 0f;

        Color startColor = defeatMessageText.color;
        startColor.a = 0f;
        Color endColor = defeatMessageText.color;
        endColor.a = 1f;

        defeatMessageText.color = startColor;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Usa unscaledDeltaTime porque el juego está pausado
            float t = elapsed / duration;

            defeatMessageText.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        defeatMessageText.color = endColor;
    }

    private void RetryGame()
    {
        // Restaura el timeScale antes de reintentar
        Time.timeScale = 1f;
        SceneLoader.LoadGameScene();
    }

    private void ReturnToMenu()
    {
        // Restaura el timeScale antes de cambiar de escena
        Time.timeScale = 1f;
        SceneLoader.LoadMenuScene();
    }

    // Método público para activar la derrota desde otros scripts
    public void TriggerDefeat()
    {
        DefeatEvents.OnDefeat?.Invoke();
    }
}