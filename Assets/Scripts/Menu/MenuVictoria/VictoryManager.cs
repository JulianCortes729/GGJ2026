using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class VictoryManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryMessageText;
    [SerializeField] private GameObject victoryEffects;

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnVictory = true;
    private int enemiesToDefeat;

    private string[] mensajesVictoria = new string[]
    {
        "�CAMPE�N INDISCUTIDO, MAESTRO!",
        "�NADIE TE AGUANT� UN ROUND, CRACK!",
        "�LOS MANDASTE A TODOS A LA LONA!",
        "�LOS HICISTE PELOTA A TODOS!",
        "�TE QUEDASTE SOLO EN EL RING, �DOLO!",
        "�INVENCIBLE, MAESTRO!",
        "�NO DEJASTE NI UNO EN PIE, CRACK!",
        "�LEYENDA DEL RING, WACHO!",
        "�LES DISTE UNA PALIZA HIST�RICA!",
        "�DOMINIO TOTAL, PAP�!",
        "�LOS NOQUEASTE A TODOS, MAESTRO!"
    };

    private void Start()
    {
        enemiesToDefeat = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (victoryEffects != null) victoryEffects.SetActive(false);
    }

    private void OnEnable()
    {
        VictoryEvents.OnEnemyDefeated += ReduceEnemyCount;
        VictoryEvents.OnVictory += ShowVictoryScreen;
        VictoryEvents.OnReturnToMenuFromVictory += ReturnToMenu;
    }

    private void OnDisable()
    {
        VictoryEvents.OnEnemyDefeated -= ReduceEnemyCount;
        VictoryEvents.OnVictory -= ShowVictoryScreen;
        VictoryEvents.OnReturnToMenuFromVictory -= ReturnToMenu;
    }

    private void ReduceEnemyCount()
    {
        enemiesToDefeat--;
        if (enemiesToDefeat <= 0)
        {
            VictoryEvents.OnVictory();
        }
    }

    private void ShowVictoryScreen()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (victoryEffects != null) victoryEffects.SetActive(true);

        AudioManagerMENUS.Instance.PlaySFX(AudioManagerMENUS.Instance.bellEnd);

        // Solo controlamos UI y Tiempo. EL AUDIO VA POR EVENTOS.
        if (pauseGameOnVictory)
        {
            Time.timeScale = 0f;
        }

        if (victoryMessageText != null)
        {
            string mensajeAleatorio = mensajesVictoria[Random.Range(0, mensajesVictoria.Length)];
            victoryMessageText.text = mensajeAleatorio;
            StartCoroutine(AnimateVictoryText());
        }
    }

    private IEnumerator AnimateVictoryText()
    {
        if (victoryMessageText == null) yield break;

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        victoryMessageText.transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            victoryMessageText.transform.localScale = Vector3.Lerp(startScale, endScale, scale);
            yield return null;
        }
        victoryMessageText.transform.localScale = endScale;
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f; // Restauramos el tiempo
        SceneLoader.LoadMenuScene();
    }

    public void TriggerVictory()
    {
        VictoryEvents.OnVictory?.Invoke();
    }
}