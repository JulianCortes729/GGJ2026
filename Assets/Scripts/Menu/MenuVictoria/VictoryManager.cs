using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VictoryManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryMessageText;
    [SerializeField] private GameObject victoryEffects; // Opcional: para partículas o efectos

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnVictory = true;

    private string[] mensajesVictoria = new string[]
    {
        "¡CAMPEÓN INDISCUTIDO, MAESTRO!",
        "¡NADIE TE AGUANTÓ UN ROUND, CRACK!",
        "¡LOS MANDASTE A TODOS A LA LONA!",
        "¡LOS HICISTE PELOTA A TODOS!",
        "¡TE QUEDASTE SOLO EN EL RING, ÍDOLO!",
        "¡INVENCIBLE, MAESTRO!",
        "¡NO DEJASTE NI UNO EN PIE, CRACK!",
        "¡LEYENDA DEL RING, WACHO!",
        "¡LES DISTE UNA PALIZA HISTÓRICA!",
        "¡DOMINIO TOTAL, PAPÁ!",
        "¡LOS NOQUEASTE A TODOS, MAESTRO!"
    };

    private void Start()
    {
        //Oculta el panel de victoria al inicio
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (victoryEffects != null)
        {
            victoryEffects.SetActive(false);
        }
    }

    private void OnEnable()
    {
        VictoryEvents.OnVictory += ShowVictoryScreen;
        VictoryEvents.OnReturnToMenuFromVictory += ReturnToMenu;
    }

    private void OnDisable()
    {
        VictoryEvents.OnVictory -= ShowVictoryScreen;
        VictoryEvents.OnReturnToMenuFromVictory -= ReturnToMenu;
    }

    private void ShowVictoryScreen()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (victoryEffects != null)
        {
            victoryEffects.SetActive(true);
        }

        // Pausa el juego si está configurado
        if (pauseGameOnVictory)
        {
            Time.timeScale = 0f;
        }

        // Muestra un mensaje aleatorio
        if (victoryMessageText != null)
        {
            string mensajeAleatorio = mensajesVictoria[Random.Range(0, mensajesVictoria.Length)];
            victoryMessageText.text = mensajeAleatorio;

            // Opcional: Animación del texto
            StartCoroutine(AnimateVictoryText());
        }
    }

    private IEnumerator AnimateVictoryText()
    {
        if (victoryMessageText == null) yield break;

        // Animación simple de escala
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        victoryMessageText.transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Usa unscaledDeltaTime porque el juego está pausado
            float t = elapsed / duration;

            //Curva de animación con rebote
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            victoryMessageText.transform.localScale = Vector3.Lerp(startScale, endScale, scale);

            yield return null;
        }

        victoryMessageText.transform.localScale = endScale;
    }

    private void ReturnToMenu()
    {
        //Restaura el timeScale antes de cambiar de escena
        Time.timeScale = 1f;
        SceneLoader.LoadMenuScene();
    }

    //Método público para activar la victoria desde otros scripts
    public void TriggerVictory()
    {
        VictoryEvents.OnVictory?.Invoke();
    }
}
