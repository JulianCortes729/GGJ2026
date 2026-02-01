using UnityEngine;
using TMPro;
using System.Collections;

public class DefeatManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private TextMeshProUGUI defeatMessageText;
    [SerializeField] private GameObject defeatEffects;

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnDefeat = true;

    private string[] mensajesDerrota = new string[]
    {
        "TE SACARON DEL RING, VIEJO...",
        "TE NOQUEARON, MAESTRO",
        "NO FUE TU D�A, CAPO",
        "TE HICIERON PELOTA, BOLUDO",
        "TE CAGARON A TROMPADAS, CHE",
        "NO AGUANTASTE EL ROUND, WACHO",
        "TE ROMPIERON TODO, HERMANO",
    };

    private void Start()
    {
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (defeatEffects != null) defeatEffects.SetActive(false);
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
        if (defeatPanel != null) defeatPanel.SetActive(true);
        if (defeatEffects != null) defeatEffects.SetActive(true);

        AudioManagerMENUS.Instance.PlaySFX(AudioManagerMENUS.Instance.bellEnd);

        // Solo manejamos el tiempo aqu�. EL AUDIO LO MANEJA AUDIOMANAGER.
        if (pauseGameOnDefeat)
        {
            Time.timeScale = 0f;
        }

        if (defeatMessageText != null)
        {
            string mensajeAleatorio = mensajesDerrota[Random.Range(0, mensajesDerrota.Length)];
            defeatMessageText.text = mensajeAleatorio;
            StartCoroutine(AnimateDefeatText());
        }
    }

    private IEnumerator AnimateDefeatText()
    {
        if (defeatMessageText == null) yield break;
        float duration = 0.8f;
        float elapsed = 0f;
        Color startColor = defeatMessageText.color;
        startColor.a = 0f;
        Color endColor = defeatMessageText.color;
        endColor.a = 1f;
        defeatMessageText.color = startColor;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            defeatMessageText.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        defeatMessageText.color = endColor;
    }

    private void RetryGame()
    {
        Time.timeScale = 1f; // Importante resetear tiempo
        SceneLoader.LoadGameScene();
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f; // Importante resetear tiempo
        SceneLoader.LoadMenuScene();
    }

    public void TriggerDefeat()
    {
        DefeatEvents.OnDefeat?.Invoke();
    }
}