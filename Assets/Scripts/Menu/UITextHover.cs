using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UITextHover : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }
}

