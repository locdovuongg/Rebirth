using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float speed = 2f;
    public float life = 1f;

    TextMeshPro text;
    Vector3 dir;
    Color startColor;
    float timer;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
        dir = new Vector3(Random.Range(-0.5f, 0.5f), 1, 0);
    }

    public void Setup(string value, Color color)
    {
        if (text == null)
            text = GetComponent<TextMeshPro>();

        text.text = value;
        text.color = color;
        startColor = color;
        timer = 0f;

        Destroy(gameObject, life);
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position += dir * speed * Time.deltaTime;

        // fade out
        if (text != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / life);
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
    }
}