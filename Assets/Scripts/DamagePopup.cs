using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float speed = 2f;
    public float lifetime = 1f;

    TextMeshPro text;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    public void Setup(string value, Color color)
    {
        text.text = value;
        text.color = color;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
    }
}