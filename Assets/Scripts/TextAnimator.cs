using UnityEngine;
using TMPro;

public class TextAnimator : MonoBehaviour
{
    public float speed = 2f;
    public float strength = 0.1f;

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * speed) * strength;
        transform.localScale = initialScale * scale;
    }
}