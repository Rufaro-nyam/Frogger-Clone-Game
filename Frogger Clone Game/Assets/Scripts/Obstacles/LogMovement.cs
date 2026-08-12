using UnityEngine;

public class LogMovement : MonoBehaviour
{
    [SerializeField] private int direction = 1;
    [SerializeField] private float speed = 0.5f;

    void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }
}
