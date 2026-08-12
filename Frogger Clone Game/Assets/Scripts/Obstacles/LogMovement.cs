using UnityEngine;

public class LogMovement : MonoBehaviour
{
    private GridManager gridManager;
    [SerializeField] private float wrapOffset = 0.1f;

    private float speed;
    private int direction;

    public void SetGridManager(GridManager manager)
    {
        gridManager = manager;
    }

    public void SetMovement(float newSpeed, int newDirection)
    {
        speed = newSpeed;
        direction = newDirection;
    }

    private void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
        CheckWrap();
    }

    private void CheckWrap()
    {
        float leftEdge = gridManager.GetWorldPosition(0, 0).x;
        float rightEdge = gridManager.GetWorldPosition(gridManager.width - 1, 0).x;

        if (direction > 0 && transform.position.x > rightEdge + wrapOffset)
        {
            transform.position = new Vector2(leftEdge - wrapOffset, transform.position.y);
        }
        else if (direction < 0 && transform.position.x < leftEdge - wrapOffset)
        {
            transform.position = new Vector2(rightEdge + wrapOffset, transform.position.y);
        }
    }
}
