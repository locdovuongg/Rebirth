using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Reflection;
public class UIButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 originalScale;
    private float hoverScale = 1.2f;
    private float clickScale = 0.9f;
    private float speed = 10f;
    public TextMeshProUGUI text;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    private Vector3 targetScale;
    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        if(text != null)
        {
            text.color = normalColor;
        }
    }
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
    }
    public void OnPointerExit(PointerEventData eventData)
   {
      targetScale = originalScale;
      if(text != null)
        {
            text.color = normalColor;
        }
   }
   public void OnPointerEnter(PointerEventData eventData)
   {
      targetScale = originalScale * hoverScale;
      if(text != null)
        {
            text.color = hoverColor;
        }
   }
   public void OnPointerDown(PointerEventData eventData)
   {
      targetScale = originalScale * clickScale;
   
   }
   public void OnPointerUp(PointerEventData eventData)
   {
     targetScale = originalScale * hoverScale;
   }
}